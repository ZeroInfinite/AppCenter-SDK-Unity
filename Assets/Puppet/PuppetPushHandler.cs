// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT license.

using Microsoft.Azure.Mobile.Unity.Push;
using UnityEngine;

public class PuppetPushHandler : MonoBehaviour
{
    private static PushNotificationReceivedEventArgs _pushEventArgs = null;
    private static object _pushLock = new object();

    void Awake()
    {
        Push.PushNotificationReceived += (sender, e) =>
        {
            lock (_pushLock)
            {
                _pushEventArgs = e;
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (_pushEventArgs == null)
        {
            return;
        }
        lock (_pushLock)
        {
            if (_pushEventArgs == null)
            {
                return;
            }
            var pushSummary = "Push notification received:" +
                "\n\tNotification title: " + _pushEventArgs.Title +
                "\n\tMessage: " + _pushEventArgs.Message;
            if (_pushEventArgs.CustomData != null)
            {
                pushSummary += "\n\tCustom data:\n";
                foreach (var key in _pushEventArgs.CustomData.Keys)
                {
                    pushSummary += "\t\t" + key + " : " + _pushEventArgs.CustomData[key] + "\n";
                }
            }
            print(pushSummary);
            _pushEventArgs = null;
        }
    }
}
