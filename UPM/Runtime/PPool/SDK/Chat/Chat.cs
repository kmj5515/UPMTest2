using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Text.RegularExpressions;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using Unity.VisualScripting;

namespace PPool.ChatSDK
{
	public enum Environment
	{
		dev = 0,
		qa,
		qa2,
		sandbox,
		live
	}

	public enum ChatRoomType
	{
		GROUP,
		PERSONAL
	}

	public enum MessageType
	{
		TEXT,
		IMAGE,
		EMOJI,
		EVENT,
		FILE,
		SYSTEM,
	}

	public enum MessageStatus
	{
		SENDING,
		SENT,
		UNSENT,
	}

	public enum ChatEventType
	{
		RoomCreated,
		RoomInvited,
		RoomUpdated,
		RoomDeleted
	}

	public enum DisconnectionReason
	{
		UNSTABLE_NETWORK,
		USER_INITIATED
	}

	public enum FailureReason
	{
		AUTH_EXPIRED,
		SERVER
	}

	public class Result
	{
		public ChatError Error { get; }
		public bool IsSuccess { get; }

		public Result(Dictionary<string, object> dict)
		{
			if (dict.ContainsKey("result"))
				IsSuccess = (bool)dict["result"];

			if (dict.ContainsKey("error"))
			{
				var value = dict["error"] as Dictionary<string, object>;

				string type = value.GetValue("type", "");
				string message = value.GetValue("message", "");
				switch (type)
				{
					case "NotInitialized": Error = new NotInitializedError(message); break;
					case "ConnectionFailed": Error = new ConnectionFailedError(message); break;
					case "AuthenticationDenied": Error = new AuthenticationDeniedError(message); break;
					case "PermissionDenied": Error = new PermissionDeniedError(message); break;
					case "NotParticipated": Error = new NotParticipatedError(message); break;
					case "RoomNotFound": Error = new RoomNotFoundError(message); break;
					case "BanWords": Error = new BanWordsError(message); break;
					case "Server": Error = new ServerError(message); break;
					case "NotConnected": Error = new NotConnected(message); break;
					default: Debug.LogError($"ChatError {type} {message}"); break;
				}
			}

			if (IsSuccess == false)
			{
				Debug.LogError(Error?.Message);
			}
		}
	}

	public class ChatCredential
	{
		public string userId;
		public string token;

		public ChatCredential(string userId, string token)
		{
			this.userId = userId;
			this.token = token;
		}
	}

	public class Result<T> : Result
	{
		public T Value { get; set; }

		public Result(Dictionary<string, object> dict) : base(dict) {}
	}

	public abstract class ChatError
	{
		public string Message { get; private set; }

		public ChatError(string message)
		{
			Message = message;
		}
	}

	public class NotInitializedError : ChatError
	{
		public NotInitializedError(string message) : base(message) { }
	}
	public class ConnectionFailedError : ChatError
	{
		public ConnectionFailedError(string message) : base(message) { }
	}
	public class AuthenticationDeniedError : ChatError
	{
		public AuthenticationDeniedError(string message) : base(message) { }
	}
	public class PermissionDeniedError : ChatError
	{
		public PermissionDeniedError(string message) : base(message) { }
	}
	public class NotParticipatedError : ChatError
	{
		public NotParticipatedError(string message) : base(message) { }
	}
	public class RoomNotFoundError : ChatError
	{
		public RoomNotFoundError(string message) : base(message) { }
	}
	public class BanWordsError : ChatError
	{
		public BanWordsError(string message) : base(message) { }
	}
	public class ServerError : ChatError
	{
		public ServerError(string message) : base(message) { }
	}
	public class NotConnected : ChatError
	{
		public NotConnected(string message) : base(message) { }
	}


	public struct ChatConfig
	{
		public Environment Environment { get; }
		public string Target { get; }

		public ChatConfig(Environment environment, string target)
		{
			Environment = environment;
			Target = target;
		}

		private static ChatConfig Create(string methodName, string target)
		{
			if (Enum.TryParse<Environment>(methodName, ignoreCase: true, out var env))
			{
				return new ChatConfig(env, target);
			}

			throw new ArgumentException($"Invalid environment name: {methodName}");
		}

