using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Profiling;

namespace Unity.WorldBuilding.Profiling
{
    public static class ProfilerSnapshotUtils
    {
        private const string k_ProfilerFolder = "ProfilerLogs";
        private static readonly string k_ProjectFolder = Application.dataPath.Replace("/Assets", "");

        private static bool m_IsSilentMode = false;
        private static bool m_IsSnapshotting = false;
        private static GUIStyle m_SceneWarningStyle;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneview)
        {
            EnsureStyleGenerated();

            if (!m_IsSilentMode)
            {
                Handles.BeginGUI();
                GUI.contentColor = Color.white;
                GUI.backgroundColor = Color.black;
                GUI.Box(new Rect(5, 5, 300, 35), "Profiler Snapshot in Progress", m_SceneWarningStyle);
                Handles.EndGUI();
            }
        }

        [MenuItem("Tests/Profiler/Toggle Snapshot %m")]
        private static void ToggleSnapshot()
        {
            if (Profiler.enabled)
            {
                StopSnapshot();
            }
            else
            {
                StartSnapshot(false);
            }
        }

        public static void StartSnapshot(bool silent)
        {
            StartSnapshot(silent, string.Format("[{0:MM-dd-yyyy.HH.mm}]-profiler-log", DateTime.Now));
        }

        public static void StartSnapshot(bool silent, string fileName)
        {
            if (m_IsSnapshotting)
                return;

            if (!Directory.Exists(k_ProfilerFolder))
            {
                Directory.CreateDirectory(k_ProfilerFolder);
            }

            if (silent)
            {
                Debug.LogFormat("Profiler Snapshot Started [editorProfile={0} | deepProfile={1}]",
                    ProfilerDriver.profileEditor, ProfilerDriver.deepProfiling);
            }

            ProfilerDriver.ClearAllFrames();

            Profiler.logFile = string.Format("{0}/{1}", k_ProfilerFolder, fileName);
            Profiler.enableBinaryLog = true;
            Profiler.enabled = true;
            m_IsSilentMode = silent;
            m_IsSnapshotting = true;
        }

        public static void StopSnapshot()
        {
            if (!m_IsSnapshotting)
                return;
            
            m_IsSnapshotting = false;

            Profiler.enableBinaryLog = false;
            Profiler.enabled = false;
            
            if (!m_IsSilentMode)
            {
                string filePath = string.Format("{0}/{1}", k_ProjectFolder, Profiler.logFile);
                Debug.LogFormat("Profiler Snapshot Stopped. Profiler raw file create at path <i>{0}</i>", filePath);
                ProfilerDriver.LoadProfile(filePath, false);
                OpenProfilerWindow();
            }

            m_IsSilentMode = false;
        }

        private static void OpenProfilerWindow()
        {
            Assembly assembly = typeof(EditorWindow).Assembly;
            EditorWindow.GetWindow(assembly.GetType("UnityEditor.ProfilerWindow"));
        }

        private static void EnsureStyleGenerated()
        {
            if (m_SceneWarningStyle == null)
            {
                m_SceneWarningStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 16,
                    fontStyle = FontStyle.Bold
                };
            }
        }
    }
}
