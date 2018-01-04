using UnityEngine;
using UnityEditor;
using System.Collections;


namespace TMPro.EditorUtilities
{
    [CustomEditor(typeof(TMP_ColorGradient))]
    public class TMP_ColorGradientEditor : Editor
    {
        private SerializedProperty topLeftColor;
        private SerializedProperty topRightColor;
        private SerializedProperty bottomLeftColor;
        private SerializedProperty bottomRightColor;


        void OnEnable()
        {
            // Get the UI Skin and Styles for the various Editors
            TMP_UIStyleManager.GetUIStyles();

            topLeftColor = serializedObject.FindProperty("topLeft");
            topRightColor = serializedObject.FindProperty("topRight");
            bottomLeftColor = serializedObject.FindProperty("bottomLeft");
            bottomRightColor = serializedObject.FindProperty("bottomRight");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Label("<b>TextMeshPro - Color Gradient Preset</b>", TMP_UIStyleManager.Section_Label);

            EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);

            EditorGUILayout.PropertyField(topLeftColor, new GUIContent("Top Left"));
            EditorGUILayout.PropertyField(topRightColor, new GUIContent("Top Right"));
            EditorGUILayout.PropertyField(bottomLeftColor, new GUIContent("Bottom Left"));
            EditorGUILayout.PropertyField(bottomRightColor, new GUIContent("Bottom Right"));

            EditorGUILayout.EndVertical();


            if (serializedObject.ApplyModifiedProperties())
                TMPro_EventManager.ON_COLOR_GRAIDENT_PROPERTY_CHANGED(target as TMP_ColorGradient);

        }
    }
}
