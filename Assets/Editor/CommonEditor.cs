using UnityEditor;
using UnityEngine;

namespace ETD.Editor
{
    public class CommonEditor : EditorWindow
    {
        public bool preprocessorDirectiveGroupEnabled;
        public bool isTest;
        public bool isLive;

        private readonly string _prefKey = "CommonDebuggerData";
        private readonly string _preprocessTestKey = "IS_TEST";
        private readonly string _preprocessLiveKey = "IS_LIVE";

        // Add menu item named "My Window" to the Window menu
        [MenuItem("Tools/Common Debuger")]
        public static void ShowWindow()
        {
            GetWindow(typeof(CommonEditor));
        }
    
        private void OnEnable()
        {
            var data = EditorPrefs.GetString(_prefKey, JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(data, this);

            var preprocessorKey = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (preprocessorKey == _preprocessLiveKey)
                isLive = true;
            else if (preprocessorKey == _preprocessTestKey)
                isTest = true;
            
            preprocessorDirectiveGroupEnabled = false;
        }

        private void OnGUI()
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        
            GUILayout.Space(10);
        
            preprocessorDirectiveGroupEnabled = EditorGUILayout.BeginToggleGroup("Preprocessor Directive", preprocessorDirectiveGroupEnabled);
            GUILayout.Space(5);
            
            isTest = EditorGUILayout.Toggle("IS_TEST", isTest);
            isLive = EditorGUILayout.Toggle("IS_LIVE", isLive);

            if (isTest)
                isLive = false;
            else if (isLive)
                isTest = false;
            
            EditorGUILayout.EndToggleGroup();
        
            GUILayout.Space(10);
        
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            if (GUILayout.Button("Apply"))
            {
                var data = JsonUtility.ToJson(this, false);
                EditorPrefs.SetString(_prefKey, data);

                var preprocessorKey = "";
                preprocessorKey += isTest ? $"{_preprocessTestKey};" : "";
                preprocessorKey += isLive ? _preprocessLiveKey : "";

                Debug.Log(preprocessorKey);
            
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                    preprocessorKey);
            }
            GUILayout.EndHorizontal();
        }
    }
}