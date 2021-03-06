// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT license.

#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using UnityEngine;

namespace Microsoft.AppCenter.Unity.Distribute.Internal
{
    class DistributeInternal
    {
        private static AndroidJavaClass _distribute = new AndroidJavaClass("com.microsoft.appcenter.distribute.Distribute");

        public static void PrepareEventHandlers()
        {
            AppCenterBehavior.InitializingServices += Initialize;
            AppCenterBehavior.Started += StartBehavior;
        }

        private static void Initialize()
        {
            DistributeDelegate.SetDelegate();
        }

        private static void StartBehavior()
        {
            var instance = _distribute.CallStatic<AndroidJavaObject>("getInstance");
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            instance.Call("onActivityResumed", activity);
        }

        public static AppCenterTask SetEnabledAsync(bool isEnabled)
        {
            var future = _distribute.CallStatic<AndroidJavaObject>("setEnabled", isEnabled);
            return new AppCenterTask(future);
        }

        public static AppCenterTask<bool> IsEnabledAsync()
        {
            var future = _distribute.CallStatic<AndroidJavaObject>("isEnabled");
            return new AppCenterTask<bool>(future);
        }

        public static IntPtr GetNativeType()
        {
            return AndroidJNI.FindClass("com/microsoft/appcenter/distribute/Distribute");
        }

        public static void SetInstallUrl(string installUrl)
        {
            _distribute.CallStatic("setInstallUrl", installUrl);
        }

        public static void SetApiUrl(string apiUrl)
        {
            _distribute.CallStatic("setApiUrl", apiUrl);
        }

        public static void NotifyUpdateAction(int updateAction)
        {
            _distribute.CallStatic("notifyUpdateAction", updateAction);
        }
    }
}
#endif
