using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;

namespace PPool.ChatSDK
{
	public class TextSegment
	{
		public string text;
		public List<TextStyle> styles = new List<TextStyle>();
		public Entity entity;
	}

	public class TextStyle
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public StyleCode type;
		public string hexCode;
	}

	public class Entity
	{
		public int type;
		public string value;
	}

	public enum StyleCode
	{
		ST, // Strikethrough
		DL, // Deleted
		EM, // Emphasis (Italic)
		CL, // Color
		BR  // Line Break
	}

	public class FormattedMessage
	{
		public List<TextSegment> segments { get; set; } = new List<TextSegment>();

		public string ToJson()
		{
			var root = new
			{
				segments = this.segments
			};

			return JsonConvert.SerializeObject(
				root,
				Formatting.None,
				new JsonSerializerSettings
				{
					StringEscapeHandling = StringEscapeHandling.Default,
					NullValueHandling = NullValueHandling.Ignore
				});
		}

		public bool IsPlain()
		{
			if (segments == null || segments.Count == 0)
				return true;

			foreach (var segment in segments)
			{
				if (segment.entity != null)
					return false;

				if (segment.styles.Any(s =>
					s.type == StyleCode.CL ||  
					s.type == StyleCode.BR ||  
					s.type == StyleCode.ST || 
					s.type == StyleCode.DL))  
				{
					return false;
				}
			}

			return true;
		}

		public string RawText()
		{
			if (segments == null || segments.Count == 0)
				return string.Empty;

			return string.Join("", segments.Select(s => s.text));
		}

		public Dictionary<string, object> ToDictionary()
		{
			return MiniJSON.Json.Deserialize(ToJson()) as Dictionary<string, object>;
		}

		public static FormattedMessage FromMiniJson(Dictionary<string, object> root)
		{
			//var root = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
			//var content = root["content"] as Dictionary<string, object>;
			//var segmentList = content["segments"] as List<object>;

			var segmentList = root["segments"] as List<object>;

			var message = new FormattedMessage();

			foreach (var segObj in segmentList)
			{
				var segDict = segObj as Dictionary<string, object>;
				var segment = new TextSegment
				{
					text = segDict["text"] as string
				};

				// styles
				if (segDict.TryGetValue("styles", out object stylesObj))
				{
					var stylesList = stylesObj as List<object>;
					foreach (var styleObj in stylesList)
					{
						var styleDict = styleObj as Dictionary<string, object>;
						var textStyle = new TextStyle();

						if (styleDict.TryGetValue("type", out object typeVal))
						{
							if (System.Enum.TryParse(typeVal.ToString(), out StyleCode styleCode))
								textStyle.type = styleCode;
						}

						if (styleDict.TryGetValue("hexCode", out object hexVal))
						{
							string hexCode = hexVal.ToString();
							int start = hexCode.IndexOf('#');
							if (start < 0)
								hexCode = hexCode.Insert(0, "#");

							textStyle.hexCode = hexCode;
						}

						segment.styles.Add(textStyle);
					}
				}

				// entity
				if (segDict.TryGetValue("entity", out object entityObj))
				{
					var entityDict = entityObj as Dictionary<string, object>;
					segment.entity = new Entity
					{
						type = System.Convert.ToInt32(entityDict["type"]),
						value = entityDict["value"] as string
					};
				}

				message.segments.Add(segment);
			}

			return message;
		}

		public string ToHtml()
		{
			var sb = new StringBuilder();

			foreach (var segment in segments)
			{
				if (segment.styles.Any(s => s.type == StyleCode.BR))
				{
					sb.Append("<br>");
					continue;
				}

				string text = segment.text;
				var styledText = ApplyStyles(text, segment.styles);

				if (segment.entity != null && segment.entity.type == 0)
				{
					sb.Append($"<link=\"{segment.entity.value}\">{styledText}</link>");
				}
				else
				{
					sb.Append(styledText);
				}
			}

			return sb.ToString();
		}

		private static string ApplyStyles(string text, List<TextStyle> styles)
		{
			string result = text;

			// COLOR (안쪽부터)
			var colorStyle = styles.FirstOrDefault(s => s.type == StyleCode.CL);
			if (colorStyle != null)
				result = $"<color={colorStyle.hexCode}>{result}</color>";

			if (styles.Any(s => s.type == StyleCode.ST))
				result = $"<b>{result}</b>";

			if (styles.Any(s => s.type == StyleCode.EM))
				result = $"<i>{result}</i>";

			if (styles.Any(s => s.type == StyleCode.DL))
				result = $"<s>{result}</s>";

			return result;
		}
	}

	public static class HtmlToFormattedMessageParser
	{
		private static readonly Regex tagRegex = new Regex(@"(<[^>]+>|[^<]+)", RegexOptions.Compiled);

		public static FormattedMessage Parse(string html)
		{
			var message = new FormattedMessage();
			var tagStack = new Stack<string>();
			string currentLink = null;

			foreach (Match match in tagRegex.Matches(html))
			{
				string token = match.Value;

				if (token.StartsWith("<"))
				{
					if (token.StartsWith("</"))
					{
						string tagName = token.Substring(2, token.Length - 3).ToLower();
						tagStack.Pop();
						if (tagName == "link") currentLink = null;
					}
					else
					{
						if (token.StartsWith("<link="))
						{
							int start = token.IndexOf('"') + 1;
							int end = token.LastIndexOf('"');
							currentLink = token.Substring(start, end - start);
							tagStack.Push("link");
						}
						else if (token.StartsWith("<color="))
						{
							int start = token.IndexOf('#') + 1;
							int end = token.IndexOf('>', start);
							string hex = token.Substring(start, end - start);
							tagStack.Push("color#" + hex);
						}
						else
						{
							string tag = token.Substring(1, token.Length - 2).ToLower();
							tagStack.Push(tag);
						}
					}
				}
				else if (token.Equals("<br>", StringComparison.OrdinalIgnoreCase))
				{
					message.segments.Add(new TextSegment
					{
						text = "",
						styles = new List<TextStyle> { new TextStyle { type = StyleCode.BR } }
					});
				}
				else
				{
					// 텍스트 노드 처리 (분리하여 segments 생성)
					var segments = SplitSpecialTokens(token, tagStack, currentLink);
					message.segments.AddRange(segments);
				}
			}

			return message;
		}

		private static List<TextSegment> SplitSpecialTokens(string rawText, Stack<string> tagStack, string currentLink)
		{
			var segments = new List<TextSegment>();

			// 순차적 토큰 파싱 (정규식으로 세밀하게 분해)
			int index = 0;
			while (index < rawText.Length)
			{
				if (rawText[index] == '@' || rawText[index] == '#')
				{
					char prefix = rawText[index];
					int start = index;
					index++; // skip '@' or '#'

					while (index < rawText.Length && Char.IsLetterOrDigit(rawText[index]))
						//while (index < rawText.Length && IsAsciiLetterOrDigit(rawText[index]))
						index++;

					string keyword = rawText.Substring(start, index - start); // @user1 or #tag

					var segment = new TextSegment
					{
						text = keyword,
						styles = GetCurrentStyles(tagStack)
					};

					if (prefix == '@')
						segment.entity = new Entity { type = 1, value = keyword.Substring(1) };
					else
						segment.entity = new Entity { type = 2, value = keyword.Substring(1) };

					// 링크 적용
					if (!string.IsNullOrEmpty(currentLink) && segment.entity == null)
						segment.entity = new Entity { type = 0, value = currentLink };

					segments.Add(segment);
				}
				else
				{
					int start = index;
					while (index < rawText.Length && rawText[index] != '@' && rawText[index] != '#')
						index++;

					string text = rawText.Substring(start, index - start);
					if (!string.IsNullOrEmpty(text))
					{
						var segment = new TextSegment
						{
							text = text,
							styles = GetCurrentStyles(tagStack)
						};

						if (!string.IsNullOrEmpty(currentLink))
							segment.entity = new Entity { type = 0, value = currentLink };

						segments.Add(segment);
					}
				}
			}

			return segments;
		}

		private static List<TextStyle> GetCurrentStyles(Stack<string> tagStack)
		{
			var list = new List<TextStyle>();
			foreach (string tag in tagStack)
			{
				if (tag == "b" || tag == "u" || tag == "strike")
					list.Add(new TextStyle { type = StyleCode.ST });
				else if (tag == "i")
					list.Add(new TextStyle { type = StyleCode.EM });
				else if (tag == "s")
					list.Add(new TextStyle { type = StyleCode.DL });
				else if (tag.StartsWith("color#"))
					list.Add(new TextStyle { type = StyleCode.CL, hexCode = tag.Substring(6) });
			}
			return list;
		}

		private static bool IsAsciiLetterOrDigit(char c)
		{
			return (c >= 'a' && c <= 'z') ||
				   (c >= 'A' && c <= 'Z') ||
				   (c >= '0' && c <= '9');
		}
	}
}