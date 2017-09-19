﻿// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT license.

#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using UnityEngine;
using Microsoft.Azure.Mobile.Internal.Utility;

namespace Microsoft.Azure.Mobile.Unity.Internal
{
    class CustomPropertiesInternal
    {
        public static AndroidJavaObject Create()
        {
            return new AndroidJavaObject("com.microsoft.azure.mobile.CustomProperties");
        }

        public static void SetString(AndroidJavaObject properties, string key, string val)
        {
            properties.Call<AndroidJavaObject>("set", key, val);
        }

        public static void SetNumber(AndroidJavaObject properties, string key, int val)
        {
            properties.Call<AndroidJavaObject>("set", key, JavaNumberHelper.Convert(val));
        }

        public static void SetNumber(AndroidJavaObject properties, string key, long val)
        {
            properties.Call<AndroidJavaObject>("set", key, JavaNumberHelper.Convert(val));
        }

        public static void SetNumber(AndroidJavaObject properties, string key, float val)
        {
            properties.Call<AndroidJavaObject>("set", key, JavaNumberHelper.Convert(val));
        }

        public static void SetNumber(AndroidJavaObject properties, string key, double val)
        {
            properties.Call<AndroidJavaObject>("set", key, JavaNumberHelper.Convert(val));
        }

        public static void SetBool(AndroidJavaObject properties, string key, bool val)
        {
            properties.Call<AndroidJavaObject>("set", key, val);
        }

        public static void SetDate(AndroidJavaObject properties, string key, DateTime val)
        {
            properties.Call<AndroidJavaObject>("set", key, JavaDateHelper.DateTimeConvert(val));
        }

        public static void Clear(AndroidJavaObject properties, string key)
        {
            properties.Call<AndroidJavaObject>("clear", key);
        }
    }
}
#endif
