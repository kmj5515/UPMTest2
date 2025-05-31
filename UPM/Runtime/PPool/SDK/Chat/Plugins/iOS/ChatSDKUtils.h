#import <Foundation/Foundation.h>

#ifndef ChatSDKUtils_h
#define ChatSDKUtils_h

@interface ChatSDKUtils : NSObject
+ (void)sendUnityMessageWithJsonObject:(NSDictionary *)jsonObject;
+ (void)notifyUnityMessageWithJsonObject:(NSDictionary *)jsonObject;

+ (NSString *)serializeObjectToJSONString:(id)object;
+ (id)deserializeJSONStringToObject:(NSString *)jsonString;

+ (NSDictionary *)getErrorJson:(NSError *)error;
+ (NSDictionary *)roomNotFoundError;

@end

static inline NSString *makeNSString(const char *string) {
    if (string) {
        return [NSString stringWithUTF8String:string];
    }
    return @"";
}

#endif /* ChatSDKUtils_h */ 
