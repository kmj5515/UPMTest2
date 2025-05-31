#if !UNITY_IOS && !UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPool.ChatSDK
{
	internal class ChatInterface
    {
		public static void Initialize(int environment, string target) {}
		public static void Connect(ChatCredential chatCredential, string identifier) {}
		public static void Disconnect() {}
		public static void Logout() {}
		public static void FetchRooms(string identifier) {}
		public static void FetchRoom(string id, string identifier) {}
		public static void Enter(string roomId, string identifier) {}
		public static void CreateRoom(string chatRoomOptionJson, string identifier) {}		
		public static void Invite(string roomId, string userIdsJson, string identifier) {}
		public static void SetAlarm(string roomId, bool alarm, string identifier) {}
		public static void Leave(string roomId, string identifier) {}
		public static void Exit(string roomId, string identifier) {}
		public static void UpdateCredential(ChatCredential chatCredential) {}
		public static string GetConnectionStatus() { return ""; }
		public static void RegisterConnectionStatus(string identifier) {}
		public static void UnregisterConnectionStatus() {}
		public static void RegisterChatEvent(string identifier) {}
		public static void UnregisterChatEvent() {}

		// ChatRoomSession
		public static void SendMessage(string roomId, string formattedMessageJson, string identifier) {}
		public static void ResendMessage(string roomId, string chatMessageJson, string identifier) {}
		public static void FetchLatestMessages(string roomId, int count, string identifier) {}
		public static void FetchPreviousMessages(string roomId, int count, int before, string identifier) {}
		public static void FetchNextMessages(string roomId, int count, int since, string identifier) {}
		public static void FetchUnsentMessages(string roomId, string identifier) {}
		public static void DeleteUnsentMessage(string roomId, string messageId, string identifier) {}
		public static void RegisterChatRoomEvent(string roomId, string identifier) {}
		public static void UnregisterChatRoomEvent(string roomId) {}
	}
}
#endif
