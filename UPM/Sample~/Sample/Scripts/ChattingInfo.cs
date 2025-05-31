using PPool.ChatSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ChattingInfo : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public RectTransform layout;
	public TMP_Text userNickName;
    public TMP_Text chat;
    public TMP_Text time;

	Camera mainCamera;
	Canvas canvas;

	public Button resendBt;
    public Button deleteBt;
    public GameObject sendingImg;

	public System.Action<ChattingInfo> chatObjDestroy;

	public string clientId;
	ChatRoomSession roomSession;
	ChatMessage chatMessage;

	private IEnumerator Start()
	{
		yield return null;

		canvas = gameObject.GetComponentInParent<Canvas>();
		if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			mainCamera = null;
		else
			mainCamera = canvas.worldCamera;

		float nickY = this.userNickName.GetComponent<RectTransform>().sizeDelta.y;
		float chatY = this.chat.GetComponent<RectTransform>().sizeDelta.y;
		float timeY = this.time.GetComponent<RectTransform>().sizeDelta.y;

		layout.sizeDelta = new Vector2(layout.sizeDelta.x, nickY + chatY + timeY);

		RectTransform rectTransform = GetComponent<RectTransform>();
		rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, layout.sizeDelta.y);
	}

	public void SetMessage(ChatMessage chatMessage, ChatRoomSession roomSession)
	{
		ClearSendingState();

		this.chatMessage = chatMessage;
		bool isMyChat = chatMessage.Sender.UserId.Equals(ChatData.Instance.UserId);

		this.clientId = chatMessage.ClientId;
		this.userNickName.text = isMyChat ? "Me" : "Other";
		this.chat.text = chatMessage.Content.ToHtml();
		this.time.text = "";//todo : chatMessage.Date;
		this.roomSession = roomSession;

		UpdateSendingState(chatMessage);

		if (resendBt != null)resendBt.onClick.AddListener(ResendButtonClicked);
        if (deleteBt != null)deleteBt.onClick.AddListener(DeleteButtonClicked);
	}

	public void UpdateMessage(ChatMessage chatMessage)
	{
		this.chatMessage = chatMessage;
		UpdateSendingState(chatMessage);
	}

	void UpdateSendingState(ChatMessage chatMessage)
	{
		ClearSendingState();

		switch (chatMessage.Status)
		{
			case MessageStatus.SENDING:
				if (sendingImg != null) sendingImg.SetActive(true);
				break;

			case MessageStatus.SENT:
				// 아무것도 켜지 않음
				break;

			case MessageStatus.UNSENT:
				if (resendBt != null)
				{
					resendBt.gameObject.SetActive(true);
					resendBt.interactable = true;
				}
				if (deleteBt != null)
				{
					deleteBt.gameObject.SetActive(true);
					deleteBt.interactable = true;
				}
				break;

			default:
				Debug.LogWarning($"Unhandled MessageStatus: {chatMessage.Status}");
				break;
		}
	}


	private void ClearSendingState()
	{
		if (resendBt != null) resendBt.gameObject.SetActive(false);
		if (deleteBt != null) deleteBt.gameObject.SetActive(false);
		if (sendingImg != null) sendingImg.SetActive(false);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		int linkIndex = TMP_TextUtilities.FindIntersectingLink(this.chat, Input.mousePosition, mainCamera);

		if (linkIndex != -1)
		{
			TMP_LinkInfo linkInfo = this.chat.textInfo.linkInfo[linkIndex];
			Application.OpenURL(linkInfo.GetLinkID());
		}
	}

	private void ResendButtonClicked()
	{
		if (roomSession == null) return;

		if (chatMessage == null) return;

		roomSession.ResendMessage(chatMessage, result =>
        {
            if (result.IsSuccess)
            {
                Debug.Log("Resend successful!");
            }
            else
            {
                Debug.LogWarning("Resend failed!");
            }
        });

		resendBt.interactable = false;
		deleteBt.interactable = false;
	}

	private void DeleteButtonClicked()
	{
		roomSession.DeleteUnsentMessage(chatMessage.ClientId, result =>
        {
            if (result.IsSuccess)
            {
				chatObjDestroy?.Invoke(this);
				Destroy(this.gameObject);
            }
        });
	}

	void OnDestroy()
	{
		if (resendBt != null) resendBt.onClick.RemoveListener(ResendButtonClicked);
		if (deleteBt != null) deleteBt.onClick.RemoveListener(DeleteButtonClicked);
	}
}


