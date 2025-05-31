using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using PPool.ChatSDK;
using MiniJSON;
using System.Data;

public class ChattingList : BaseUI
{
	// User
	[SerializeField] Image userImage;
	[SerializeField] TMP_Text userID;
	[SerializeField] TMP_Text userNickName;


    // Chat Top
	[SerializeField] Button logoutBt;
	[SerializeField] Button createRoomBt;

    // Chat List
    [SerializeField] GameObject chattingPrefab;
    [SerializeField] RectTransform content;

	[SerializeField] ChatSDKSample chatSDKSample;
	[SerializeField] ChattingRoom chattingRoom;
	[SerializeField] CreateRoomPopup createRoomPopup;

	[SerializeField] Image connectState;

	List<ChattingRoomInfo> chattingRooms = new List<ChattingRoomInfo>();

	public override void Init()
    {
        base.Init();

        SettingUserInfo(ChatData.Instance.UserId, ChatData.Instance.Nickname);

        logoutBt.onClick.AddListener(OnLogoutClicked);
        createRoomBt.onClick.AddListener(OnCreateRoomClicked);

		// 채팅방 목록 생성
		FetchRooms();

        ChatSDK.RegisterChatEvent((chatEvent) =>
        {
			switch(chatEvent.Type)
            {
				case ChatEventType.RoomCreated:
					{
						Debug.Log("@@@ [Unity-Sample] ChatEventType RoomCreated");
						var roomObj = chattingRooms.Find((room) => chatEvent.RoomId == room.roomId);
						if (roomObj == null)
						{
							ChatRoom room = chatEvent.Room;
							ChatData.Instance.AddRoom(room);
							CreateChattingRoom(room.Id, room.Title, room.Preview, room.Users);
						}
					}
					break;
				case ChatEventType.RoomInvited:
                    {
                        Debug.Log("@@@ [Unity-Sample] ChatEventType RoomInvited");
						var roomObj = chattingRooms.Find((room) => chatEvent.RoomId == room.roomId);
						if (roomObj == null)
						{
                            ChatRoom room = chatEvent.Room;
							ChatData.Instance.AddRoom(room);
							CreateChattingRoom(room.Id, room.Title, room.Preview, room.Users);
						}
					}
                    break;
				case ChatEventType.RoomUpdated:
					{
						Debug.Log("@@@ [Unity-Sample] ChatEventType RoomUpdated");
						var roomObj = chattingRooms.Find((room) => chatEvent.RoomId == room.roomId);
						if (roomObj != null)
						{
							ChatRoom room = chatEvent.Room;
							ChatData.Instance.UpdateRoom(room);
                            roomObj.UpdateRooom(room);
						}
					}
					break;
				case ChatEventType.RoomDeleted:
					{
						Debug.Log("@@@ [Unity-Sample] ChatEventType RoomDeleted");
						ChatData.Instance.RemoveRoom(chatEvent.RoomId);
						RemoveChattingRoom(chatEvent.RoomId);
					}
					break;
			}
		});

		connectState.gameObject.SetActive(true);

		ChatSDK.RegisterConnectionStatus((connectionStatus) =>
        {
		    switch (connectionStatus)
            {
				case Connected:
					{
						Debug.Log($"@@@ Connected");
						connectState.color = new Color(0, 1, 0, 1);

						if (chattingRoom.gameObject.activeSelf && !string.IsNullOrEmpty(chattingRoom.RoomId))
						{
							ChatSDK.Enter(chattingRoom.RoomId, (result) =>
							{
								if (result.IsSuccess)
								{
									Debug.Log("@@@ Enter");
								}
							});
						}
					}
					break;
				case Disconnected:
                    {
						Disconnected status = connectionStatus as Disconnected;
						if(status != null)
						{
							Debug.Log($"@@@ Disconnected {status.Reason}");
						}

						connectState.color = new Color(1, 0, 0, 1);
					}
                    break;
				case Reconnecting:
					{
						Debug.Log($"@@@ Reconnecting");
						connectState.color = new Color(1, 0, 0, 1);
					}
					break;
				case ConnectionFailed:
                    {
						ConnectionFailed status = connectionStatus as ConnectionFailed;
						if (status != null)
						{
							Debug.Log($"@@@ ConnectionFailed {status.Reason} {status.Code} {status.Message}");
						}

						connectState.color = new Color(1, 0, 0, 1);
					}
                    break;
			}
		});
	}