		public static ChatConfig Dev(string target) => Create(nameof(Dev), target);
		public static ChatConfig Qa(string target) => Create(nameof(Qa), target);
		public static ChatConfig Qa2(string target) => Create(nameof(Qa2), target);
		public static ChatConfig Sandbox(string target) => Create(nameof(Sandbox), target);
		public static ChatConfig Live(string target) => Create(nameof(Live), target);
	}

	public class ChatUser
	{
		public string UserId { get; protected set; }
		public string Nickname { get; protected set; }
		public string ProfileUrl { get; protected set; }

		public ChatUser(Dictionary<string, object> dict)
		{
			UserId = dict.GetValue("userId", "");
			Nickname = dict.GetValue("nickname", "");
			ProfileUrl = dict.GetValue("profileUrl", "");
		}

		public string ToJson()
		{
			return Json.Serialize(ToDictionary());
		}

		public Dictionary<string, object> ToDictionary()
		{
			var dict = new Dictionary<string, object>
			{
				{ "userId", UserId },
				{ "nickname", Nickname },
				{ "profileUrl", ProfileUrl },
			};

			return dict;
		}
	}

	public class ChatRoom
	{
		public string Id { get; private set; }
		public string Title { get; private set; }
		public List<ChatUser> Users { get; private set; }
		public int UserCount { get; private set; }
		public string Thumbnail { get; private set; }
		public long UpdatedAt { get; private set; }
		public int LastSeq { get; private set; }
		public int ReadSeq { get; private set; }
		public string Preview { get; private set; }
		public bool IsAlarmEnabled { get; private set; }
		public ChatRoomType Type { get; private set; }
		public Dictionary<string, ChatRoomSession> RoomSessionDic { get; private set; } = new Dictionary<string, ChatRoomSession>();

		public ChatRoom(Dictionary<string, object> dict)
		{
			Id = dict.GetValue("roomId", "");
			Title = dict.GetValue("title", "");
			UserCount = dict.GetValue("userCount", 0);
			Thumbnail = dict.GetValue("thumbnail", "");
			UpdatedAt = dict.GetValue("updatedAt", 0L);
			LastSeq = dict.GetValue("lastSeq", 0);
			ReadSeq = dict.GetValue("readSeq", 0);
			Preview = dict.GetValue("preview", "");
			IsAlarmEnabled = dict.GetValue("isAlarmEnabled", false);
			Type = (ChatRoomType)dict.GetValue("type", 0);

			Users = new List<ChatUser>();
			List<object> userList = dict["users"] as List<object>;
			for (int i = 0; i < userList.Count; i++)
			{
				var userDict = userList[i] as Dictionary<string, object>;
				ChatUser user = new ChatUser(userDict);
				Users.Add(user);
			}
		}

		public bool IsPersonal()
		{
			return Type == ChatRoomType.PERSONAL;
		}
	}

	public class ChatRoomOption
	{
		public List<string> UserIds { get; private set; }
		public string Title { get; private set; }
		public string ProfileUrl { get; private set; }
		public ChatRoomType Type { get; private set; }

		public ChatRoomOption(List<string> userIds, string title, string profileUrl, ChatRoomType type)
		{
			UserIds = userIds ?? new List<string>();
			Title = title;
			ProfileUrl = profileUrl;
			Type = type;
		}

		public string ToJson()
		{
			var dict = new Dictionary<string, object>
			{
				{ "userIds", UserIds },
				{ "title", Title },
				{ "profileUrl", ProfileUrl },
				{ "type", (int)Type }
			};

			return Json.Serialize(dict);
		}
	}

	public class ChatMessage
	{
		public string RoomId { get; private set; }
		public MessageType Type { get; private set; }
		public int Seq { get; private set; }
		public FormattedMessage Content { get; private set; }
		public long Date { get; private set; }
		public ChatUser Sender { get; private set; }
		public MessageStatus Status { get; private set; }
		public string ClientId { get; private set; }

