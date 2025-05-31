#import "ChatSDKUtils.h"

@implementation ChatSDKUtils

+ (NSString *)ChatSDKMakeNSString:(const char *)string {
    if (string) {
        return [NSString stringWithUTF8String:string];
    }
    return @"";
}

+ (void)sendUnityMessageWithJsonObject:(NSDictionary *)jsonObject {
    NSString *jsonString = [ChatSDKUtils serializeObjectToJSONString:jsonObject];
    NSLog(@"@@@ [Native] UnitySendMessage: %@", jsonString);

    dispatch_async(dispatch_get_main_queue(), ^{
        UnitySendMessage("PPoolSDK", "CallMessage", [jsonString UTF8String]);
    });
}

+ (void)notifyUnityMessageWithJsonObject:(NSDictionary *)jsonObject {
    NSString *jsonString = [ChatSDKUtils serializeObjectToJSONString:jsonObject];
    NSLog(@"@@@ [Native] UnitySendMessage: %@", jsonString);

    dispatch_async(dispatch_get_main_queue(), ^{
        UnitySendMessage("PPoolSDK", "NotifyMessage", [jsonString UTF8String]);
    });
}

+ (NSString *)serializeObjectToJSONString:(id)object {
    if (![NSJSONSerialization isValidJSONObject:object]) {
        NSLog(@"[JSONHelper] 직렬화 오류: 객체는 JSON으로 직렬화할 수 없습니다.");
        return nil;
    }

    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:object options:NSJSONWritingPrettyPrinted error:&error];
    if (!jsonData) {
        NSLog(@"[JSONHelper] 직렬화 오류: %@", error.localizedDescription);
        return nil;
    }

    return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
}

+ (id)deserializeJSONStringToObject:(NSString *)jsonString {
    NSData *jsonData = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
    if (!jsonData) {
        NSLog(@"[JSONHelper] 역직렬화 오류: JSON 문자열을 NSData로 변환할 수 없습니다.");
        return nil;
    }

    NSError *error;
    id jsonObject = [NSJSONSerialization JSONObjectWithData:jsonData options:NSJSONReadingMutableContainers error:&error];
    if (!jsonObject) {
        NSLog(@"[JSONHelper] 역직렬화 오류: %@", error.localizedDescription);
    }

    return jsonObject;
}

+ (NSDictionary *)getErrorJson:(NSError *)error {
    NSMutableDictionary *errorDic = [NSMutableDictionary dictionary];
    [errorDic setObject:[error localizedDescription] forKey:@"message"];
    
    switch (error.code) {
        case 0:
            [errorDic setObject:@"NotInitialized" forKey:@"type"];
            break;
            
        case 1:
            [errorDic setObject:@"ConnectionFailed" forKey:@"type"];
            break;
            
        case 2:
            [errorDic setObject:@"AuthenticationDenied" forKey:@"type"];
            break;
            
        case 3:
            [errorDic setObject:@"PermissionDenied" forKey:@"type"];
            break;
            
        case 4:
            [errorDic setObject:@"NotParticipated" forKey:@"type"];
            break;
            
        case 5:
            [errorDic setObject:@"RoomNotFound" forKey:@"type"];
            break;
            
        case 6:
            [errorDic setObject:@"BanWords" forKey:@"type"];
            break;
        
        case 7:
            [errorDic setObject:@"Server" forKey:@"type"];
            break;

        case 8:
            [errorDic setObject:@"NotConnected" forKey:@"type"];
            break;
            
        default:
            break;
    }
    
    return [NSDictionary dictionaryWithDictionary:errorDic];
}

+ (NSDictionary *)roomNotFoundError {
    return [NSDictionary dictionaryWithObjectsAndKeys:
            @"RoomNotFound", @"type",
            @"Chat Room not found. It may been deleted or you don't have access.", @"message",
            nil];
}

@end 
