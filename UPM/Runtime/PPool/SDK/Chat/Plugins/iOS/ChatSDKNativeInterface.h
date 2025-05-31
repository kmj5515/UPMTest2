#import <Foundation/Foundation.h>

#ifndef ChatSDKNativeInterface_h
#define ChatSDKNativeInterface_h

#ifdef __cplusplus
extern "C" {
#endif

    // Core functions
    void ChatSDKInitialize(int environment, const char* target);
    void ChatSDKConnect(const char* userId, const char* token, const char* identifier);
    void ChatSDKDisconnect();
    void ChatSDKLogout();
    void ChatSDKFetchRooms(const char* identifier);
    void ChatSDKFetchRoom(const char* roomId, const char* identifier);
    void ChatSDKCreateRoom(const char* chatRoomOptionJson, const char* identifier);
    void ChatSDKEnter(const char* roomId, const char* identifier);
    void ChatSDKLeave(const char* roomId, const char* identifier);
    void ChatSDKExit(const char* roomId, const char* identifier);
    void ChatSDKInvite(const char* roomId, const char* userIdsJson, const char* identifier);
    void ChatSDKSetAlarm(const char* roomId, bool alarm, const char* identifier);
    void ChatSDKUpdateCredential(const char* userId, const char* token);
    
    void ChatSDKSendMessage(const char* roomId, const char* message, const char* identifier);
    void ChatSDKResendMessage(const char* roomId, const char* message, const char* identifier);
    void ChatSDKFetchLatestMessages(const char* roomId, int count, const char* identifier);
    void ChatSDKFetchPreviousMessages(const char* roomId, int count, int before, const char* identifier);
    void ChatSDKFetchNextMessages(const char* roomId, int count, int since, const char* identifier);
    
    void ChatSDKRegisterChatRoomEvent(const char* roomId, const char* identifier);
    void ChatSDKUnregisterChatRoomEvent(const char* roomId);

    const char* ChatSDKGetConnectionStatus();
    void ChatSDKRegisterConnectionStatus(const char* identifier);
    void ChatSDKUnregisterConnectionStatus();

    void ChatSDKRegisterChatEvent(const char* identifier);
    void ChatSDKUnregisterChatEvent();
    
    void ChatSDKFetchUnsentMessages(const char* roomId, const char* identifier);
    void ChatSDKDeleteUnsentMessage(const char* roomId, const char* messageId, const char* identifier);
    
#ifdef __cplusplus
}
#endif

#endif /* ChatSDKNativeInterface_h */