    public override void Deinit()
    {
        base.Deinit();

		connectState.gameObject.SetActive(false);

		logoutBt.onClick.RemoveListener(OnLogoutClicked);
        createRoomBt.onClick.RemoveListener(OnCreateRoomClicked);
    
        ClearChattingRoom();

		ChatSDK.UnregisterChatEvent();
        ChatSDK.UnregisterConnectionStatus();
	}

    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }

	void OnApplicationPause(bool pause)
    {
		if (pause == false)
		{
			string userId = ChatData.Instance.UserId;
			string token = ChatData.Instance.Token;

			if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(token))
			{
				ChatCredential credential = new ChatCredential(userId, token);
				ChatSDK.Connect(credential, (result) => {
		            if (result.IsSuccess)
		            {
		                FetchRooms();

                        if(chattingRoom.gameObject.activeSelf && !string.IsNullOrEmpty(chattingRoom.RoomId))
                        {
							ChatSDK.Enter(chattingRoom.RoomId, (result) =>
							{
                                if (result.IsSuccess)
                                {
                                    Debug.Log("@@@ Enter");
                                }
							});
						}
		            }
		        });
		    }
		}
		else
		{
			ChatSDK.Disconnect();
		}
	}

    private void SettingUserInfo(string userId, string userNickname)
    {
        // 여기서 유저 정보 설정 처리
        // userImage.sprite = Chat.data.profileUrl;
        userID.text = userId;
        userNickName.text = userNickname;
    }

    private void OnLogoutClicked()
    {
        // 여기서 로그아웃 처리
        Debug.Log("@@@ [Unity-Sample] ChattingListView OnLogoutClicked");

        chatSDKSample.Logout();

        this.Deinit();
        this.Hide();
    }

    private void OnCreateRoomClicked()
    {
        // 여기서 채팅방 생성 처리
        Debug.Log("@@@ [Unity-Sample] ChattingListView OnCreateRoomClicked");
        createRoomPopup.Init();
        createRoomPopup.Show();
    }

    public void FetchRooms()
    {
		ClearChattingRoom();
        ChatData.Instance.ClearRooms();

		ChatSDK.FetchRooms((result) =>
        {
			if(result.IsSuccess)
            {
				ChatData.Instance.SetRooms(result.Value);

				foreach (var room in result.Value)
				{
					CreateChattingRoom(room.Id, room.Title, room.Preview, room.Users);
				}
			}
		});
    }

	public void FetchRoom(string roomId)
    {
		ChatSDK.FetchRoom(roomId, (result) =>
        {
            if(result.IsSuccess)
            {
				ChatData.Instance.UpdateRoom(result.Value);

				var room = chattingRooms.Find((room) => roomId == room.roomId);
				if (room != null)
				{
					room.UpdateRooom(result.Value);
				}
			}
		});
	}

	private void CreateChattingRoom(string roomId, string roomName, string roomLastMessage, List<ChatUser> roomUsers)
    {
        GameObject chatObect = Instantiate(chattingPrefab);

        ChattingRoomInfo chattingRoomInfo = chatObect.GetComponent<ChattingRoomInfo>();
        chattingRoomInfo.Init(roomId, roomName, roomLastMessage, roomUsers);
        chattingRooms.Add(chattingRoomInfo);

		chatObect.transform.SetParent(content, false);
        chatObect.SetActive(true);

        chatObect.GetComponent<Button>().onClick.AddListener(() => OnChattingRoomClicked(roomId, roomName));
    }

    public void RemoveChattingRoom(string roomId)
    {
        var toRemove = chattingRooms.FindAll(room => room.roomId == roomId);
        foreach (var chattingRoom in toRemove)
        {
            var btn = chattingRoom.gameObject.GetComponent<Button>();
            if (btn != null)
                btn.onClick.RemoveAllListeners();
            Destroy(chattingRoom.gameObject);
        }
        chattingRooms.RemoveAll(room => room.roomId == roomId);
    }

    public void ClearChattingRoom()
    {
        foreach (var chattingRoom in chattingRooms)
        {
            var btn = chattingRoom.gameObject.GetComponent<Button>();
            if (btn != null)
                btn.onClick.RemoveAllListeners();
            Destroy(chattingRoom.gameObject);
        }
        chattingRooms.Clear();
    }

    private void OnChattingRoomClicked(string roomId, string roomName)
    {
        EnterRoom(roomId, roomName);
	}

    public void EnterRoom(string roomId, string roomName)
    {
        ChatSDK.Enter(roomId , (result) =>
        {
            if(result.IsSuccess)
            {
                chattingRoom.SetInfo(roomId, roomName, result.Value);
				chattingRoom.Init();
				chattingRoom.Show();
			}
			else
			{
				Debug.Log($"EnterRoom Error Message: {result.Error?.Message}");

				switch (result.Error)
				{
					case NotInitializedError:
						Debug.Log($"NotInitializedError");
						break;
					case ConnectionFailedError: break;
					case AuthenticationDeniedError: break;
					case PermissionDeniedError: break;
					case NotParticipatedError: break;
					case RoomNotFoundError: break;
					case BanWordsError: break;
					case ServerError: break;
					case NotConnected: break;
				}
			}
        });
	}
}
