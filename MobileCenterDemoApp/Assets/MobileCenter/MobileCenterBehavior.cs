﻿// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT license.

using Microsoft.Azure.Mobile.Unity;
using UnityEngine;
using System;
using System.Reflection;
using Microsoft.Azure.Mobile.Unity.Internal;

[HelpURL("https://docs.microsoft.com/en-us/mobile-center/sdk/")]
public class MobileCenterBehavior : MonoBehaviour
{
    public static event Action InitializingServices;
    public static event Action InitializedMobileCenterAndServices;
    public static event Action Started;

    private static MobileCenterBehavior instance;

    public MobileCenterSettings settings;

    private void Awake()
    {
        // Make sure that Mobile Center have only one instance.
        if (instance != null)
        {
            Debug.LogError("Mobile Center should have only one instance!");
            DestroyImmediate(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize Mobile Center.
        if (settings == null)
        {
            Debug.LogError("Mobile Center isn't configured!");
            return;
        }
        StartMobileCenter();
    }

    private void Start()
    {
        if (Started != null)
        {
            Started.Invoke();
        }
    }

    private void StartMobileCenter()
    {
        var services = settings.Services;
        PrepareEventHandlers(services);
        InvokeInitializingServices();
        MobileCenter.SetWrapperSdk();

        // On iOS and Android Mobile Center starting automatically.
#if UNITY_EDITOR || (!UNITY_IOS && !UNITY_ANDROID)
        MobileCenter.LogLevel = settings.InitialLogLevel;
        if (settings.CustomLogUrl.UseCustomUrl)
        {
            MobileCenter.SetLogUrl(settings.CustomLogUrl.Url);
        }
        var appSecret = MobileCenter.GetSecretForPlatform(settings.AppSecret);
        var nativeServiceTypes = MobileCenter.ServicesToNativeTypes(services);
        MobileCenterInternal.Start(appSecret, nativeServiceTypes, services.Length);
#endif
        InvokeInitializedServices();
    }

    private static void PrepareEventHandlers(Type[] services)
    {
        foreach (var service in services)
        {
            var method = service.GetMethod("PrepareEventHandlers");
            if (method != null)
            {
                method.Invoke(null, null);
            }
        }
    }

    private static void InvokeInitializingServices()
    {
        if (InitializingServices != null)
        {
            InitializingServices.Invoke();
        }
    }

    private static void InvokeInitializedServices()
    {
        if (InitializedMobileCenterAndServices != null)
        {
            InitializedMobileCenterAndServices.Invoke();
        }
    }
}
