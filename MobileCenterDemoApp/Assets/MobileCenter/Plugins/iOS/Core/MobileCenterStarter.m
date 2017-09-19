#define MOBILE_CENTER_UNITY_USE_DISTRIBUTE
#define MOBILE_CENTER_UNITY_USE_ANALYTICS
#define MOBILE_CENTER_UNITY_USE_PUSH
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT license.

#import "MobileCenterStarter.h"
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@import MobileCenter;

#ifdef MOBILE_CENTER_UNITY_USE_PUSH
@import MobileCenterPush;
#import "../Push/PushDelegate.h"
#endif

#ifdef MOBILE_CENTER_UNITY_USE_ANALYTICS
@import MobileCenterAnalytics;
#endif

#ifdef MOBILE_CENTER_UNITY_USE_DISTRIBUTE
@import MobileCenterDistribute;
#import "../Distribute/DistributeDelegate.h"
#endif

@implementation MobileCenterStarter

static NSString *const kMSAppSecret = @"03786f44-32da-4eed-9e2a-7bb9aeb8b3f4";
static NSString *const kMSCustomLogUrl = @"custom-log-url";
static NSString *const kMSCustomApiUrl = @"custom-api-url";
static NSString *const kMSCustomInstallUrl = @"custom-install-url";

static const int kMSLogLevel = 2;

+ (void)load {
  [[NSNotificationCenter defaultCenter] addObserver:self
                                           selector:@selector(startMobileCenter)
                                               name:UIApplicationDidFinishLaunchingNotification
                                             object:nil];
}

+ (void)startMobileCenter {
  NSMutableArray<Class>* classes = [[NSMutableArray alloc] init];

#ifdef MOBILE_CENTER_UNITY_USE_PUSH
  [MSPush setDelegate:[UnityPushDelegate sharedInstance]];
  [classes addObject:MSPush.class];
#endif

#ifdef MOBILE_CENTER_UNITY_USE_ANALYTICS
  [classes addObject:MSAnalytics.class];
#endif

#ifdef MOBILE_CENTER_UNITY_USE_DISTRIBUTE

#ifdef MOBILE_CENTER_UNITY_USE_CUSTOM_API_URL
  [MSDistribute setApiUrl:kMSCustomApiUrl];
#endif // MOBILE_CENTER_UNITY_USE_CUSTOM_API_URL

#ifdef MOBILE_CENTER_UNITY_USE_CUSTOM_INSTALL_URL
  [MSDistribute setInstallUrl:kMSCustomInstallUrl];
#endif // MOBILE_CENTER_UNITY_USE_CUSTOM_INSTALL_URL
  [MSDistribute setDelegate:[UnityDistributeDelegate sharedInstance]];
  [classes addObject:MSDistribute.class];

#endif // MOBILE_CENTER_UNITY_USE_DISTRIBUTE

  [MSMobileCenter setLogLevel:(MSLogLevel)kMSLogLevel];

#ifdef MOBILE_CENTER_UNITY_USE_CUSTOM_LOG_URL
  [MSMobileCenter setLogUrl:kMSCustomLogUrl];
#endif

  [MSMobileCenter start:kMSAppSecret withServices:classes];
}

@end