		public ChatMessage(Dictionary<string, object> dict)
		{
			RoomId = dict.GetValue("roomId", "");
			Type = (MessageType)dict.GetValue("type", 0);
			Seq = dict.GetValue("seq", 0);
			Content = FormattedMessage.FromMiniJson(dict["content"] as Dictionary<string, object>);
			Date = dict.GetValue("date", 0L);
			Sender = new ChatUser(dict["sender"] as Dictionary<string, object>);
			Status = (MessageStatus)dict.GetValue("status", 0);
			ClientId = dict.GetValue("clientId", "");
		}

		public string ToJson()
		{
			var dict = new Dictionary<string, object>
			{
				{ "roomId", RoomId },
				{ "type", (int)Type },
				{ "seq", Seq },
				{ "content", Content.ToDictionary() },
				{ "date", Date },
				{ "sender", Sender.ToDictionary() },
				{ "status", (int)Status },
				{ "clientId", ClientId }
			};

			return Json.Serialize(dict);
		}
	}

	public class ChatRoomSession
	{
		public string RoomId { get; private set; }

		public ChatRoomSession(string roomId)
		{
			RoomId = roomId;
		}

		public void SendMessage(string msg, Action<Result> callback)
		{
			FormattedMessage formattedMessage = HtmlToFormattedMessageParser.Parse(msg);
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result result = new Result(value);
				callback?.Invoke(result);
			});
			ChatInterface.SendMessage(RoomId, formattedMessage.ToJson(), identifier);
		}

		public void ResendMessage(ChatMessage chatMessage, Action<Result> callback)
		{
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result result = new Result(value);
				callback?.Invoke(result);
			});
			ChatInterface.ResendMessage(RoomId, chatMessage.ToJson(), identifier);
		}

		public void FetchLatestMessages(int count, Action<Result<List<ChatMessage>>> callback)
		{
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result<List<ChatMessage>> result = new Result<List<ChatMessage>>(value);

				if(result.IsSuccess)
				{
					result.Value = CreateMessages(value);
				}

				callback?.Invoke(result);
			});
			ChatInterface.FetchLatestMessages(RoomId, count, identifier);
		}

		public void FetchPreviousMessages(int count, int before, Action<Result<List<ChatMessage>>> callback)
		{
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result<List<ChatMessage>> result = new Result<List<ChatMessage>>(value);

				if (result.IsSuccess)
				{
					result.Value = CreateMessages(value);
				}

				callback?.Invoke(result);
			});
			ChatInterface.FetchPreviousMessages(RoomId, count, before, identifier);
		}

		public void FetchNextMessages(int count, int since, Action<Result<List<ChatMessage>>> callback)
		{
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result<List<ChatMessage>> result = new Result<List<ChatMessage>>(value);

				if (result.IsSuccess)
				{
					result.Value = CreateMessages(value);
				}

				callback?.Invoke(result);
			});
			ChatInterface.FetchNextMessages(RoomId, count, since, identifier);
		}

		public void FetchUnsentMessages(Action<Result<List<ChatMessage>>> callback)
		{
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result<List<ChatMessage>> result = new Result<List<ChatMessage>>(value);

				if (result.IsSuccess)
				{
					result.Value = CreateMessages(value);
				}

				callback?.Invoke(result);
			});
			ChatInterface.FetchUnsentMessages(RoomId, identifier);
		}

		public void DeleteUnsentMessage(string messageId, Action<Result> callback)
		{
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result result = new Result(value);
				callback?.Invoke(result);
			});
			ChatInterface.DeleteUnsentMessage(RoomId, messageId, identifier);
		}

		string registerChatEventIdentifier = string.Empty;
		public void RegisterChatRoomEvent(Action<object> callback)
		{
			registerChatEventIdentifier = PPoolSDK.Instance.AddAction(value =>
			{
				if(value.ContainsKey("MessageReceived"))
				{
					var msg = value["MessageReceived"] as Dictionary<string, object>;
					ChatMessage chatMessage = new ChatMessage(msg);
					callback?.Invoke(chatMessage);
				}
				else if(value.ContainsKey("RoomUpdated"))
				{
					var room = value["RoomUpdated"] as Dictionary<string, object>;
					ChatRoom chatRoom = new ChatRoom(room);
					callback?.Invoke(chatRoom);
				}
			});
			ChatInterface.RegisterChatRoomEvent(RoomId, registerChatEventIdentifier);
		}

		public void UnregisterChatRoomEvent()
		{
			ChatInterface.UnregisterChatRoomEvent(RoomId);
			PPoolSDK.Instance.RemoveAction(registerChatEventIdentifier);
			registerChatEventIdentifier = string.Empty;
		}

		private List<ChatMessage> CreateMessages(Dictionary<string, object> dict)
		{
			List<ChatMessage> list = new List<ChatMessage>();

			if (dict.ContainsKey("messages"))
			{
				List<object> messages = dict["messages"] as List<object>;

				for (int i = 0; i < messages.Count; i++)
				{
					var msg = messages[i] as Dictionary<string, object>;
					ChatMessage chatMessage = new ChatMessage(msg);
					list.Add(chatMessage);
				}
			}

			return list;
		}
	}

	public class ChatEvent
	{
		public ChatEventType Type { get; private set; }
		public ChatRoom Room { get; private set; }
		public string RoomId { get; private set; }

		public ChatEvent(Dictionary<string, object> dict)
		{
			Type = (ChatEventType)dict.GetValue("type", 0);
			RoomId = dict.GetValue("roomId", "");

			if (dict.ContainsKey("room"))
			{
				Room = new ChatRoom(dict["room"] as Dictionary<string, object>);
				RoomId = Room.Id;
			}
		}
	}

	public abstract class ConnectionStatus
	{
	}

	public class Connected : ConnectionStatus
	{		
	}

	public class Disconnected : ConnectionStatus
	{
		public string Reason { get; private set; }

		public Disconnected(Dictionary<string, object> dict)
		{
			Reason = dict.GetValue("reason", "");
		}
	}

	public class Reconnecting : ConnectionStatus
	{
	}

	public class ConnectionFailed : ConnectionStatus
	{
		public string Reason { get; private set; }
		public int Code { get; private set; }
		public string Message { get; private set; }

		public ConnectionFailed(Dictionary<string, object> dict)
		{
			Reason = dict.GetValue("reason", "");
			Code = dict.GetValue("code", 0);
			Message = dict.GetValue("message", "");
		}
	}

	public class ChatSDK
    {
		static bool isInit = false;
		static string connectionEventIdentifier = string.Empty;
		static string chatEventIdentifier = string.Empty;

		public static void Initialize(ChatConfig config)
		{
			if (isInit == false)
			{
				isInit = true;

				ChatInterface.Initialize((int)config.Environment, config.Target);
			}
		}

		public static void Connect(ChatCredential chatCredential, Action<Result> callback)
		{
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result result = new Result(value);
				callback?.Invoke(result);
			});
			ChatInterface.Connect(chatCredential, identifier);
		}

		public static void Disconnect()
		{
			ChatInterface.Disconnect();
		}

		public static void Logout()
		{
			ChatInterface.Logout();
		}

		public static void FetchRooms(Action<Result<List<ChatRoom>>> callback)
		{
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result<List<ChatRoom>> result = new Result<List<ChatRoom>>(value);

				if (result.IsSuccess && value.ContainsKey("rooms"))
				{
					result.Value = new List<ChatRoom>();

					var roomList = value.GetValue("rooms", new List<object>());

					foreach (var item in roomList)
					{
						var roomDict = item as Dictionary<string, object>;
						if (roomDict != null)
						{
							var room = new ChatRoom(roomDict);
							result.Value.Add(room);
						}
					}
				}

				callback?.Invoke(result);
			});
			ChatInterface.FetchRooms(identifier);
		}

		public static void FetchRoom(string roomId, Action<Result<ChatRoom>> callback)
		{
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result<ChatRoom> result = new Result<ChatRoom>(value);

				if(result.IsSuccess)
				{
					if (value.ContainsKey("room"))
					{
						result.Value = new ChatRoom(value["room"] as Dictionary<string, object>);
					}
				}

				callback?.Invoke(result);
			});
			ChatInterface.FetchRoom(roomId, identifier);
		}

		public static void Enter(string roomId, Action<Result<ChatRoomSession>> callback)
		{
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result<ChatRoomSession> result = new Result<ChatRoomSession>(value);
				
				if (result.IsSuccess)
				{
					result.Value = new ChatRoomSession(roomId);
				}

				callback?.Invoke(result);
			});
			ChatInterface.Enter(roomId, identifier);
		}

		public static void CreateRoom(ChatRoomOption chatRoomOption, Action<Result<ChatRoom>> callback)
		{
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result<ChatRoom> result = new Result<ChatRoom>(value);
				
				if (result.IsSuccess)
				{
					if (value.ContainsKey("room"))
					{
						result.Value = new ChatRoom(value["room"] as Dictionary<string, object>);
					}
				}

				callback?.Invoke(result);
			});
			ChatInterface.CreateRoom(chatRoomOption.ToJson(), identifier);
		}

		public static void Invite(string roomId, List<string> userIds, Action<Result> callback)
		{
			string userIdsJson = Json.Serialize(userIds);
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result result = new Result(value);
				callback?.Invoke(result);
			});
			ChatInterface.Invite(roomId, userIdsJson, identifier);
		}

		public static void SetAlarm(string roomId, bool alarm, Action<Result> callback)
		{
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result result = new Result(value);
				callback?.Invoke(result);
			});
			ChatInterface.SetAlarm(roomId, alarm, identifier);
		}

		public static void Leave(string roomId, Action<Result> callback)
		{
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result result = new Result(value);
				callback?.Invoke(result);
			});
			ChatInterface.Leave(roomId, identifier);
		}

		public static void Exit(string roomId, Action<Result> callback)
		{
			string identifier = PPoolSDK.Instance.AddAction(value =>
			{
				Result result = new Result(value);
				callback?.Invoke(result);
			});
			ChatInterface.Exit(roomId, identifier);
		}

		public static void UpdateCredential(ChatCredential chatCredential)
		{
			ChatInterface.UpdateCredential(chatCredential);
		}

		public static void RegisterChatEvent(Action<ChatEvent> callback)
		{
			if (string.IsNullOrEmpty(chatEventIdentifier) == false)
				return;

			chatEventIdentifier = PPoolSDK.Instance.AddAction(value =>
			{
				ChatEvent chatEvent = new ChatEvent(value);
				callback?.Invoke(chatEvent);
			});
			ChatInterface.RegisterChatEvent(chatEventIdentifier);
		}

		public static void UnregisterChatEvent()
		{
			ChatInterface.UnregisterChatEvent();

			PPoolSDK.Instance.RemoveAction(chatEventIdentifier);
			chatEventIdentifier = string.Empty;
		}

		public static ConnectionStatus GetConnectionStatus()
		{
			string jsonData = ChatInterface.GetConnectionStatus();
			var value = Json.Deserialize(jsonData) as Dictionary<string, object>;
			return CreateConnectionStatus(value);
		}

		public static void RegisterConnectionStatus(Action<ConnectionStatus> callback)
		{
			if (string.IsNullOrEmpty(connectionEventIdentifier) == false)
				return;

			connectionEventIdentifier = PPoolSDK.Instance.AddAction(value =>
			{
				callback?.Invoke(CreateConnectionStatus(value));
			});
			ChatInterface.RegisterConnectionStatus(connectionEventIdentifier);
		}

		private static ConnectionStatus CreateConnectionStatus(Dictionary<string, object> value)
		{
			ConnectionStatus result = null;
			string type = value.GetValue("status", "");
			switch (type)
			{
				case "Connected": result = new Connected(); break;
				case "Disconnected": result = new Disconnected(value); break;
				case "Reconnecting": result = new Reconnecting(); break;
				case "ConnectionFailed": result = new ConnectionFailed(value); break;
				default: Debug.LogError($"ConnectionStatus error {type}"); break;
			}
			return result;
		}

		public static void UnregisterConnectionStatus()
		{
			ChatInterface.UnregisterConnectionStatus();

			PPoolSDK.Instance.RemoveAction(connectionEventIdentifier);
			connectionEventIdentifier = string.Empty;
		}
	}


	public static class DictionaryExtensions
	{
		public static T GetValue<T>(this Dictionary<string, object> dict, string key, T defaultValue = default)
		{
			if (dict.TryGetValue(key, out var value))
			{
				if (value is T tValue)
					return tValue;

				try
				{
					return (T)Convert.ChangeType(value, typeof(T));
				}
				catch
				{
					return defaultValue;
				}
			}
			return defaultValue;
		}
	}
}
