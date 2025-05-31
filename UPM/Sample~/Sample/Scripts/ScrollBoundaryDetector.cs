using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ScrollBoundaryDetector : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
	public ScrollRect scrollRect;
	public Action onOverScrollTop;
	public Action onOverScrollDown;

	private bool isDragging = false;
	float startPos = 0;

	void Start()
	{
		
	}

	public void Init()
	{
		startPos = 0;
		StartCoroutine(CheckStartPosition());
	}

	IEnumerator CheckStartPosition()
	{
		bool check = true;
		while(check)
		{
			yield return null;
			
			if (startPos != scrollRect.content.anchoredPosition.y)
			{
				startPos = scrollRect.content.anchoredPosition.y;
			}
			else
			{
				check = false;
			}
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		isDragging = true;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!isDragging) return;

		float deltaY = eventData.delta.y;

		if (IsAtBottom() && deltaY > 0)
		{
			onOverScrollDown?.Invoke();
		}

		if (IsAtTop() && deltaY < 0)
		{
			onOverScrollTop?.Invoke();
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		isDragging = false;
	}

	private bool IsAtTop()
	{
		float contentHeight = scrollRect.content.rect.height;
		float scrollY = scrollRect.content.anchoredPosition.y;
		float checkValue = MathF.Abs(contentHeight) - MathF.Abs(scrollY);

		return checkValue < -10.0f;
	}

	private bool IsAtBottom()
	{
		float scrollY = scrollRect.content.anchoredPosition.y;
		float checkValue = MathF.Abs(startPos) - MathF.Abs(scrollY);

		return checkValue > 10.0f;
	}
}
