// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT license.

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

[CustomEditor(typeof(MobileCenterSettings))]
public class MobileCenterSettingsEditor : Editor
{
    public const string SettingsPath = "Assets/MobileCenter/MobileCenterSettings.asset";

    public static MobileCenterSettings Settings
    {
        get
        {
            return AssetDatabase.LoadAssetAtPath<MobileCenterSettings>(SettingsPath);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw app secrets.
        Header("App Secrets");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("iOSAppSecret"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("AndroidAppSecret"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("UWPAppSecret"));

        // Draw modules.
        Header("Modules");
        if (MobileCenterSettings.Analytics != null)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UseAnalytics"));
        }
        if (MobileCenterSettings.Crashes != null)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UseCrashes"));
        }
        if (MobileCenterSettings.Distribute != null)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UseDistribute"));
        }
        if (MobileCenterSettings.Push != null)
        {
            var serializedProperty = serializedObject.FindProperty("UsePush");
            EditorGUILayout.PropertyField(serializedProperty);
#if !UNITY_2017_1_OR_NEWER
            if (serializedProperty.boolValue)
            {
                EditorGUILayout.HelpBox ("In Unity versions prior to 2017.1 you need to add required capabilities in XCode manually.", MessageType.Info);
            }
#endif
        }

        // Draw other.
        Header("Other Setup");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("InitialLogLevel"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("CustomLogUrl"));

        serializedObject.ApplyModifiedProperties();
    }

    [PostProcessScene]
    static void AddStartupCodeToAndroid()
    {
        var settings = Settings;
        if (settings == null)
        {
            return;
        }
        var settingsMaker = new MobileCenterSettingsMakerAndroid();
        settingsMaker.SetAppSecret(settings.AndroidAppSecret);
        if (settings.CustomLogUrl.UseCustomLogUrl)
        {
            settingsMaker.SetLogUrl(settings.CustomLogUrl.LogUrl);
        }
        if (settings.UsePush)
        {
            settingsMaker.StartPushClass();
        }
        if (settings.UseAnalytics)
        {
            settingsMaker.StartAnalyticsClass();
        }
        if (settings.UseDistribute)
        {
            settingsMaker.StartDistributeClass();
        }
        if (settings.UseCrashes)
        {
            settingsMaker.StartCrashesClass();
        }
        settingsMaker.SetLogLevel((int)settings.InitialLogLevel);
        settingsMaker.CommitSettings();
    }

    private static void Header(string label)
    {
        GUILayout.Label(label, EditorStyles.boldLabel);
        GUILayout.Space(-4);
    }
}
