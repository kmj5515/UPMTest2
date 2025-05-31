using PPool.ChatSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatData : MonoBehaviour
{
	public string UserId { get; set; }
	public string Token { get; set; }
	public string RoomId { get; set; }
	public string Nickname { get; set; }
	public string ProfileUrl { get; set; }

	List<ChatRoom> rooms = new List<ChatRoom>();
	Dictionary<string, ChatRoomSession> roomSessionDic = new Dictionary<string, ChatRoomSession>();

	public void Init()
	{

	}

	public void SetRooms(List<ChatRoom> rooms)
	{
		this.rooms = rooms;
	}

	public void AddRoom(ChatRoom room)
	{
		rooms.Add(room);
	}

	public void RemoveRoom(ChatRoom room)
	{
		rooms.Remove(room);
	}

	public void RemoveRoom(string id)
	{
		rooms.RemoveAll((room) => id == room.Id);
	}

	public List<ChatRoom> GetRooms()
	{
		return rooms;
	}

	public ChatRoom GetRoom(string id)
	{
		return rooms.Find((room) => id == room.Id);
	}

	public void UpdateRoom(ChatRoom inRoom)
	{
		ChatRoom room = GetRoom(inRoom.Id);
		if(room != null)
		{
			room = inRoom;
		}
	}

	public void ClearRooms()
	{
		rooms.Clear();
	}

	static ChatData instance;
	public static ChatData Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject gameObject = new GameObject("ChatData");
				instance = gameObject.AddComponent<ChatData>();
			}
			return instance;
		}
	}

	public void AddOrUpdateSession(string roomId, ChatRoomSession session)
    {
        roomSessionDic[roomId] = session;
    }

    public bool RemoveSession(string roomId)
    {
        if (roomSessionDic.Remove(roomId)) 
			return true;
        else 
			return false;
    }

    public ChatRoomSession GetSession(string roomId)
    {
        roomSessionDic.TryGetValue(roomId, out var session);
        return session;
    }
}
