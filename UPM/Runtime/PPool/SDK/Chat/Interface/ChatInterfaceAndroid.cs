#if UNITY_ANDROID
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

namespace PPool.ChatSDK
{
	internal class ChatInterface
	{
		static AndroidJavaObject sdkWrapper = new AndroidJavaObject("com.ppool.chat.ChatSDKWrapper");

		public static void Initialize(int environment, string target)
		{
			sdkWrapper.CallStatic("Initialize", environment, target);
		}

		public static void Connect(ChatCredential chatCredential, string identifier)
		{
			sdkWrapper.CallStatic("Connect", chatCredential.userId, chatCredential.token, identifier);
		}

		public static void Disconnect()
		{
			sdkWrapper.CallStatic("Disconnect");
		}

		public static void Logout()
		{
			sdkWrapper.CallStatic("Logout");
		}

		public static void FetchRooms(string identifier)
		{
			sdkWrapper.CallStatic("FetchRooms", identifier);
		}

		public static void FetchRoom(string id, string identifier)
		{
			sdkWrapper.CallStatic("FetchRoom", id, identifier);
		}

		public static void Enter(string roomId, string identifier)
		{
			sdkWrapper.CallStatic("Enter", roomId, identifier);
		}

		public static void CreateRoom(string chatRoomOptionJson, string identifier)
		{
			sdkWrapper.CallStatic("CreateRoom", chatRoomOptionJson, identifier);
		}

		public static void Invite(string roomId, string userIdsJson, string identifier)
		{
			sdkWrapper.CallStatic("Invite", roomId, userIdsJson, identifier);
		}

		public static void SetAlarm(string roomId, bool alarm, string identifier)
		{
			sdkWrapper.CallStatic("SetAlarm", roomId, alarm, identifier);
		}

		public static void Leave(string roomId, string identifier)
		{
			sdkWrapper.CallStatic("Leave", roomId, identifier);
		}

		public static void Exit(string roomId, string identifier)
		{
			sdkWrapper.CallStatic("Exit", roomId, identifier);
		}

		public static void UpdateCredential(ChatCredential chatCredential)
		{
			sdkWrapper.CallStatic("UpdateCredential", chatCredential.userId, chatCredential.token);
		}

		public static string GetConnectionStatus()
		{
			return sdkWrapper.CallStatic<string>("GetConnectionStatus");
		}

		public static void RegisterConnectionStatus(string identifier)
		{
			sdkWrapper.CallStatic("RegisterConnectionStatus", identifier);
		}

		public static void UnregisterConnectionStatus()
		{
			sdkWrapper.CallStatic("UnregisterConnectionStatus");
		}

		public static void RegisterChatEvent(string identifier)
		{
			sdkWrapper.CallStatic("RegisterChatEvent", identifier);
		}

		public static void UnregisterChatEvent()
		{
			sdkWrapper.CallStatic("UnregisterChatEvent");
		}

		// ChatRoomSession
		public static void SendMessage(string roomId, string formattedMessageJson, string identifier)
		{
			sdkWrapper.CallStatic("SendMessage", roomId, formattedMessageJson, identifier);
		}

		public static void ResendMessage(string roomId, string chatMessageJson, string identifier)
		{
			sdkWrapper.CallStatic("ResendMessage", roomId, chatMessageJson, identifier);
		}

		public static void FetchLatestMessages(string roomId, int count, string identifier)
		{
			sdkWrapper.CallStatic("FetchLatestMessages", roomId, count, identifier);
		}

		public static void FetchPreviousMessages(string roomId, int count, int before, string identifier)
		{
			sdkWrapper.CallStatic("FetchPreviousMessages", roomId, count, before, identifier);
		}

		public static void FetchNextMessages(string roomId, int count, int since, string identifier)
		{			
			sdkWrapper.CallStatic("FetchNextMessages", roomId, count, since, identifier);
		}

		public static void FetchUnsentMessages(string roomId, string identifier)
		{
			sdkWrapper.CallStatic("FetchUnsentMessages", roomId, identifier);
		}

		public static void DeleteUnsentMessage(string roomId, string messageId, string identifier)
		{
			sdkWrapper.CallStatic("DeleteUnsentMessage", roomId, messageId, identifier);
		}

		public static void RegisterChatRoomEvent(string roomId, string identifier)
		{
			sdkWrapper.CallStatic("RegisterChatRoomEvent", roomId, identifier);
		}

		public static void UnregisterChatRoomEvent(string roomId)
		{
			sdkWrapper.CallStatic("UnregisterChatRoomEvent", roomId);
		}
	}
}
#endif