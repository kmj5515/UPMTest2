//
//  ChatSDKDelegate.m
//  UnityFramework
//
//  Created by 강정은/SGP PPool개발팀 on 5/16/25.
//

#import "ChatSDKDelegate.h"
#import "ChatSDKUtils.h"

@implementation ChatSDKDelegate

+ (ChatSDKDelegate *)shared {
    static ChatSDKDelegate *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[ChatSDKDelegate alloc] init];
    });
    return sharedInstance;
}

- (void)didCreateRoom:(ChatRoom *)room {
    if (self.eventIdentifier == nil) {
        return;
    }
    
    NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
    [valueObject setObject:[NSNumber numberWithInt:0] forKey:@"type"];
    [valueObject setObject:[room dictionary] forKey:@"room"];
    
    NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
    [jsonObject setObject:self.eventIdentifier forKey:@"identifier"];
    [jsonObject setObject:valueObject forKey:@"value"];
   
    [ChatSDKUtils notifyUnityMessageWithJsonObject:jsonObject];
}

- (void)didInviteRoom:(ChatRoom *)room {
    if (self.eventIdentifier == nil) {
        return;
    }
    
    NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
    [valueObject setObject:[NSNumber numberWithInt:1] forKey:@"type"];
    [valueObject setObject:[room dictionary] forKey:@"room"];
    
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
    [valueObject setObject:[NSNumber numberWithInt:2] forKey:@"type"];
    [valueObject setObject:[room dictionary] forKey:@"room"];
    
    NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
    [jsonObject setObject:self.eventIdentifier forKey:@"identifier"];
    [jsonObject setObject:valueObject forKey:@"value"];
   
    [ChatSDKUtils notifyUnityMessageWithJsonObject:jsonObject];
}

- (void)didDeleteRoom:(NSString *)roomId {
    if (self.eventIdentifier == nil) {
        return;
    }
    
    NSMutableDictionary *valueObject = [NSMutableDictionary dictionary];
    [valueObject setObject:[NSNumber numberWithInt:3] forKey:@"type"];
    [valueObject setObject:roomId forKey:@"roomId"];
    
    NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
    [jsonObject setObject:self.eventIdentifier forKey:@"identifier"];
    [jsonObject setObject:valueObject forKey:@"value"];
   
    [ChatSDKUtils notifyUnityMessageWithJsonObject:jsonObject];
}

- (void)didChangeConnectionStatus:(ChatConnectionStatus *)status {
    if (self.connectionIdentifier == nil) {
        return;
    }
    
    NSMutableDictionary *jsonObject = [NSMutableDictionary dictionary];
    [jsonObject setObject:self.connectionIdentifier forKey:@"identifier"];
    [jsonObject setObject:[status dictionary] forKey:@"value"];
   
    [ChatSDKUtils notifyUnityMessageWithJsonObject:jsonObject];
}

@end
