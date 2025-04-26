using System;
using ETD.Scripts.Common;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
namespace ETD.Editor
{
    [CustomEditor(typeof(CustomButton))]
    [CanEditMultipleObjects]
    public class CustomButtonEditor : ButtonEditor
    {
        private SerializedProperty downScaleProperty;
        private SerializedProperty upScaleProperty;
        private SerializedProperty isContinuousClickProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            upScaleProperty = serializedObject.FindProperty("upScale");
            downScaleProperty = serializedObject.FindProperty("downScale");
            isContinuousClickProperty = serializedObject.FindProperty("isContinuousClick");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.Space();
            var customButton = (CustomButton)target;
            
            serializedObject.Update();
            EditorGUILayout.PropertyField(downScaleProperty);
            EditorGUILayout.PropertyField(upScaleProperty);
            EditorGUILayout.PropertyField(isContinuousClickProperty);
            serializedObject.ApplyModifiedProperties();
           // customButton.UpScale = EditorGUILayout.FloatField("Up Scale", customButton.UpScale);
           // customButton.DownScale = EditorGUILayout.FloatField("Down Scale", customButton.DownScale);
        }
    }
}
#endif