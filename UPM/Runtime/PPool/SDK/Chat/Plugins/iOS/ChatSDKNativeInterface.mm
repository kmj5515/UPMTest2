#import <Foundation/Foundation.h>
#import "ChatSDKNativeInterface.h"
#import <PPoolChat/PPoolChat-Swift.h>

#import "ChatSDKUtils.h"
#import "ChatSDKDelegate.h"
#import "ChatSDKRoomDelegate.h"

static dispatch_queue_t serialQueue = dispatch_queue_create("com.ppool.chatsdk", DISPATCH_QUEUE_SERIAL);

// 방별 세션 캐시 (roomId -> ChatRoomSession)
static NSMutableDictionary<NSString *, ChatRoomSession *> *roomSessionDic = [NSMutableDictionary dictionary];
static NSMutableDictionary<NSString *, ChatSDKRoomDelegate *> *roomDelegateDic = [NSMutableDictionary dictionary];

extern "C" {
    void ChatSDKInitialize(int environment, const char* target) {
        NSLog(@"@@@ [Native] ChatSDK.initialize %d", environment);
        
        NSString *nsTarget = makeNSString(target);
        ChatConfig *config;
        switch (environment) {
            case 0:
                config = [ChatConfig devWithTarget:nsTarget isLogEnabled:true];
                break;
            case 1:
                config = [ChatConfig qaWithTarget:nsTarget isLogEnabled:true];
                break;
            case 2:
                config = [ChatConfig qa2WithTarget:nsTarget isLogEnabled:true];
                break;
            case 3:
                config = [ChatConfig sandboxWithTarget:nsTarget isLogEnabled:true];
                break;
            case 4:
                config = [ChatConfig liveWithTarget:nsTarget isLogEnabled:false];
                break;
            default:
                config = [ChatConfig devWithTarget:nsTarget isLogEnabled:true];
                break;
        }
        
        [ChatSDK initialize:config];
        ChatSDK.delegate = [ChatSDKDelegate shared];
    }

    void ChatSDKConnect(const char* userId, const char* token, const char* identifier) {
        NSString *nsUserId = makeNSString(userId);
        NSString *nsToken = makeNSString(token);
        NSString *nsIdentifier = makeNSString(identifier);
        
        dispatch_async(serialQueue, ^{
            ChatCredential *credential = [[ChatCredential alloc] initWithUserId:nsUserId token:nsToken];
            [ChatSDK connectWithCredential:credential completionHandler:^(NSError * _Nullable error) {
                dispatch_async(serialQueue, ^{
                    NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
                    [jsonObject setObject:nsIdentifier forKey:@"identifier"];
                    
                    NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
                    if (error) {
                        [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];
                    }
                    
                    [valueObject setObject:@(error == nil) forKey:@"result"];
                    [jsonObject setObject:valueObject forKey:@"value"];
                    
                    NSLog(@"@@@ [Native] ChatSDK.connect result: %@", jsonObject);
                    [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                });
            }];
        });
    }

    void ChatSDKDisconnect() {
        [ChatSDK disconnect];
        NSLog(@"@@@ [Native] ChatSDK.disconnect");
    }

    void ChatSDKLogout() {
        [ChatSDK logout];
        NSLog(@"@@@ [Native] ChatSDK.logout");
    }

    void ChatSDKFetchRooms(const char* identifier) {
        NSString* nsIdentifier = makeNSString(identifier);
        dispatch_async(serialQueue, ^{
            [ChatSDK fetchRoomsWithCompletionHandler:^(NSError * _Nullable error, NSArray<ChatRoom* > * _Nullable rooms) {
                dispatch_async(serialQueue, ^{
                    NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
                    NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];

                    if (error) {
                        NSLog(@"@@@ [Native] ChatSDK.fetchRooms fail: %@", error);
                        
                        [valueObject setObject:@(NO) forKey:@"result"];
                        [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];

                        [valueObject setObject:[NSArray array] forKey:@"rooms"];
                    } else {
                        NSMutableArray *roomArray = [NSMutableArray array];
                        for (ChatRoom *room in rooms) {
                            [roomArray addObject:[room dictionary]];
                        }

                        [valueObject setObject:@(YES) forKey:@"result"];
                        [valueObject setObject:roomArray forKey:@"rooms"];
                    }

                    [jsonObject setObject:nsIdentifier forKey:@"identifier"];
                    [jsonObject setObject:valueObject forKey:@"value"];

                    NSLog(@"@@@ [Native] ChatSDK.fetchRooms result: %@", jsonObject);
                    [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                });
            }];
        });
    }


    void ChatSDKFetchRoom(const char* roomId, const char* identifier) {
        NSString *nsRoomId = makeNSString(roomId);
        NSString *nsIdentifier = makeNSString(identifier);
        
        dispatch_async(serialQueue, ^{
            [ChatSDK fetchRoomWithId:nsRoomId completionHandler:^(NSError * _Nullable error, ChatRoom *room) {
                dispatch_async(serialQueue, ^{
                    NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
                    NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];

                    if (error) {
                        [valueObject setObject:@(NO) forKey:@"result"];
                        [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];
                    } else {
                        [valueObject setObject:@(YES) forKey:@"result"];
                        [valueObject setObject:[room dictionary] forKey:@"room"];
                    }

                    [jsonObject setObject:nsIdentifier forKey:@"identifier"];
                    [jsonObject setObject:valueObject forKey:@"value"];

                    NSLog(@"@@@ [Native] ChatSDK.fetchRoom result: %@", jsonObject);
                    [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                });
            }];
        });
    }

    void ChatSDKCreateRoom(const char* chatRoomOptionJson, const char* identifier) {
        NSString *nsOptionJson = makeNSString(chatRoomOptionJson);
        NSString *nsIdentifier = makeNSString(identifier);

        dispatch_async(serialQueue, ^{
            NSDictionary *chatRoomOptions = [ChatSDKUtils deserializeJSONStringToObject:nsOptionJson];

            NSString *title = [chatRoomOptions objectForKey:@"title"];
            NSString *profileUrl = [chatRoomOptions objectForKey:@"profileUrl"];
            NSArray *userIds = [chatRoomOptions objectForKey:@"userIds"];
            int type = [[chatRoomOptions objectForKey:@"type"] intValue];

            ChatRoomType roomType = type == 0 ? ChatRoomTypeGroup : ChatRoomTypePersonal;
            ChatRoomOption *options = [[ChatRoomOption alloc] initWithUserIds:userIds
                                                                        title:title
                                                                profileUrl:profileUrl
                                                                        type:roomType];

            [ChatSDK createRoomWithOptions:options completionHandler:^(NSError * _Nullable error, ChatRoom *room) {
                dispatch_async(serialQueue, ^{
                    NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
                    
                    if (error) {
                        [valueObject setObject:@(NO) forKey:@"result"];
                        [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];
                    } else {
                        [valueObject setObject:@(YES) forKey:@"result"];
                        [valueObject setObject:[room dictionary] forKey:@"room"];
                    }
                    
                    NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
                    [jsonObject setObject:nsIdentifier forKey:@"identifier"];
                    [jsonObject setObject:valueObject forKey:@"value"];

                    NSLog(@"@@@ [Native] ChatSDK.createRoom result: %@", jsonObject);
                    [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                });
            }];
        });
    }

    void ChatSDKEnter(const char* roomId, const char* identifier) {
        NSString *nsRoomId = makeNSString(roomId);
        NSString *nsIdentifier = makeNSString(identifier);
        
        dispatch_async(serialQueue, ^{
            [ChatSDK enterWithRoomId:nsRoomId
                   completionHandler:^(NSError * _Nullable error, ChatRoomSession *session) {
                dispatch_async(serialQueue, ^{
                    NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];

                    if (error) {
                        [valueObject setObject:@(NO) forKey:@"result"];
                        [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];
                    } else {
                        [roomSessionDic setObject:session forKey:nsRoomId];
                        [valueObject setObject:@(YES) forKey:@"result"];
                    }

                    NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
                    [jsonObject setObject:nsIdentifier forKey:@"identifier"];
                    [jsonObject setObject:valueObject forKey:@"value"];

                    NSLog(@"@@@ [Native] ChatSDK.enter result: %@", jsonObject);
                    [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                });
            }];
        });
    }

    void ChatSDKLeave(const char* roomId, const char* identifier) {
        NSString *nsRoomId = makeNSString(roomId);
        NSString *nsIdentifier = makeNSString(identifier);
        
        dispatch_async(serialQueue, ^{
            [ChatSDK leaveWithRoomId:nsRoomId completionHandler:^(NSError * _Nullable error) {
                dispatch_async(serialQueue, ^{
                    NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];

                    if (error) {
                        [valueObject setObject:@(NO) forKey:@"result"];
                        [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];
                    } else {
                        [roomSessionDic removeObjectForKey:nsRoomId];
                        [valueObject setObject:@(YES) forKey:@"result"];
                    }

                    NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
                    [jsonObject setObject:nsIdentifier forKey:@"identifier"];
                    [jsonObject setObject:valueObject forKey:@"value"];

                    NSLog(@"@@@ [Native] ChatSDK.leave result: %@", jsonObject);
                    [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                });
            }];
        });
    }

    void ChatSDKExit(const char* roomId, const char* identifier) {
        NSString *nsRoomId = makeNSString(roomId);
        NSString *nsIdentifier = makeNSString(identifier);
        
        dispatch_async(serialQueue, ^{
            [ChatSDK exitWithRoomId:nsRoomId completionHandler:^(NSError * _Nullable error) {
                dispatch_async(serialQueue, ^{
                    NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
                    
                    if (error) {
                        [valueObject setObject:@(NO) forKey:@"result"];
                        [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];
                    } else {
                        [roomSessionDic removeObjectForKey:nsRoomId];
                        [valueObject setObject:@(YES) forKey:@"result"];
                        [valueObject setObject:@"" forKey:@"error"];
                    }

                    NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
                    [jsonObject setObject:nsIdentifier forKey:@"identifier"];
                    [jsonObject setObject:valueObject forKey:@"value"];

                    NSLog(@"@@@ [Native] ChatSDK.exit result: %@", jsonObject);
                    [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                });
            }];
        });
    }

    void ChatSDKInvite(const char* roomId, const char* userIdsJson, const char* identifier) {
        NSString *nsRoomId = makeNSString(roomId);
        NSString *nsUserIdsJson = makeNSString(userIdsJson);
        NSString *nsIdentifier = makeNSString(identifier);

        dispatch_async(serialQueue, ^{
            NSArray *userIds = [ChatSDKUtils deserializeJSONStringToObject:nsUserIdsJson];
            [ChatSDK inviteWithRoomId:nsRoomId userIds:userIds completionHandler:^(NSError * _Nullable error) {
                dispatch_async(serialQueue, ^{
                    NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
                    
                    if (error) {
                        [valueObject setObject:@(NO) forKey:@"result"];
                        [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];
                    } else {
                        [valueObject setObject:@(YES) forKey:@"result"];
                    }

                    NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
                    [jsonObject setObject:nsIdentifier forKey:@"identifier"];
                    [jsonObject setObject:valueObject forKey:@"value"];

                    NSLog(@"@@@ [Native] ChatSDK.invite result: %@", jsonObject);
                    [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                });
            }];
        });
    }

    void ChatSDKSetAlarm(const char* roomId, bool alarm, const char* identifier) {
        NSString *nsRoomId = makeNSString(roomId);
        NSString *nsIdentifier = makeNSString(identifier);
        
        dispatch_async(serialQueue, ^{
            [ChatSDK setAlarmWithRoomId:nsRoomId alarm:alarm completionHandler:^(NSError * _Nullable error) {
                dispatch_async(serialQueue, ^{
                    NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];

                    if (error) {
                        [valueObject setObject:@(NO) forKey:@"result"];
                        [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];
                    } else {
                        [valueObject setObject:@(YES) forKey:@"result"];
                    }

                    NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
                    [jsonObject setObject:nsIdentifier forKey:@"identifier"];
                    [jsonObject setObject:valueObject forKey:@"value"];

                    NSLog(@"@@@ [Native] ChatSDK.setAlarm result: %@", jsonObject);
                    [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                });
            }];
        });
    }

    void ChatSDKUpdateCredential(const char* userId, const char* token) {
        NSString *nsUserId = makeNSString(userId);
        NSString *nsToken = makeNSString(token);
        
        dispatch_async(serialQueue, ^{
            ChatCredential *credential = [[ChatCredential alloc] initWithUserId:nsUserId token:nsToken];
            
            [ChatSDK updateCredential:credential];
            NSLog(@"@@@ [Native] ChatSDK.updateCredential: %@", credential);
        });
    }

    void ChatSDKSendMessage(const char* roomId, const char* message, const char* identifier) {
        NSString *nsRoomId = makeNSString(roomId);
        NSString *nsMessage = makeNSString(message);
        NSString *nsIdentifier = makeNSString(identifier);
        
        dispatch_async(serialQueue, ^{
            ChatRoomSession *session = roomSessionDic[nsRoomId];
            
            NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
            [jsonObject setObject:nsIdentifier forKey:@"identifier"];
            
            NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
            
            if (session == nil) {
                [valueObject setObject:@(NO) forKey:@"result"];
                [valueObject setObject:[ChatSDKUtils roomNotFoundError] forKey:@"error"];
                
                [jsonObject setObject:valueObject forKey:@"value"];
                [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                return;
            }
            
            NSDictionary *messageDic = [ChatSDKUtils deserializeJSONStringToObject:nsMessage];
            FormattedMessage *formattedMessage = [[FormattedMessage alloc] initWithDictionary:messageDic];
            [session sendMessage:formattedMessage completionHandler:^(NSError * _Nullable error) {
                if (error) {
                    [valueObject setObject:@(NO) forKey:@"result"];
                    [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];
                } else {
                    [valueObject setObject:@(YES) forKey:@"result"];
                }
                
                [jsonObject setObject:valueObject forKey:@"value"];
                [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
            }];
        });
    }

    void ChatSDKResendMessage(const char* roomId, const char* message, const char* identifier) {
        NSString *nsRoomId = makeNSString(roomId);
        NSString *nsMessage = makeNSString(message);
        NSString *nsIdentifier = makeNSString(identifier);
        
        dispatch_async(serialQueue, ^{
            ChatRoomSession *session = roomSessionDic[nsRoomId];
            
            NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
            [jsonObject setObject:nsIdentifier forKey:@"identifier"];
            
            NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
            
            if (session == nil) {
                [valueObject setObject:@(NO) forKey:@"result"];
                [valueObject setObject:[ChatSDKUtils roomNotFoundError] forKey:@"error"];
                
                [jsonObject setObject:valueObject forKey:@"value"];
                [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                return;
            }
            
            NSDictionary *messageDic = [ChatSDKUtils deserializeJSONStringToObject:nsMessage];
            ChatMessage *chatMessage = [[ChatMessage alloc] initWithDictionary:messageDic];
            [session resendMessage:chatMessage completionHandler:^(NSError * _Nullable error) {
                if (error) {
                    [valueObject setObject:@(NO) forKey:@"result"];
                    [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];
                } else {
                    [valueObject setObject:@(YES) forKey:@"result"];
                }
                
                [jsonObject setObject:valueObject forKey:@"value"];
                [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
            }];
        });
    }

    void ChatSDKFetchLatestMessages(const char* roomId, int count, const char* identifier) {
        NSString *nsRoomId = makeNSString(roomId);
        NSString *nsIdentifier = makeNSString(identifier);
        
        dispatch_async(serialQueue, ^{
            ChatRoomSession *session = roomSessionDic[nsRoomId];
            
            NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
            [jsonObject setObject:nsIdentifier forKey:@"identifier"];
            
            NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
            
            if (session == nil) {
                [valueObject setObject:@(NO) forKey:@"result"];
                [valueObject setObject:[ChatSDKUtils roomNotFoundError] forKey:@"error"];
                
                [jsonObject setObject:valueObject forKey:@"value"];
                [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                return;
            }
            
            [session fetchLatestMessagesWithCount:count completionHandler:^(NSError * _Nullable error, NSArray<ChatMessage *> * _Nullable messages) {
                if (error) {
                    [valueObject setObject:@(NO) forKey:@"result"];
                    [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];
                } else {
                    [valueObject setObject:@(YES) forKey:@"result"];
                    
                    NSMutableArray *messageArray = [NSMutableArray array];
                    for (ChatMessage *message in messages) {
                        [messageArray addObject:[message dictionary]];
                    }
                    [valueObject setObject:messageArray forKey:@"messages"];
                }
                
                [jsonObject setObject:valueObject forKey:@"value"];
                [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
            }];
        });
    }

    void ChatSDKFetchPreviousMessages(const char* roomId, int count, int before, const char* identifier) {
        NSString *nsRoomId = makeNSString(roomId);
        NSString *nsIdentifier = makeNSString(identifier);
        
        dispatch_async(serialQueue, ^{
            ChatRoomSession *session = roomSessionDic[nsRoomId];
            
            NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
            [jsonObject setObject:nsIdentifier forKey:@"identifier"];
            
            NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
            
            if (session == nil) {
                [valueObject setObject:@(NO) forKey:@"result"];
                [valueObject setObject:[ChatSDKUtils roomNotFoundError] forKey:@"error"];
                
                [jsonObject setObject:valueObject forKey:@"value"];
                [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                return;
            }
            
            [session fetchPreviousMessagesWithCount:count before:before completionHandler:^(NSError * _Nullable error, NSArray<ChatMessage *> * _Nullable messages) {
                if (error) {
                    [valueObject setObject:@(NO) forKey:@"result"];
                    [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];
                } else {
                    [valueObject setObject:@(YES) forKey:@"result"];
                    
                    NSMutableArray *messageArray = [NSMutableArray array];
                    for (ChatMessage *message in messages) {
                        [messageArray addObject:[message dictionary]];
                    }
                    [valueObject setObject:messageArray forKey:@"messages"];
                }
                
                [jsonObject setObject:valueObject forKey:@"value"];
                [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
            }];
        });
    }

    void ChatSDKFetchNextMessages(const char* roomId, int count, int since, const char* identifier) {
        NSString *nsRoomId = makeNSString(roomId);
        NSString *nsIdentifier = makeNSString(identifier);
        
        dispatch_async(serialQueue, ^{
            ChatRoomSession *session = roomSessionDic[nsRoomId];
            
            NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
            [jsonObject setObject:nsIdentifier forKey:@"identifier"];
            
            NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
            
            if (session == nil) {
                [valueObject setObject:@(NO) forKey:@"result"];
                [valueObject setObject:[ChatSDKUtils roomNotFoundError] forKey:@"error"];
                
                [jsonObject setObject:valueObject forKey:@"value"];
                [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                return;
            }
            
            [session fetchNextMessagesWithCount:count since:since completionHandler:^(NSError * _Nullable error, NSArray<ChatMessage *> * _Nullable messages) {
                if (error) {
                    [valueObject setObject:@(NO) forKey:@"result"];
                    [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];
                } else {
                    [valueObject setObject:@(YES) forKey:@"result"];
                    
                    NSMutableArray *messageArray = [NSMutableArray array];
                    for (ChatMessage *message in messages) {
                        [messageArray addObject:[message dictionary]];
                    }
                    [valueObject setObject:messageArray forKey:@"messages"];
                }
                
                [jsonObject setObject:valueObject forKey:@"value"];
                [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
            }];
        });
    }

    void ChatSDKRegisterChatRoomEvent(const char* roomId, const char* identifier) {
        NSString *nsRoomId = makeNSString(roomId);
        NSString *nsIdentifier = makeNSString(identifier);
        
        ChatRoomSession *session = [roomSessionDic objectForKey:nsRoomId];
        if (session == nil) {
            return;
        }
        
        ChatSDKRoomDelegate *delegate = [[ChatSDKRoomDelegate alloc] initWithIndentifier:nsIdentifier];
        [roomDelegateDic setObject:delegate forKey:nsRoomId];
        
        session.delegate = delegate;
    }

    void ChatSDKUnregisterChatRoomEvent(const char* roomId) {
        NSString *nsRoomId = makeNSString(roomId);
        
        ChatSDKRoomDelegate *delegate = [roomDelegateDic objectForKey:nsRoomId];
        delegate.eventIdentifier = nil;
        [roomDelegateDic removeObjectForKey:nsRoomId];
        
        ChatRoomSession *session = [roomSessionDic objectForKey:nsRoomId];
        session.delegate = nil;
    }

    const char* ChatSDKGetConnectionStatus() {
        ChatConnectionStatus *status = ChatSDK.connectionStatus;
        NSDictionary *statusDic = [status dictionary];
        return [[ChatSDKUtils serializeObjectToJSONString:statusDic] UTF8String];
    }

    void ChatSDKRegisterConnectionStatus(const char* identifier) {
        NSString *nsIdentifier = makeNSString(identifier);
        [ChatSDKDelegate shared].connectionIdentifier = nsIdentifier;
    }

    void ChatSDKUnregisterConnectionStatus() {
        [ChatSDKDelegate shared].connectionIdentifier = nil;
    }

    void ChatSDKRegisterChatEvent(const char* identifier) {
        NSString *nsIdentifier = makeNSString(identifier);
        [ChatSDKDelegate shared].eventIdentifier = nsIdentifier;
    }

    void ChatSDKUnregisterChatEvent() {
        [ChatSDKDelegate shared].eventIdentifier = nil;
    }

    void ChatSDKFetchUnsentMessages(const char* roomId, const char* identifier) {
        NSString *nsRoomId = makeNSString(roomId);
        NSString *nsIdentifier = makeNSString(identifier);
        
        dispatch_async(serialQueue, ^{
            ChatRoomSession *session = roomSessionDic[nsRoomId];
            
            NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
            [jsonObject setObject:nsIdentifier forKey:@"identifier"];
            
            NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
            
            if (session == nil) {
                [valueObject setObject:@(NO) forKey:@"result"];
                [valueObject setObject:[ChatSDKUtils roomNotFoundError] forKey:@"error"];
                
                [jsonObject setObject:valueObject forKey:@"value"];
                [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                return;
            }
            
            [session fetchUnsentMessagesCompletionHandler:^(NSError * _Nullable error, NSArray<ChatMessage *> * _Nullable messages) {
                if (error) {
                    [valueObject setObject:@(NO) forKey:@"result"];
                    [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];
                } else {
                    [valueObject setObject:@(YES) forKey:@"result"];
                    
                    NSMutableArray *messageArray = [NSMutableArray array];
                    for (ChatMessage *message in messages) {
                        [messageArray addObject:[message dictionary]];
                    }
                    [valueObject setObject:messageArray forKey:@"messages"];
                }
                
                [jsonObject setObject:valueObject forKey:@"value"];
                [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
            }];
        });
    }
    
    void ChatSDKDeleteUnsentMessage(const char* roomId, const char* messageId, const char* identifier) {
        NSString *nsRoomId = makeNSString(roomId);
        NSString *nsMessageId = makeNSString(messageId);
        NSString *nsIdentifier = makeNSString(identifier);
        
        dispatch_async(serialQueue, ^{
            ChatRoomSession *session = roomSessionDic[nsRoomId];
            
            NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
            [jsonObject setObject:nsIdentifier forKey:@"identifier"];
            
            NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
            
            if (session == nil) {
                [valueObject setObject:@(NO) forKey:@"result"];
                [valueObject setObject:[ChatSDKUtils roomNotFoundError] forKey:@"error"];
                
                [jsonObject setObject:valueObject forKey:@"value"];
                [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
                return;
            }
            
            [session deleteUnsentMessage:nsMessageId completionHandler:^(NSError * _Nullable error) {
                if (error) {
                    [valueObject setObject:@(NO) forKey:@"result"];
                    [valueObject setObject:[ChatSDKUtils getErrorJson:error] forKey:@"error"];
                } else {
                    [valueObject setObject:@(YES) forKey:@"result"];
                }
                
                [jsonObject setObject:valueObject forKey:@"value"];
                [ChatSDKUtils sendUnityMessageWithJsonObject:jsonObject];
            }];
        });
    }
}
