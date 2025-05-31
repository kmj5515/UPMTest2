//
//  ChatSDKDelegate.h
//  UnityFramework
//
//  Created by 강정은/SGP PPool개발팀 on 5/16/25.
//

#import <Foundation/Foundation.h>
#import <PPoolChat/PPoolChat-Swift.h>

NS_ASSUME_NONNULL_BEGIN

@interface ChatSDKDelegate : NSObject <ChatDelegate>

@property (nonatomic, nullable) NSString *eventIdentifier;
@property (nonatomic, nullable) NSString *connectionIdentifier;

+ (ChatSDKDelegate *)shared;

@end

NS_ASSUME_NONNULL_END
