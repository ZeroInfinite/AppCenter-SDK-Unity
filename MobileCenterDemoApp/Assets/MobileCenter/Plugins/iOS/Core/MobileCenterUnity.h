// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT license.

#import <MobileCenter/MobileCenter.h>

extern "C" void mobile_center_unity_set_log_level(int logLevel);
extern "C" int mobile_center_unity_get_log_level();
extern "C" bool mobile_center_unity_is_configured();
extern "C" void mobile_center_unity_set_log_url(const char* logUrl);
extern "C" void mobile_center_unity_set_enabled(bool isEnabled);
extern "C" bool mobile_center_unity_is_enabled();
extern "C" const char* mobile_center_unity_get_install_id();
extern "C" void mobile_center_unity_set_custom_properties(MSCustomProperties* properties);
extern "C" void mobile_center_unity_set_wrapper_sdk(const char* wrapperSdkVersion,
                                                    const char* wrapperSdkName,
                                                    const char* wrapperRuntimeVersion,
                                                    const char* liveUpdateReleaseLabel,
                                                    const char* liveUpdateDeploymentKey,
                                                    const char* liveUpdatePackageHash);
