//
//  ChatSDKRoomDelegate.h
//  UnityFramework
//
//  Created by 강정은/SGP PPool개발팀 on 5/16/25.
//

#import <Foundation/Foundation.h>
#import <PPoolChat/PPoolChat-Swift.h>

NS_ASSUME_NONNULL_BEGIN

@interface ChatSDKRoomDelegate : NSObject <ChatRoomDelegate>

@property (nonatomic, nullable) NSString *eventIdentifier;

- (instancetype)initWithIndentifier:(NSString *)identifier;

@end

NS_ASSUME_NONNULL_END
