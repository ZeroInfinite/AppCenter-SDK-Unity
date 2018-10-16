﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AppCenterEditor
{
    public class AppCenterEditorSDKTools : Editor
    {
        public static bool IsInstalled { get { return GetAppCenterSettings() != null; } }

        private const string AnalyticsDownloadFormat = "https://github.com/Microsoft/AppCenter-SDK-Unity/releases/download/{0}/AppCenterAnalytics-v{0}.unitypackage";
        private const string CrashesDownloadFormat = "https://github.com/Microsoft/AppCenter-SDK-Unity/releases/download/{0}/AppCenterCrashes-v{0}.unitypackage";
        private const string DistributeDownloadFormat = "https://github.com/Microsoft/AppCenter-SDK-Unity/releases/download/{0}/AppCenterDistribute-v{0}.unitypackage";
        private static Type appCenterSettingsType = null;
        private static bool isInitialized; //used to check once, gets reset after each compile;
        private static string installedSdkVersion = string.Empty;
        private static string latestSdkVersion = string.Empty;
        private static UnityEngine.Object sdkFolder;
        private static UnityEngine.Object _previousSdkFolderPath;
        private static bool isObjectFieldActive;
        public static bool isSdkSupported = true;

        public static void DrawSdkPanel()
        {
            if (!isInitialized)
            {
                //SDK is installed.
                CheckSdkVersion();
                isInitialized = true;
                GetLatestSdkVersion();
                sdkFolder = FindSdkAsset();

                if (sdkFolder != null)
                {
                    AppCenterEditorPrefsSO.Instance.SdkPath = AssetDatabase.GetAssetPath(sdkFolder);
                    // AppCenterEditorDataService.SaveEnvDetails();
                }
            }

            if (IsInstalled)
            {
                ShowSdkInstalledMenu();
            }
            else
            {
                ShowSdkNotInstalledMenu();
            }
        }

        private static void ShowSdkInstalledMenu()
        {
            isObjectFieldActive = sdkFolder == null;

            if (_previousSdkFolderPath != sdkFolder)
            {
                // something changed, better save the result.
                _previousSdkFolderPath = sdkFolder;

                AppCenterEditorPrefsSO.Instance.SdkPath = (AssetDatabase.GetAssetPath(sdkFolder));
                //TODO: check if we need this?
                // AppCenterEditorDataService.SaveEnvDetails();

                isObjectFieldActive = false;
            }

            var labelStyle = new GUIStyle(AppCenterEditorHelper.uiStyle.GetStyle("titleLabel"));
            using (new AppCenterGuiFieldHelper.UnityVertical(AppCenterEditorHelper.uiStyle.GetStyle("gpStyleGray1")))
            {
                EditorGUILayout.LabelField(string.Format("SDK {0} is installed", string.IsNullOrEmpty(installedSdkVersion) ? "Unknown" : installedSdkVersion),
                    labelStyle, GUILayout.MinWidth(EditorGUIUtility.currentViewWidth));

                if (!isObjectFieldActive)
                {
                    GUI.enabled = false;
                }
                else
                {
                    EditorGUILayout.LabelField(
                        "An SDK was detected, but we were unable to find the directory. Drag-and-drop the top-level App Center SDK folder below.",
                        AppCenterEditorHelper.uiStyle.GetStyle("orTxt"));
                }

                using (new AppCenterGuiFieldHelper.UnityHorizontal(AppCenterEditorHelper.uiStyle.GetStyle("gpStyleClear")))
                {
                    GUILayout.FlexibleSpace();
                    sdkFolder = EditorGUILayout.ObjectField(sdkFolder, typeof(UnityEngine.Object), false, GUILayout.MaxWidth(200));
                    GUILayout.FlexibleSpace();
                }

                if (!isObjectFieldActive)
                {
                    // this is a hack to prevent our "block while loading technique" from breaking up at this point.
                    GUI.enabled = !EditorApplication.isCompiling && AppCenterEditor.blockingRequests.Count == 0;
                }

                if (isSdkSupported && sdkFolder != null)
                {
                    using (new AppCenterGuiFieldHelper.UnityHorizontal(AppCenterEditorHelper.uiStyle.GetStyle("gpStyleClear")))
                    {
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("REMOVE SDK", AppCenterEditorHelper.uiStyle.GetStyle("textButton"), GUILayout.MinHeight(32), GUILayout.MinWidth(200)))
                        {
                            RemoveSdk();
                        }

                        GUILayout.FlexibleSpace();
                    }
                }
            }

            if (sdkFolder != null)
            {
                using (new AppCenterGuiFieldHelper.UnityVertical(AppCenterEditorHelper.uiStyle.GetStyle("gpStyleGray1")))
                {
                    isSdkSupported = false;
                    string[] versionNumber = !string.IsNullOrEmpty(installedSdkVersion) ? installedSdkVersion.Split('.') : new string[0];

                    var numerical = 0;
                    if (string.IsNullOrEmpty(installedSdkVersion) || versionNumber == null || versionNumber.Length == 0 ||
                        (versionNumber.Length > 0 && int.TryParse(versionNumber[0], out numerical) && numerical < 0))
                    {
                        //older version of the SDK
                        using (new AppCenterGuiFieldHelper.UnityHorizontal(AppCenterEditorHelper.uiStyle.GetStyle("gpStyleClear")))
                        {
                            EditorGUILayout.LabelField("SDK is outdated. Consider upgrading to the get most features.", AppCenterEditorHelper.uiStyle.GetStyle("orTxt"));
                        }
                    }
                    else if (numerical >= 0)
                    {
                        isSdkSupported = true;
                    }

                    using (new AppCenterGuiFieldHelper.UnityHorizontal(AppCenterEditorHelper.uiStyle.GetStyle("gpStyleClear")))
                    {
                        if (ShowSDKUpgrade() && isSdkSupported)
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Upgrade to " + latestSdkVersion, AppCenterEditorHelper.uiStyle.GetStyle("Button"), GUILayout.MinHeight(32)))
                            {
                                UpgradeSdk();
                            }
                            GUILayout.FlexibleSpace();
                        }
                        else if (isSdkSupported)
                        {
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.LabelField("You have the latest SDK!", labelStyle, GUILayout.MinHeight(32));
                            GUILayout.FlexibleSpace();
                        }
                    }
                }
            }

            using (new AppCenterGuiFieldHelper.UnityHorizontal(AppCenterEditorHelper.uiStyle.GetStyle("gpStyleGray1")))
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("VIEW RELEASE NOTES", AppCenterEditorHelper.uiStyle.GetStyle("textButton"), GUILayout.MinHeight(32), GUILayout.MinWidth(200)))
                {
                    Application.OpenURL("https://github.com/Microsoft/AppCenter-SDK-Unity/releases");
                }

                GUILayout.FlexibleSpace();
            }
        }

        private static void ShowSdkNotInstalledMenu()
        {
            using (new AppCenterGuiFieldHelper.UnityVertical(AppCenterEditorHelper.uiStyle.GetStyle("gpStyleGray1")))
            {
                var labelStyle = new GUIStyle(AppCenterEditorHelper.uiStyle.GetStyle("titleLabel"));

                EditorGUILayout.LabelField("No SDK is installed.", labelStyle, GUILayout.MinWidth(EditorGUIUtility.currentViewWidth));
                GUILayout.Space(20);

                using (new AppCenterGuiFieldHelper.UnityHorizontal(AppCenterEditorHelper.uiStyle.GetStyle("gpStyleGray1")))
                {
                    var buttonWidth = 200;

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Refresh", AppCenterEditorHelper.uiStyle.GetStyle("Button"), GUILayout.MaxWidth(buttonWidth), GUILayout.MinHeight(32)))
                        appCenterSettingsType = null;
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Install App Center SDK", AppCenterEditorHelper.uiStyle.GetStyle("Button"), GUILayout.MaxWidth(buttonWidth), GUILayout.MinHeight(32)))
                        ImportLatestSDK();
                    GUILayout.FlexibleSpace();
                }
            }
        }

        public static void ImportLatestSDK()
        {
            var downloadUrls = new[]
            {
                string.Format(AnalyticsDownloadFormat, latestSdkVersion),
                string.Format(CrashesDownloadFormat, latestSdkVersion),
                string.Format(DistributeDownloadFormat, latestSdkVersion)
            };
            AppCenterEditorHttp.MakeDownloadCall(downloadUrls, downloadedFiles =>
            {
                foreach (var file in downloadedFiles)
                {
                    Debug.Log("Importing package: " + file);
                    AssetDatabase.ImportPackage(file, false);
                    Debug.Log("Deleteing file: " + file);
                    FileUtil.DeleteFileOrDirectory(file);
                }
                AppCenterEditorPrefsSO.Instance.SdkPath = AppCenterEditorHelper.DEFAULT_SDK_LOCATION;
                //AppCenterEditorDataService.SaveEnvDetails();
            });
        }

        public static Type GetAppCenterSettings()
        {
            if (appCenterSettingsType == typeof(object))
                return null; // Sentinel value to indicate that AppCenterSettings doesn't exist
            if (appCenterSettingsType != null)
                return appCenterSettingsType;

            appCenterSettingsType = typeof(object); // Sentinel value to indicate that AppCenterSettings doesn't exist
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in allAssemblies)
                foreach (var eachType in assembly.GetTypes())
                    if (eachType.Name == AppCenterEditorHelper.APPCENTER_SETTINGS_TYPENAME)
                        appCenterSettingsType = eachType;
            //if (appCenterSettingsType == typeof(object))
            //    Debug.LogWarning("Should not have gotten here: "  + allAssemblies.Length);
            //else
            //    Debug.Log("Found Settings: " + allAssemblies.Length + ", " + appCenterSettingsType.Assembly.FullName);
            return appCenterSettingsType == typeof(object) ? null : appCenterSettingsType;
        }

        private static bool ShowSDKUpgrade()
        {
            if (string.IsNullOrEmpty(latestSdkVersion) || latestSdkVersion == "Unknown")
            {
                return false;
            }

            if (string.IsNullOrEmpty(installedSdkVersion) || installedSdkVersion == "Unknown")
            {
                return true;
            }

            string[] currrent = installedSdkVersion.Split('.');
            string[] latest = latestSdkVersion.Split('.');

            return int.Parse(latest[0]) > int.Parse(currrent[0])
                || int.Parse(latest[1]) > int.Parse(currrent[1])
                || int.Parse(latest[2]) > int.Parse(currrent[2]);
        }

        private static void UpgradeSdk()
        {
            if (EditorUtility.DisplayDialog("Confirm SDK Upgrade", "This action will remove the current App Center SDK and install the lastet version.", "Confirm", "Cancel"))
            {
                //RemoveSdk(false);
                ImportLatestSDK();
            }
        }

        private static void RemoveSdk(bool prompt = true)
        {
            if (prompt && !EditorUtility.DisplayDialog("Confirm SDK Removal", "This action will remove the current App Center SDK.", "Confirm", "Cancel"))
            {
                return;
            }

            if (Directory.Exists(Application.dataPath + "/Plugins/Android/res/values"))
            {
                var files = Directory.GetFiles(Application.dataPath + "/Plugins/Android/res/values", "appcenter-settings.xml*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    FileUtil.DeleteFileOrDirectory(file);
                }
            }

            if (FileUtil.DeleteFileOrDirectory(AppCenterEditorPrefsSO.Instance.SdkPath))
            {
                AppCenterEditor.RaiseStateUpdate(AppCenterEditor.EdExStates.OnSuccess, "App Center SDK removed.");

                // HACK for 5.4, AssetDatabase.Refresh(); seems to cause the install to fail.
                if (prompt)
                {
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                AppCenterEditor.RaiseStateUpdate(AppCenterEditor.EdExStates.OnError, "An unknown error occured and the App Center SDK could not be removed.");
            }
        }

        private static void CheckSdkVersion()
        {
            if (!string.IsNullOrEmpty(installedSdkVersion))
                return;

            var types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                        if (type.Name == AppCenterEditorHelper.APPCENTER_WRAPPER_SDK_TYPENAME)
                            types.Add(type);
                }
                catch (ReflectionTypeLoadException)
                {
                    // For this failure, silently skip this assembly unless we have some expectation that it contains App Center
                    if (assembly.FullName.StartsWith("Assembly-CSharp")) // The standard "source-code in unity proj" assembly name
                        Debug.LogWarning("App Center Editor Extension error, failed to access the main CSharp assembly that probably contains App Center SDK");
                    continue;
                }
            }

            foreach (var type in types)
            {
                foreach (var field in type.GetFields())
                {
                    if (field.Name == "WrapperSdkVersion")
                    {
                        installedSdkVersion = field.GetValue(field).ToString();
                        break;
                    }
                }
            }
        }

        private static void GetLatestSdkVersion()
        {
            var threshold = AppCenterEditorPrefsSO.Instance.EdSet_lastSdkVersionCheck != DateTime.MinValue ? AppCenterEditorPrefsSO.Instance.EdSet_lastSdkVersionCheck.AddHours(1) : DateTime.MinValue;

            if (DateTime.Today > threshold)
            {
                AppCenterEditorHttp.MakeGitHubApiCall("https://api.github.com/repos/Microsoft/AppCenter-SDK-Unity/git/refs/tags", (version) =>
                {
                    latestSdkVersion = version ?? "Unknown";
                    AppCenterEditorPrefsSO.Instance.EdSet_latestSdkVersion = latestSdkVersion;
                });
            }
            else
            {
                latestSdkVersion = AppCenterEditorPrefsSO.Instance.EdSet_latestSdkVersion;
            }
        }

        private static UnityEngine.Object FindSdkAsset()
        {
            UnityEngine.Object sdkAsset = null;

            // look in editor prefs
            if (AppCenterEditorPrefsSO.Instance.SdkPath != null)
            {
                sdkAsset = AssetDatabase.LoadAssetAtPath(AppCenterEditorPrefsSO.Instance.SdkPath, typeof(UnityEngine.Object));
            }
            if (sdkAsset != null)
                return sdkAsset;

            sdkAsset = AssetDatabase.LoadAssetAtPath(AppCenterEditorHelper.DEFAULT_SDK_LOCATION, typeof(UnityEngine.Object));
            if (sdkAsset != null)
                return sdkAsset;

            var fileList = Directory.GetDirectories(Application.dataPath, "*AppCenter", SearchOption.AllDirectories);
            if (fileList.Length == 0)
                return null;

            var relPath = fileList[0].Substring(fileList[0].LastIndexOf("Assets" + Path.DirectorySeparatorChar));
            return AssetDatabase.LoadAssetAtPath(relPath, typeof(UnityEngine.Object));
        }
    }
}