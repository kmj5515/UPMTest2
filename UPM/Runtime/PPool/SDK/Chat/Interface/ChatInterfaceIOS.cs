#if UNITY_IOS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace PPool.ChatSDK
{
	internal class ChatInterface
	{
		[DllImport("__Internal")]
        private static extern void ChatSDKInitialize(int environment, string target);
        internal static void Initialize(int environment, string target)
        {
            ChatSDKInitialize(environment, target);
        }

		[DllImport("__Internal")]
        private static extern void ChatSDKConnect(string userId, string token, string identifier);
        internal static void Connect(ChatCredential chatCredential, string identifier)
        {
            ChatSDKConnect(chatCredential.userId, chatCredential.token, identifier);
        }

		[DllImport("__Internal")]
        private static extern void ChatSDKDisconnect();
        internal static void Disconnect()
        {
            ChatSDKDisconnect();
        }
		
		[DllImport("__Internal")]
        private static extern void ChatSDKLogout();
        internal static void Logout()
        {
            ChatSDKLogout();
        }
		
		[DllImport("__Internal")]
        private static extern void ChatSDKFetchRooms(string identifier);
        internal static void FetchRooms(string identifier)
        {
            ChatSDKFetchRooms(identifier);
        }

		[DllImport("__Internal")]
        private static extern void ChatSDKFetchRoom(string id);
        internal static void FetchRoom(string id, string identifier)
        {
            ChatSDKFetchRoom(id);
        }

		[DllImport("__Internal")]
        private static extern void ChatSDKEnter(string roomId, string identifier);
        internal static void Enter(string roomId, string identifier)
        {
            ChatSDKEnter(roomId, identifier);
        }

		[DllImport("__Internal")]
        private static extern void ChatSDKCreateRoom(string chatRoomOptionJson, string identifier);
        internal static void CreateRoom(string chatRoomOptionJson, string identifier)
        {
            ChatSDKCreateRoom(chatRoomOptionJson, identifier);
        }

		[DllImport("__Internal")]	
        private static extern void ChatSDKInvite(string roomId, string userIdsJson, string identifier);
        internal static void Invite(string roomId, string userIdsJson, string identifier)
        {
            ChatSDKInvite(roomId, userIdsJson, identifier);
        }

		[DllImport("__Internal")]
        private static extern void ChatSDKSetAlarm(string roomId, bool alarm, string identifier);
        internal static void SetAlarm(string roomId, bool alarm, string identifier)
        {
            ChatSDKSetAlarm(roomId, alarm, identifier);
        }

		[DllImport("__Internal")]
        private static extern void ChatSDKLeave(string roomId, string identifier);
        internal static void Leave(string roomId, string identifier)
        {
            ChatSDKLeave(roomId, identifier);
        }

		[DllImport("__Internal")]
        private static extern void ChatSDKExit(string roomId, string identifier);
        internal static void Exit(string roomId, string identifier)
        {
            ChatSDKExit(roomId, identifier);
        }
		
		[DllImport("__Internal")]
        private static extern void ChatSDKUpdateCredential(string userId, string token);
        internal static void UpdateCredential(ChatCredential chatCredential)
        {
            ChatSDKUpdateCredential(chatCredential.userId, chatCredential.token);
        }

        [DllImport("__Internal")]
        private static extern string ChatSDKGetConnectionStatus();
        internal static string GetConnectionStatus()
        {
            return ChatSDKGetConnectionStatus();
        }

		[DllImport("__Internal")]
        private static extern void ChatSDKRegisterConnectionStatus(string identifier);
        internal static void RegisterConnectionStatus(string identifier)
        {
            ChatSDKRegisterConnectionStatus(identifier);
        }
		
		[DllImport("__Internal")]
        private static extern void ChatSDKUnregisterConnectionStatus();
        internal static void UnregisterConnectionStatus()
        {
            ChatSDKUnregisterConnectionStatus();
        }

        // ChatRoomSession
		[DllImport("__Internal")]
        private static extern void ChatSDKSendMessage(string roomId, string formattedMessageJson, string identifier);
        internal static void SendMessage(string roomId, string formattedMessageJson, string identifier)
        {
            ChatSDKSendMessage(roomId, formattedMessageJson, identifier);
        }

        [DllImport("__Internal")]
        private static extern void ChatSDKResendMessage(string roomId, string chatMessageJson, string identifier);
        public static void ResendMessage(string roomId, string chatMessageJson, string identifier) {
            ChatSDKResendMessage(roomId, chatMessageJson, identifier);
        }

        [DllImport("__Internal")]
        private static extern void ChatSDKFetchLatestMessages(string roomId, int count, string identifier);
        public static void FetchLatestMessages(string roomId, int count, string identifier) {
            ChatSDKFetchLatestMessages(roomId, count, identifier);
        }
        
        [DllImport("__Internal")]
        private static extern void ChatSDKFetchPreviousMessages(string roomId, int count, int before, string identifier);
        public static void FetchPreviousMessages(string roomId, int count, int before, string identifier) {
            ChatSDKFetchPreviousMessages(roomId, count, before, identifier);
        }

        [DllImport("__Internal")]
        private static extern void ChatSDKFetchNextMessages(string roomId, int count, int since, string identifier);
        public static void FetchNextMessages(string roomId, int count, int since, string identifier) {
            ChatSDKFetchNextMessages(roomId, count, since, identifier);
        }
        
        [DllImport("__Internal")]
        private static extern void ChatSDKFetchUnsentMessages(string roomId, string identifier);
        public static void FetchUnsentMessages(string roomId, string identifier) {
            ChatSDKFetchUnsentMessages(roomId, identifier);
        }

        [DllImport("__Internal")]
        private static extern void ChatSDKDeleteUnsentMessage(string roomId, string messageId, string identifier);
        public static void DeleteUnsentMessage(string roomId, string messageId, string identifier) {
            ChatSDKDeleteUnsentMessage(roomId, messageId, identifier);
        }

        [DllImport("__Internal")]
        private static extern void ChatSDKRegisterChatEvent(string identifier);
        internal static void RegisterChatEvent(string identifier)
        {
            ChatSDKRegisterChatEvent(identifier);
        }

		[DllImport("__Internal")]
        private static extern void ChatSDKUnregisterChatEvent();
        internal static void UnregisterChatEvent()
        {
            ChatSDKUnregisterChatEvent();
        }

        [DllImport("__Internal")]
        private static extern void ChatSDKRegisterChatRoomEvent(string roomId, string identifier);
        internal static void RegisterChatRoomEvent(string roomId, string identifier) {
            ChatSDKRegisterChatRoomEvent(roomId, identifier);
        }
        
        [DllImport("__Internal")]
        private static extern void ChatSDKUnregisterChatRoomEvent(string roomId);
        internal static void UnregisterChatRoomEvent(string roomId) {
            ChatSDKUnregisterChatRoomEvent(roomId);
        }
	}
}
#endif