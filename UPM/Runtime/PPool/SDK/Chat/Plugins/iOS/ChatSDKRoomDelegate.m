//
//  ChatSDKRoomDelegate.m
//  UnityFramework
//
//  Created by 강정은/SGP PPool개발팀 on 5/16/25.
//

#import "ChatSDKRoomDelegate.h"
#import "ChatSDKUtils.h"

@implementation ChatSDKRoomDelegate

- (instancetype)initWithIndentifier:(NSString *)identifier
{
    self = [super init];
    if (self) {
        self.eventIdentifier = identifier;
    }
    return self;
}

- (void)didReceiveMessage:(ChatMessage *)message {
    if (self.eventIdentifier == nil) {
        return;
    }
    
    NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
    [valueObject setObject:[message dictionary] forKey:@"MessageReceived"];
    
    NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
    [jsonObject setObject:self.eventIdentifier forKey:@"identifier"];
    [jsonObject setObject:valueObject forKey:@"value"];
   
    [ChatSDKUtils notifyUnityMessageWithJsonObject:jsonObject];
}

- (void)didUpdateRoom:(ChatRoom *)room {
    if (self.eventIdentifier == nil) {
        return;
    }
    
    NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
    [valueObject setObject:[room dictionary] forKey:@"RoomUpdated"];
    
    NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
    [jsonObject setObject:self.eventIdentifier forKey:@"identifier"];
    [jsonObject setObject:valueObject forKey:@"value"];
   
    [ChatSDKUtils notifyUnityMessageWithJsonObject:jsonObject];
}

@end
