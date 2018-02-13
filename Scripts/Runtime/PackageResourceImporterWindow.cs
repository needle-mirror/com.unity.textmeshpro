#if UNITY_EDITOR

using System.IO;
using UnityEngine;
using UnityEditor;
 

namespace TMPro
{ 
    public class PackageResourceImporterWindow : EditorWindow
    {
        public static void ShowPackageImporterWindow()
        {
            var window = GetWindow<PackageResourceImporterWindow>();
            window.titleContent = new GUIContent("TMP Importer");
            window.Focus();
        }


        public GUISkin LightSkin;
        public GUISkin DarkSkin;

        static GUIStyle k_Label;
        static GUIStyle k_SectionLabel;
        static GUIStyle k_SquareAreaBoxGrey;

        [SerializeField]
        bool k_EssentialResourcesImported;
        [SerializeField]
        bool k_ExamplesAndExtrasResourcesImported;
        [SerializeField]
        bool k_IsImportingExamples;


        void OnEnable()
        {
            // Set Editor Window Size
            SetEditorWindowSize();

            // Get the UI Skin and Styles for the various Editors
            GetGUIStyles();

            // Special handling due to scripts imported in a .unitypackage result in resulting in an assembly reload which clears the callbacks.
            if (k_IsImportingExamples)
            {
                AssetDatabase.importPackageCompleted += ImportCallback;
                k_IsImportingExamples = false;
            }
        }

        private void OnDestroy()
        {
            k_EssentialResourcesImported = false;
            k_ExamplesAndExtrasResourcesImported = false;
        }


        void OnGUI()
        {
            bool importEssentialsPackage = false;
            bool importExamplesPackage = false;

            GUILayout.BeginVertical();
            {
                GUILayout.Label("<b>TextMesh Pro Resources Importer</b>", k_SectionLabel);

                // Display options to import Essential resources
                GUILayout.BeginVertical(k_SquareAreaBoxGrey);
                {
                    GUILayout.Label("<b>TMP Essentials</b>", k_Label);
                    GUILayout.Label("This appears to be the first time you access TextMesh Pro, as such we need to add resources to your project that are essential for using TextMesh Pro. These new resources will be placed at the root of your project in the \"TextMesh Pro\" folder.", k_Label);
                    GUILayout.Space(5f);

                    GUI.enabled = k_EssentialResourcesImported == false ? true : false;
                    if (GUILayout.Button("Import TMP Essentials"))
                    {
                        importEssentialsPackage = true;
                    }
                    GUILayout.Space(5f);
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();

                // Display options to import Examples & Extras
                GUILayout.BeginVertical(k_SquareAreaBoxGrey);
                {
                    GUILayout.Label("<b>TMP Examples & Extras</b>", k_Label);
                    GUILayout.Label("The Examples & Extras package contains addition resources and examples that will make discovering and learning about TextMesh Pro's powerful features easier. These additional resources will be placed in the same folder as the TMP essential resources.", k_Label);
                    GUILayout.Space(5f);

                    GUI.enabled = (k_EssentialResourcesImported == true && k_ExamplesAndExtrasResourcesImported == false) ? true : false;
                    if (GUILayout.Button("Import TMP Examples & Extras"))
                    {
                        importExamplesPackage = true;
                    }
                    GUILayout.Space(5f);
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
            GUILayout.Space(5f);

            // Import Essential Resources 
            if (importEssentialsPackage)
            {
                AssetDatabase.importPackageCompleted += ImportCallback;

                string packageFullPath = GetPackageFullPath();
                AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/TMP Essential Resources.unitypackage", false);
            }

            // Import Examples & Extras
            if (importExamplesPackage)
            {
                // Set flag to get around importing scripts as per of this package which results in an assembly reload which in turn prevents / clears any callbacks.
                k_IsImportingExamples = true;

                string packageFullPath = GetPackageFullPath();
                AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/TMP Examples & Extras.unitypackage", false);
            }
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }


        void GetGUIStyles()
        {
            GUISkin skin;

            if (EditorGUIUtility.isProSkin)
                skin = DarkSkin;
            else
                skin = LightSkin;

            k_Label = skin.FindStyle("Label");
            k_SectionLabel = skin.FindStyle("Section Label");
            k_SquareAreaBoxGrey = skin.FindStyle("Square Area Box (85 Grey)");

        }


        /// <summary>
        /// Limits the minimum size of the editor window.
        /// </summary>
        void SetEditorWindowSize()
        {
            EditorWindow editorWindow = this;

            Vector2 windowSize = new Vector2(640, 240);
            editorWindow.minSize = windowSize;
            editorWindow.maxSize = windowSize;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        void ImportCallback(string packageName)
        {
            if (packageName == "TMP Essential Resources")
            {
                k_EssentialResourcesImported = true;
                TMPro_EventManager.ON_RESOURCES_LOADED();
            }
            else if (packageName == "TMP Examples & Extras")
            {
                k_ExamplesAndExtrasResourcesImported = true;
                k_IsImportingExamples = false;
            }

            Debug.Log("[" + packageName + "] have been imported.");

            AssetDatabase.importPackageCompleted -= ImportCallback;
        }


        string GetPackageFullPath()
        {
            // Check for potential UPM package
            string packagePath = Path.GetFullPath("Packages/com.unity.textmeshpro");
            if (Directory.Exists(packagePath))
            {
                return packagePath;
            }

            packagePath = Path.GetFullPath("Assets/..");
            if (Directory.Exists(packagePath))
            {
                // Search default location for development package
                if (Directory.Exists(packagePath + "/Assets/Packages/com.unity.TextMeshPro/Editor Resources"))
                {
                    return packagePath + "/Assets/Packages/com.unity.TextMeshPro";
                }

                // Search for default location of normal TextMesh Pro AssetStore package
                if (Directory.Exists(packagePath + "/Assets/TextMesh Pro/Editor Resources"))
                {
                    return packagePath + "/Assets/TextMesh Pro";
                }

                // Search for potential alternative locations in the user project
                string[] matchingPaths = Directory.GetDirectories(packagePath, "TextMesh Pro", SearchOption.AllDirectories);
                string path = ValidateLocation(matchingPaths, packagePath);
                if (path != null) return packagePath + path;
            }

            return null;
        }

        string ValidateLocation(string[] paths, string projectPath)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                // Check if the Editor Resources folder exists.
                if (Directory.Exists(paths[i] + "/Editor Resources"))
                {
                    string folderPath = paths[i].Replace(projectPath, "");
                    folderPath = folderPath.TrimStart('\\', '/');
                    return folderPath;
                }
            }

            return null;
        }
    }

}

#endif
