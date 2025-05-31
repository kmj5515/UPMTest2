using PPool.ChatSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ChattingRoom : BaseUI
{
	// Info
	[SerializeField] TMP_Text roomTitle;

    // Chat Top
	[SerializeField] Button backBt;
	[SerializeField] Button alarmBt;
	[SerializeField] Button inviteBt;
	[SerializeField] Button exitBt;

    // Chat List
    [SerializeField] GameObject myChatPrefab;
    [SerializeField] GameObject otherChatPrefab;
    [SerializeField] RectTransform content;

    // Chat Bottom
	[SerializeField] TMP_InputField messageInput;
	[SerializeField] Button sendBt;

	[SerializeField] ScrollBoundaryDetector scrollDetector;

	// Test Sample
	[SerializeField] TMP_Text sampleText;

    Dictionary<string, ChattingInfo> chattingInfos = new Dictionary<string, ChattingInfo>();
    [SerializeField] ChattingList chattingList;
    [SerializeField] InvitePopup invitePopup;
    [SerializeField] NoticePopup noticePopup;

	private ChatRoomSession roomSession = null;
	private bool isAlarm = false;
    public string RoomId { get; set; }

    private int startSeq = int.MaxValue;
	private int endSeq = 0;
	private bool isCoolingDown = false;

	public void OnEnable()
	{
        scrollDetector.Init();

        StartCoroutine(FetchMessages());
	}

	public override void Init()
    {
        base.Init();

		startSeq = int.MaxValue;
	    endSeq = 0;
		isCoolingDown = false;

		backBt.onClick.AddListener(OnBackClicked);
        alarmBt.onClick.AddListener(OnAlarmClicked);
        inviteBt.onClick.AddListener(OnInviteClicked);
        exitBt.onClick.AddListener(OnExitClicked);
        sendBt.onClick.AddListener(OnSendClicked);

        messageInput.onEndEdit.AddListener(OnMessageInputChanged);

		if (roomSession != null)
        {
            roomSession.RegisterChatRoomEvent((obj) =>
            {
                ChatMessage message = obj as ChatMessage;
                if (message != null) {
                    if (chattingInfos.TryGetValue(message.ClientId, out ChattingInfo chatInfo))
                    {
                        // 이미 있는 메시지 → 상태 업데이트
                        chatInfo.UpdateMessage(message);
                    }
                    else
                    {
                        bool isMyChat = message.Sender.UserId.Equals(ChatData.Instance.UserId);
                        CreateChat(message, false, isMyChat);
                    }
                }

                ChatRoom room = obj as ChatRoom;
                if (room != null)
                {
                    roomTitle.text = room.Title;
                    Debug.Log($"@@@ RegisterChatRoomEvent RoomUpdated : {room.Title}");
				}
			});

            scrollDetector.onOverScrollTop = () =>
			{
				if (startSeq <= 1)
                    return;

				if (!isCoolingDown)
				{
					roomSession.FetchPreviousMessages(30, startSeq-1, (result) =>
                    {
                        if(result.IsSuccess)
                        {
							UpdateFetchMessage(result.Value, false, true);
						}
					});

					StartCoroutine(CooldownRoutine());
				}
			};

            scrollDetector.onOverScrollDown = () =>
			{
				if (!isCoolingDown)
				{
					roomSession.FetchNextMessages(30, endSeq + 1, (result) =>
					{
                        if (result.IsSuccess)
                        {
                            UpdateFetchMessage(result.Value, true, false);
                        }
					});

					StartCoroutine(CooldownRoutine());
				}
			};
		}
	}

	IEnumerator FetchMessages()
    {
        yield return FetchLatestMessages();

        yield return FetchUnsentMessages();
	}

	IEnumerator FetchLatestMessages()
    {
		bool isDone = false;

        roomSession.FetchLatestMessages(30, (result) =>
        {
            if (result.IsSuccess)
            {
                UpdateFetchMessage(result.Value, true, false);
            }
            isDone = true;
		});

		yield return new WaitUntil(() => isDone);
	}

	IEnumerator FetchUnsentMessages()
    {
		bool isDone = false;

		roomSession.FetchUnsentMessages((result) =>
		{
			if (result.IsSuccess)
			{
                List<ChatMessage> chatMessages = result.Value;
				chatMessages.Reverse();

				foreach (var message in chatMessages)
				{
					if (chattingInfos.ContainsKey(message.ClientId) == false)
					{
						bool isMyChat = message.Sender.UserId.Equals(ChatData.Instance.UserId);
						CreateChat(message, false, isMyChat);
					}
				}
				isDone = true;
			}
		});

		yield return new WaitUntil(() => isDone);
	}

	void OnApplicationPause(bool pause)
    {
        isCoolingDown = false;
	}

	void UpdateFetchMessage(List<ChatMessage> chatMessages, bool isReverse, bool addToTop)
    {
        if(isReverse)
		    chatMessages.Reverse();

		foreach (var msg in chatMessages)
		{
			bool isMyChat = msg.Sender.UserId.Equals(ChatData.Instance.UserId);
			string message = msg.Content.ToHtml();
			string clientId = msg.ClientId;

            startSeq = startSeq > msg.Seq ? msg.Seq : startSeq;
            endSeq = endSeq < msg.Seq ? msg.Seq : endSeq;

            Debug.Log($"@@@ startSeq: {startSeq} endSeq: {endSeq}");

			if (chattingInfos.ContainsKey(clientId) == false)
				CreateChat(msg, addToTop, isMyChat);
		}
	}

	IEnumerator CooldownRoutine()
	{
		isCoolingDown = true;
		yield return new WaitForSeconds(2f);
		isCoolingDown = false;
	}

	public override void Deinit()
    {
        base.Deinit();

        backBt.onClick.RemoveListener(OnBackClicked);
        alarmBt.onClick.RemoveListener(OnAlarmClicked);
        inviteBt.onClick.RemoveListener(OnInviteClicked);
        exitBt.onClick.RemoveListener(OnExitClicked);
        sendBt.onClick.RemoveListener(OnSendClicked);

        messageInput.onEndEdit.RemoveListener(OnMessageInputChanged);

        if (roomSession != null)
        {
            roomSession.UnregisterChatRoomEvent();
		}
        roomSession = null;

        scrollDetector.onOverScrollTop = null;
        scrollDetector.onOverScrollDown = null;

		foreach (var obj in chattingInfos)
		{
			Destroy(obj.Value.gameObject);
		}
		chattingInfos.Clear();

        this.Hide();
	}

    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
	}

    public void SetInfo(string roomId, string title, ChatRoomSession roomSession)
    {
        this.RoomId = roomId;
        roomTitle.text = title;
        this.roomSession = roomSession;

        ChatData.Instance.AddOrUpdateSession(roomId, roomSession);
		ChatData.Instance.RoomId = roomId;

        ChatRoom room = ChatData.Instance.GetRoom(roomId);
        if(room != null)
        {
            isAlarm = room.IsAlarmEnabled;
			UpdateAlarmBt();
		}
	}

    private void OnBackClicked()
    {
		ChatSDK.Leave(RoomId, (result) =>
        {
            if (result.IsSuccess)
            {
                Debug.Log($"@@@ [Unity-Sample] Leave room result: {result}");
                this.Deinit();

                ChatData.Instance.RoomId = string.Empty;
                chattingList.FetchRoom(RoomId);
            }
		});
	}

	private void OnAlarmClicked()
    {
        bool changeAlarm = !isAlarm;
        ChatSDK.SetAlarm(RoomId, changeAlarm, (result) =>
        {
            if (result.IsSuccess)
            {
                isAlarm = changeAlarm;
                UpdateAlarmBt();
			}
            Debug.Log($"@@@ [Unity-Sample] SetAlarm result: {result}");
        });
    }

    private void UpdateAlarmBt()
    {
		alarmBt.GetComponent<Image>().color = isAlarm ? Color.yellow : Color.white;
	}

	private void OnInviteClicked()
    {
        invitePopup.Init();
        invitePopup.Show();
    }

    private void OnExitClicked()
    {
        ChatSDK.Exit(RoomId, (result) =>
        {
            Debug.Log($"@@@ [Unity-Sample] Exit result: {result}");

            if (result.IsSuccess)
            {
                ChatData.Instance.RemoveSession(RoomId);
                ChatData.Instance.RoomId = string.Empty;

				this.Hide();
                this.Deinit();
            }
        });
    }

    private void OnSendClicked()
    {
        string message = messageInput.text;
        if (string.IsNullOrEmpty(message))
        {
			Debug.Log($"@@@ [Unity-Sample] message IsNullOrEmpty");
			return;
        }

		if (roomSession == null)
        {
			Debug.Log($"@@@ [Unity-Sample] roomSession null");
			return;
        }

        messageInput.text = string.Empty;

		roomSession?.SendMessage(message, (result) =>
        {
            if (result.IsSuccess)
            {
                Debug.Log($"@@@ [Unity-Sample] Message sent successfully");
            }
            else
            {
                Debug.LogError($"@@@ [Unity-Sample] Failed to send message");
                noticePopup.SetInfo("Error", $"{result.Error?.Message}");
                noticePopup.Show();
            }
        });
    }

    public void TestSendMessage()
    {
		string message = sampleText.text;
		Debug.Log($"@@@ [Unity-Sample] TestSendMessage {message}");

		if (string.IsNullOrEmpty(message))
		{
			Debug.Log($"@@@ [Unity-Sample] message IsNullOrEmpty");
			return;
		}

		if (roomSession == null)
		{
			Debug.Log($"@@@ [Unity-Sample] roomSession null");
			return;
		}

		messageInput.text = string.Empty;

		roomSession?.SendMessage(message, (result) =>
		{
			if (result.IsSuccess)
			{
				Debug.Log($"@@@ [Unity-Sample] Message sent successfully");
			}
			else
			{
				Debug.LogError($"@@@ [Unity-Sample] Failed to send message");
			}
		});
	}

	private void CreateChat(ChatMessage message, bool addToTop, bool isMyChat = false)
    {
        GameObject chatObject = Instantiate(isMyChat ? myChatPrefab : otherChatPrefab);
        ChattingInfo chattingInfo = chatObject.GetComponent<ChattingInfo>();
        
        chattingInfo.SetMessage(message, roomSession);
		chatObject.transform.SetParent(content, false);

        chattingInfo.chatObjDestroy += DeleteChatDestroyed;

        if (addToTop)
            chatObject.transform.SetAsFirstSibling();
        else
            chatObject.transform.SetAsLastSibling();

		chatObject.SetActive(true);

		chattingInfos.Add(message.ClientId, chattingInfo);
	}

    private void DeleteChatDestroyed(ChattingInfo chattingInfo)
    {
        if (chattingInfos.ContainsKey(chattingInfo.clientId))
        {
            chattingInfos.Remove(chattingInfo.clientId);
        }
    }

    private void OnMessageInputChanged(string message)
    {
        // if (string.IsNullOrEmpty(message))
        // {
        //     return;
        // }

        // OnSendClicked();
    }

    // private void OnMessageReceived(Message message)
    // {
    //     Debug.Log($"@@@ [Unity-Sample] Message received: {message.content}");
    //     CreateChat(message.content, message.senderId == "Me");
    // }

    // private void OnChatEventReceived(ChatEvent chatEvent)
    // {
    //     Debug.Log($"@@@ [Unity-Sample] Chat event received: {chatEvent.type}");
    //     switch (chatEvent.type)
    //     {
    //         case ChatEventType.RoomInvited:
    //             Debug.Log($"@@@ [Unity-Sample] Room invited: {chatEvent.room.id}");
    //             break;
    //         case ChatEventType.RoomUpdated:
    //             Debug.Log($"@@@ [Unity-Sample] Room updated: {chatEvent.room.id}");
    //             break;
    //     }
    // }
}
