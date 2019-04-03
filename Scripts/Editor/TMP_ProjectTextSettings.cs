using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace TMPro
{

    public static class TMP_ProjectTextSettings
    {

        // Open Project Text Settings
        [MenuItem("Window/TextMeshPro/TMP Settings", false, 2000)]
        public static void SelectProjectTextSettings()
        {
            TMP_Settings textSettings = TMP_Settings.instance;
            Selection.activeObject = textSettings;

            // TODO: Do we want to ping the Project Text Settings asset in the Project Inspector
            //EditorUtility.FocusProjectWindow();
            //EditorGUIUtility.PingObject(textSettings);
        }
    }
}
