﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AppCenterEditor
{
    public class AppCenterEditor : EditorWindow
    {
        public enum EdExStates { OnMenuItemClicked, OnHttpReq, OnHttpRes, OnError, OnSuccess, OnWarning }

        public delegate void AppCenterEdExStateHandler(EdExStates state, string status, string misc);
        public static event AppCenterEdExStateHandler EdExStateUpdate;

        public static Dictionary<string, float> blockingRequests = new Dictionary<string, float>(); // key and blockingRequest start time
        private static float blockingRequestTimeOut = 10f; // abandon the block after this many seconds.

        public static string latestEdExVersion = string.Empty;

        internal static AppCenterEditor window;

        void OnEnable()
        {
            if (window == null)
            {
                window = this;
                window.minSize = new Vector2(320, 0);
            }

            if (!IsEventHandlerRegistered(StateUpdateHandler))
            {
                EdExStateUpdate += StateUpdateHandler;
            }

            GetLatestEdExVersion();
        }

        void OnDisable()
        {
            AppCenterEditorPrefsSO.Instance.PanelIsShown = false;

            if (IsEventHandlerRegistered(StateUpdateHandler))
            {
                EdExStateUpdate -= StateUpdateHandler;
            }
        }

        void OnFocus()
        {
            OnEnable();
        }

        [MenuItem("Window/App Center/Editor Extensions")]
        static void AppCenterServices()
        {
            var editorAsm = typeof(Editor).Assembly;
            var inspWndType = editorAsm.GetType("UnityEditor.SceneHierarchyWindow");

            if (inspWndType == null)
            {
                inspWndType = editorAsm.GetType("UnityEditor.InspectorWindow");
            }

            window = GetWindow<AppCenterEditor>(inspWndType);
            window.titleContent = new GUIContent("AppCenter EdEx");
            AppCenterEditorPrefsSO.Instance.PanelIsShown = true;
        }

        [InitializeOnLoad]
        public class Startup
        {
            static Startup()
            {
                if (AppCenterEditorPrefsSO.Instance.PanelIsShown || !AppCenterEditorSDKTools.IsInstalled)
                {
                    EditorCoroutine.Start(OpenAppCenterServices());
                }
            }
        }

        static IEnumerator OpenAppCenterServices()
        {
            yield return new WaitForSeconds(1f);
            if (!Application.isPlaying)
            {
                AppCenterServices();
            }
        }

        private void OnGUI()
        {
            HideRepaintErrors(OnGuiInternal);
        }

        private static void HideRepaintErrors(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (!e.Message.ToLower().Contains("repaint"))
                    throw;
                // Hide any repaint issues when recompiling
            }
        }

        private void OnGuiInternal()
        {
            GUI.skin = AppCenterEditorHelper.uiStyle;

            using (new UnityVertical())
            {
                GUI.enabled = blockingRequests.Count == 0 && !EditorApplication.isCompiling;

                //Run all updaters prior to drawing;
                AppCenterEditorHeader.DrawHeader();

                AppCenterEditorMenu.DrawMenu();

                AppCenterEditorSDKTools.DrawSdkPanel();

                DisplayEditorExtensionHelpMenu();
            }

            PruneBlockingRequests();

            Repaint();
        }

        private static void DisplayEditorExtensionHelpMenu()
        {
            using (new UnityVertical(AppCenterEditorHelper.uiStyle.GetStyle("gpStyleGray1")))
            {
                using (new UnityHorizontal(AppCenterEditorHelper.uiStyle.GetStyle("gpStyleClear")))
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("App Center Editor Extensions: " + AppCenterEditorHelper.EDEX_VERSION, AppCenterEditorHelper.uiStyle.GetStyle("versionText"));
                    GUILayout.FlexibleSpace();
                }

                if (ShowEdExUpgrade())
                {
                    using (new UnityHorizontal())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("UPGRADE EDITOR EXTENSION", AppCenterEditorHelper.uiStyle.GetStyle("textButtonOr")))
                        {
                            UpgradeEdEx();
                        }
                        GUILayout.FlexibleSpace();
                    }
                }

                if (!string.IsNullOrEmpty(AppCenterEditorHelper.EDEX_ROOT))
                {
                    using (new UnityHorizontal())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("UNINSTALL EDITOR EXTENSION", AppCenterEditorHelper.uiStyle.GetStyle("textButton")))
                        {
                            RemoveEdEx();
                        }
                        GUILayout.FlexibleSpace();
                    }
                }
            }
        }

        #region menu and helper methods

        public static void RaiseStateUpdate(EdExStates state, string status = null, string json = null)
        {
            if (EdExStateUpdate != null)
            {
                EdExStateUpdate(state, status, json);
            }
        }

        private static void PruneBlockingRequests()
        {
            List<string> itemsToRemove = new List<string>();
            foreach (var req in blockingRequests)
                if (req.Value + blockingRequestTimeOut < (float)EditorApplication.timeSinceStartup)
                    itemsToRemove.Add(req.Key);

            foreach (var item in itemsToRemove)
            {
                ClearBlockingRequest(item);
                RaiseStateUpdate(EdExStates.OnWarning, string.Format(" Request {0} has timed out after {1} seconds.", item, blockingRequestTimeOut));
            }
        }

        private static void AddBlockingRequest(string state)
        {
            blockingRequests[state] = (float)EditorApplication.timeSinceStartup;
        }

        private static void ClearBlockingRequest(string state = null)
        {
            if (state == null)
            {
                blockingRequests.Clear();
            }
            else if (blockingRequests.ContainsKey(state))
            {
                blockingRequests.Remove(state);
            }
        }
        /// <summary>
        /// Handles state updates within the editor extension.
        /// </summary>
        /// <param name="state">the state that triggered this event.</param>
        /// <param name="status">a generic message about the status.</param>
        /// <param name="json">a generic container for additional JSON encoded info.</param>
        private void StateUpdateHandler(EdExStates state, string status, string json)
        {
            switch (state)
            {
                case EdExStates.OnMenuItemClicked:
                    break;

                case EdExStates.OnHttpReq:
                    break;

                case EdExStates.OnHttpRes:
                    break;

                case EdExStates.OnError:
                    ProgressBar.UpdateState(ProgressBar.ProgressBarStates.error);
                    Debug.LogError(string.Format("App Center EditorExtensions: Caught an error:{0}", status));
                    break;

                case EdExStates.OnWarning:
                    ProgressBar.UpdateState(ProgressBar.ProgressBarStates.warning);
                    Debug.LogWarning(string.Format("App Center EditorExtensions: {0}", status));
                    break;

                case EdExStates.OnSuccess:
                    ProgressBar.UpdateState(ProgressBar.ProgressBarStates.success);
                    break;
            }
        }

        public static bool IsEventHandlerRegistered(AppCenterEdExStateHandler prospectiveHandler)
        {
            if (EdExStateUpdate == null)
                return false;

            foreach (AppCenterEdExStateHandler existingHandler in EdExStateUpdate.GetInvocationList())
                if (existingHandler == prospectiveHandler)
                    return true;
            return false;
        }

        #endregion

        private static void GetLatestEdExVersion()
        {
            var threshold = AppCenterEditorPrefsSO.Instance.EdSet_lastEdExVersionCheck != DateTime.MinValue ? AppCenterEditorPrefsSO.Instance.EdSet_lastEdExVersionCheck.AddHours(1) : DateTime.MinValue;

            if (DateTime.Today > threshold)
            {
                AppCenterEditorHttp.MakeGitHubApiCall("https://api.github.com/repos/Microsoft/AppCenter-SDK-Unity/git/refs/tags", (version) =>
                {
                    latestEdExVersion = version ?? "Unknown";
                    AppCenterEditorPrefsSO.Instance.EdSet_latestEdExVersion = latestEdExVersion;
                });
            }
            else
            {
                latestEdExVersion = AppCenterEditorPrefsSO.Instance.EdSet_latestEdExVersion;
            }
        }

        private static bool ShowEdExUpgrade()
        {
            if (string.IsNullOrEmpty(latestEdExVersion) || latestEdExVersion == "Unknown")
                return false;

            if (string.IsNullOrEmpty(AppCenterEditorHelper.EDEX_VERSION) || AppCenterEditorHelper.EDEX_VERSION == "Unknown")
                return true;

            string[] currrent = AppCenterEditorHelper.EDEX_VERSION.Split('.');
            if (currrent.Length != 3)
                return true;

            string[] latest = latestEdExVersion.Split('.');
            return latest.Length != 3
                || int.Parse(latest[0]) > int.Parse(currrent[0])
                || int.Parse(latest[1]) > int.Parse(currrent[1])
                || int.Parse(latest[2]) > int.Parse(currrent[2]);
        }

        private static void RemoveEdEx(bool prompt = true)
        {
            if (prompt && !EditorUtility.DisplayDialog("Confirm Editor Extensions Removal", "This action will remove App Center Editor Extensions from the current project.", "Confirm", "Cancel"))
                return;

            try
            {
                window.Close();
                var edExRoot = new DirectoryInfo(AppCenterEditorHelper.EDEX_ROOT);
                FileUtil.DeleteFileOrDirectory(edExRoot.Parent.FullName);
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        private static void UpgradeEdEx()
        {
            if (EditorUtility.DisplayDialog("Confirm EdEx Upgrade", "This action will remove the current App Center Editor Extensions and install the lastet version.", "Confirm", "Cancel"))
            {
                window.Close();
                ImportLatestEdEx();
            }
        }

        private static void ImportLatestEdEx()
        {

        }
    }
}