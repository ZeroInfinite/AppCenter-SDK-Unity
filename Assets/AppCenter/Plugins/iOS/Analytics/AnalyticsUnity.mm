// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT license.

#import "AnalyticsUnity.h"
#import "../Core/Utility/NSStringDictionaryHelper.h"
#import "AppCenterAnalytics/MSAnalyticsTransmissionTarget.h"
#import <AppCenterAnalytics/AppCenterAnalytics.h>
#import <Foundation/Foundation.h>

void* appcenter_unity_analytics_get_type()
{
  return (void *)CFBridgingRetain([MSAnalytics class]);
}

void appcenter_unity_analytics_track_event(char* eventName)
{
  [MSAnalytics trackEvent:[NSString stringWithUTF8String:eventName]];
}

void appcenter_unity_analytics_track_event_with_properties(char* eventName, char** keys, char** values, int count)
{
  NSDictionary<NSString*, NSString*> *properties = appcenter_unity_create_ns_string_dictionary(keys, values, count);
  NSString * eventNameString = [NSString stringWithUTF8String:eventName];
  [MSAnalytics trackEvent:eventNameString withProperties:properties];
}

void appcenter_unity_analytics_track_event_with_typed_properties(char* eventName, MSEventProperties* properties)
{
    [MSAnalytics trackEvent:[NSString stringWithUTF8String:eventName] withTypedProperties:properties];
}

void appcenter_unity_analytics_set_enabled(bool isEnabled)
{
  [MSAnalytics setEnabled:isEnabled];
}

bool appcenter_unity_analytics_is_enabled()
{
  return [MSAnalytics isEnabled];
}

MSAnalyticsTransmissionTarget *appcenter_unity_analytics_transmission_target_for_token(char* transmissionTargetToken) {
  return [MSAnalytics transmissionTargetForToken: [NSString stringWithUTF8String:transmissionTargetToken]];
}

void appcenter_unity_analytics_pause()
{
    [MSAnalytics pause];
}

void appcenter_unity_analytics_resume()
{
    [MSAnalytics resume];
}

