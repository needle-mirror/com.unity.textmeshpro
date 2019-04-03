using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;


namespace TMPro
{
    public class TMP_PackageConversionUtility : Editor {

        enum SaveAssetDialogueOptions { Unset = 0, Save = 1, SaveAll = 2, DoNotSave = 3 };

        private static SerializationMode m_ProjectAssetSerializationMode;
        private static string m_ProjectExternalVersionControl;

        struct AssetRecord
        {
            public string oldGuid;
            public string newGuid;
            public string assetPath;
        }

        // Create Sprite Asset Editor Window
        //[MenuItem("Window/TextMeshPro/Generate New Package GUIDs", false, 1500)]
        public static void GenerateNewPackageGUIDs_Menu()
        {
            GenerateNewPackageGUIDs();
        }


        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Window/TextMeshPro/Import Examples and Extra Content", false, 1500)]
        public static void ImportExamplesContentMenu()
        {
            ImportExtraContent();
        }


        // Create Sprite Asset Editor Window
        [MenuItem("Window/TextMeshPro/Convert TMP Project Files to UPM", false, 1510)]
        public static void ConvertProjectGUIDsMenu()
        {
            ConvertProjectGUIDsToUPM();

            //GetVersionInfo();
        }


        // Create Sprite Asset Editor Window
        //[MenuItem("Window/TextMeshPro/Convert GUID (Source to DLL)", false, 2010)]
        public static void ConvertGUIDFromSourceToDLLMenu()
        {
            //ConvertGUIDFromSourceToDLL();

            //GetVersionInfo();
        }


        // Create Sprite Asset Editor Window
        //[MenuItem("Window/TextMeshPro/Convert GUID (DLL to Source)", false, 2020)]
        public static void ConvertGUIDFromDllToSourceMenu()
        {
            //ConvertGUIDFromDLLToSource();

            //GetVersionInfo();
        }


        // Create Sprite Asset Editor Window
        //[MenuItem("Window/TextMeshPro/Extract Package GUIDs", false, 1530)]
        public static void ExtractPackageGUIDMenu()
        {
            ExtractPackageGUIDs();
        }


        private static void GetVersionInfo()
        {
            string file = Path.GetFullPath("Assets/TextMesh Pro/Plugins/TextMeshPro.dll");

            Debug.Log(System.Diagnostics.FileVersionInfo.GetVersionInfo(file).FileVersion);
            //Debug.Log(System.Diagnostics.FileVersionInfo.GetVersionInfo(file).ProductName);
        }


        /// <summary>
        /// 
        /// </summary>
        private static void ImportExtraContent()
        {
            string packageFullPath = EditorUtilities.TMP_EditorUtility.packageFullPath;

            AssetDatabase.ImportPackage(packageFullPath + "/Examples/TMP Examples.unitypackage", true);
        }


        /// <summary>
        /// 
        /// </summary>
        private static void GenerateNewPackageGUIDs()
        {
            // Make sure Asset Serialization mode is set to ForceText with Visible Meta Files.
            SetProjectSerializationAndSourceControlModes();

            string projectPath = Path.GetFullPath("Assets/..");

            // Clear existing dictionary of AssetRecords
            List<AssetRecord> assetRecords = new List<AssetRecord>();

            // Get full list of GUIDs used in the package which including folders.
            string[] packageGUIDs = AssetDatabase.FindAssets("t:Object", new string[] { "Assets/Packages/com.unity.TextMeshPro" });

            for (int i = 0; i < packageGUIDs.Length; i++)
            {
                // Could add a progress bar for this process (if needed)

                string guid = packageGUIDs[i];
                string assetFilePath = AssetDatabase.GUIDToAssetPath(guid);
                string assetMetaFilePath = AssetDatabase.GetTextMetaFilePathFromAssetPath(assetFilePath);
                //System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetFilePath);

                AssetRecord assetRecord;
                assetRecord.oldGuid = guid;
                assetRecord.assetPath = assetFilePath;

                string newGUID = GenerateUniqueGUID();

                assetRecord.newGuid = newGUID;

                if (assetRecords.FindIndex(item => item.oldGuid == guid) != -1)
                    continue;

                assetRecords.Add(assetRecord);

                // Read the meta file for the given asset.
                string assetMetaFile = File.ReadAllText(projectPath + "/" + assetMetaFilePath);

                assetMetaFile = assetMetaFile.Replace("guid: " + guid, "guid: " + newGUID);

                File.WriteAllText(projectPath + "/" + assetMetaFilePath, assetMetaFile);

                //Debug.Log("Asset: [" + assetFilePath + "]   Type: " + assetType + "   Current GUID: [" + guid + "]   New GUID: [" + newGUID + "]");
            }

            AssetDatabase.Refresh();

            // Get list of GUIDs for assets that might need references to previous GUIDs which need to be updated.
            packageGUIDs = AssetDatabase.FindAssets("t:Object"); //  ("t:Object", new string[] { "Assets/Asset Importer" });

            for (int i = 0; i < packageGUIDs.Length; i++)
            {
                // Could add a progress bar for this process

                string guid = packageGUIDs[i];
                string assetFilePath = AssetDatabase.GUIDToAssetPath(guid);
                System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetFilePath);

                // Filter out file types we are not interested in
                if (assetType == typeof(DefaultAsset) || assetType == typeof(MonoScript) || assetType == typeof(Texture2D) || assetType == typeof(TextAsset) || assetType == typeof(Shader))
                    continue;

                // Read the asset data file
                string assetDataFile = File.ReadAllText(projectPath + "/" + assetFilePath);

                //Debug.Log("Searching Asset: [" + assetFilePath + "] of type: " + assetType);

                bool hasFileChanged = false;

                foreach (AssetRecord record in assetRecords)
                {
                    if (assetDataFile.Contains(record.oldGuid))
                    {
                        hasFileChanged = true;

                        assetDataFile = assetDataFile.Replace(record.oldGuid, record.newGuid);

                        Debug.Log("Replacing old GUID: [" + record.oldGuid + "] by new GUID: [" + record.newGuid + "] in asset file: [" + assetFilePath + "].");
                    }
                }

                if (hasFileChanged)
                {
                    // Add file to list of changed files
                    File.WriteAllText(projectPath + "/" + assetFilePath, assetDataFile);
                }

            }

            AssetDatabase.Refresh();

            // Restore project Asset Serialization and Source Control modes.
            RestoreProjectSerializationAndSourceControlModes();
        }


        private static void ExtractPackageGUIDs()
        {
            // Make sure Asset Serialization mode is set to ForceText with Visible Meta Files.
            SetProjectSerializationAndSourceControlModes();

            string projectPath = Path.GetFullPath("Assets/..");

            // Create new instance of AssetConversionData file
            AssetConversionData data = new AssetConversionData();
            data.assetRecords = new List<AssetConversionRecord>();

            // Get full list of GUIDs used in the package which including folders.
            string[] packageGUIDs = AssetDatabase.FindAssets("t:Object", new string[] { "Assets/Packages/com.unity.TextMeshPro" });

            for (int i = 0; i < packageGUIDs.Length; i++)
            {
                // Could add a progress bar for this process (if needed)

                string guid = packageGUIDs[i];
                string assetFilePath = AssetDatabase.GUIDToAssetPath(guid);
                //string assetMetaFilePath = AssetDatabase.GetTextMetaFilePathFromAssetPath(assetFilePath);

                //ObjectIdentifier[] localIdentifider = BundleBuildInterface.GetPlayerObjectIdentifiersInAsset(new GUID(guid), BuildTarget.NoTarget);
                //System.Type[] types = BundleBuildInterface.GetTypeForObjects(localIdentifider);

                System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetFilePath);

                // Filter out file types we are not interested in
                if (assetType == typeof(DefaultAsset))
                    continue;

                string newGuid = GenerateUniqueGUID();

                AssetConversionRecord record;
                record.referencedResource = Path.GetFileName(assetFilePath);
                record.target = "fileID: 2108210716, guid: " + newGuid;

                record.replacement = "fileID: 11500000, guid: " + guid;

                //if (m_AssetRecords.FindIndex(item => item.oldGuid == guid) != -1)
                //    continue;

                data.assetRecords.Add(record);

                // Read the meta file for the given asset.
                //string assetMetaFile = File.ReadAllText(projectPath + "/" + assetMetaFilePath);

                //assetMetaFile = assetMetaFile.Replace("guid: " + guid, "guid: " + newGUID);

                //File.WriteAllText(projectPath + "/" + assetMetaFilePath, assetMetaFile);

                Debug.Log("Asset: [" + Path.GetFileName(assetFilePath) + "]   Type: " + assetType + "   Current GUID: [" + guid + "]   New GUID: [" + newGuid + "]");
            }

            // Write new information into JSON file
            string dataFile = JsonUtility.ToJson(data, true);

            File.WriteAllText(projectPath + "/Assets/Packages/com.unity.TextMeshPro/PackageConversionData.json", dataFile);

            // Restore project Asset Serialization and Source Control modes.
            RestoreProjectSerializationAndSourceControlModes();
        }


        /// <summary>
        /// 
        /// </summary>
        private static void ConvertProjectGUIDsToUPM()
        {
            // Display a dialogue to get confirmation from the user that they have backed up their project.

            // Make sure Asset Serialization mode is set to ForceText with Visible Meta Files.
            SetProjectSerializationAndSourceControlModes();

            string projectPath = Path.GetFullPath("Assets/..");
            string packageFullPath = EditorUtilities.TMP_EditorUtility.packageFullPath;

            //SaveAssetDialogueOptions saveOptions = SaveAssetDialogueOptions.Unset;

            // Read Conversion Data from Json file.
            AssetConversionData conversionData = JsonUtility.FromJson<AssetConversionData>(File.ReadAllText(packageFullPath + "/PackageConversionData.json"));

            // Get list of GUIDs for assets that might need references to previous GUIDs which need to be updated.
            string[] packageGUIDs = AssetDatabase.FindAssets("t:Object");

            for (int i = 0; i < packageGUIDs.Length; i++)
            {
                // Could add a progress bar for this process

                string guid = packageGUIDs[i];
                string assetFilePath = AssetDatabase.GUIDToAssetPath(guid);
                System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetFilePath);

                // Filter out file types we are not interested in
                if (assetType == typeof(DefaultAsset) || assetType == typeof(MonoScript) || assetType == typeof(Texture2D) || assetType == typeof(TextAsset) || assetType == typeof(Shader))
                    continue;

                // Read the asset data file
                string assetDataFile;
                { assetDataFile = File.ReadAllText(projectPath + "/" + assetFilePath); }

                //Debug.Log("Searching Asset: [" + assetFilePath + "] of type: " + assetType);

                bool hasFileChanged = false;

                foreach (AssetConversionRecord record in conversionData.assetRecords)
                {
                    if (assetDataFile.Contains(record.target))
                    {
                        hasFileChanged = true;

                        assetDataFile = assetDataFile.Replace(record.target, record.replacement);

                        Debug.Log("Replacing Reference to [" + record.referencedResource + "] using [" + record.target + "] with [" + record.replacement + "] in asset file: [" + assetFilePath + "].");
                    }
                }

                if (hasFileChanged)
                {
                    Debug.Log("Writing Asset file [" + assetFilePath + "].");

                    File.WriteAllText(projectPath + "/" + assetFilePath, assetDataFile);
                }

            }

            AssetDatabase.Refresh();

            // Restore project Asset Serialization and Source Control modes.
            RestoreProjectSerializationAndSourceControlModes();
        }



        private static void ConvertGUIDFromSourceToDLL()
        {
            // Check if Asset Serialization Mode is ForceText
            if (UnityEditor.EditorSettings.serializationMode != SerializationMode.ForceText)
                UnityEditor.EditorSettings.serializationMode = SerializationMode.ForceText;

            if (UnityEditor.EditorSettings.externalVersionControl != "Visible Meta Files")
                UnityEditor.EditorSettings.externalVersionControl = "Visible Meta Files";

            string projectPath = Path.GetFullPath("Assets/..");

            // Update the GUID of the runtime DLL & editor only DLL
            //string metaDataFile = File.ReadAllText(projectPath + "/Assets/TextMesh Pro/Plugins/TextMeshPro.dll.meta");
            //metaDataFile = Regex.Replace(metaDataFile, @"guid:\s[0-9a-fA-F]+", "guid: 89f0137620f6af44b9ba852b4190e64e");
            //File.WriteAllText(projectPath + "/Assets/TextMesh Pro/Plugins/TextMeshPro.dll.meta", metaDataFile);

            //metaDataFile = File.ReadAllText(projectPath + "/Assets/TextMesh Pro/Plugins/Editor/TextMeshPro-Editor.dll.meta");
            //metaDataFile = Regex.Replace(metaDataFile, @"guid:\s[0-9a-fA-F]+", "guid: c9131c575d2b6464b8d6e7dbd1a25e2a");
            //File.WriteAllText(projectPath + "/Assets/TextMesh Pro/Plugins/Editor/TextMeshPro-Editor.dll.meta", metaDataFile);

            SaveAssetDialogueOptions saveOptions = SaveAssetDialogueOptions.Unset;


            string[] allProjectObjects = AssetDatabase.FindAssets("t:Prefab t:Scene t:ScriptableObject"); // new string[] { "Assets/TextMesh Pro/Examples/Textures" });

            for (int i = 0; i < allProjectObjects.Length; i++)
            {
                string guid = allProjectObjects[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                //Debug.Log("Searching asset: " + assetPath);

                string assetDataFile = File.ReadAllText(projectPath + "/" + assetPath);

                bool hasFileChanged = false;

                // TMP_Text script
                if (assetDataFile.Contains("m_Script: {fileID: 11500000, guid: 9ec8dc9c3fa2e5d41b939b5888d2f1e8, type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 11500000, guid: 9ec8dc9c3fa2e5d41b939b5888d2f1e8, type: 3}", "m_Script: {fileID: -1385168320, guid: 89f0137620f6af44b9ba852b4190e64e, type: 3}");
                    Debug.Log("Updated references for TMP_Text script in file: " + assetPath);
                }


                // TextMeshPro component
                if (assetDataFile.Contains("m_Script: {fileID: 11500000, guid: 1a1578b9753d2604f98d608cb4239e2f, type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 11500000, guid: 1a1578b9753d2604f98d608cb4239e2f, type: 3}", "m_Script: {fileID: -806885394, guid: 89f0137620f6af44b9ba852b4190e64e, type: 3}");
                    Debug.Log("Updated references for TextMeshPro component in file: " + assetPath);
                }

                // TextMeshProUGUI component
                if (assetDataFile.Contains("m_Script: {fileID: 11500000, guid: 496f2e385b0c62542b5c739ccfafd8da, type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 11500000, guid: 496f2e385b0c62542b5c739ccfafd8da, type: 3}", "m_Script: {fileID: 1453722849, guid: 89f0137620f6af44b9ba852b4190e64e, type: 3}");
                    Debug.Log("Updated references for TextMeshProUGUI component in file: " + assetPath);
                }

                // SubMeshUI component
                if (assetDataFile.Contains("m_Script: {fileID: 11500000, guid: a5378e1f14d974d419f811d6b0861f20, type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 11500000, guid: a5378e1f14d974d419f811d6b0861f20, type: 3}", "m_Script: {fileID: 1453722849, guid: 89f0137620f6af44b9ba852b4190e64e, type: 3}");
                    Debug.Log("Updated references for SubMeshUI component in file: " + assetPath);
                }

                // SubMesh component
                if (assetDataFile.Contains("m_Script: {fileID: 11500000, guid: bd950677b2d06c74494b1c1118584fff, type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 11500000, guid: bd950677b2d06c74494b1c1118584fff, type: 3}", "m_Script: {fileID: 1330537494, guid: 89f0137620f6af44b9ba852b4190e64e, type: 3}");
                    Debug.Log("Updated references for SubMesh component in file: " + assetPath);
                }

                // TMP_InputField component
                if (assetDataFile.Contains("m_Script: {fileID: 11500000, guid: 7b85855a3deaa2e44ac6741a6bbc85f6, type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 11500000, guid: 7b85855a3deaa2e44ac6741a6bbc85f6, type: 3}", "m_Script: {fileID: -1620774994, guid: 89f0137620f6af44b9ba852b4190e64e, type: 3}");
                    Debug.Log("Updated references for SubMesh component in file: " + assetPath);
                }

                // TMP_FontAsset
                if (assetDataFile.Contains("m_Script: {fileID: 11500000, guid: 74dfce233ddb29b4294c3e23c1d3650d, type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 11500000, guid: 74dfce233ddb29b4294c3e23c1d3650d, type: 3}", "m_Script: {fileID: -667331979, guid: 89f0137620f6af44b9ba852b4190e64e, type: 3}");
                    Debug.Log("Updated references for TMP Font Asset: " + assetPath);
                }

                // TMP_SpriteAsset
                if (assetDataFile.Contains("m_Script: {fileID: 11500000, guid: 90940d439ca0ef746af0b48419b92d2e, type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 11500000, guid: 90940d439ca0ef746af0b48419b92d2e, type: 3}", "m_Script: {fileID: 2019389346, guid: 89f0137620f6af44b9ba852b4190e64e, type: 3}");
                    Debug.Log("Updated references for TMP Sprite Asset: " + assetPath);
                }

                // TMP_Settings
                if (assetDataFile.Contains("m_Script: {fileID: 11500000, guid: aafc3c7b9e915d64e8ec3d2c88b3a231, type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 11500000, guid: aafc3c7b9e915d64e8ec3d2c88b3a231, type: 3}", "m_Script: {fileID: -395462249, guid: 89f0137620f6af44b9ba852b4190e64e, type: 3}");
                    Debug.Log("Updated references for TMP Settings: " + assetPath);
                }

                // TMP_Stylesheet
                if (assetDataFile.Contains("m_Script: {fileID: 11500000, guid: 13259b4ce497b194eb52a33d8eda0bdc, type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 11500000, guid: 13259b4ce497b194eb52a33d8eda0bdc, type: 3}", "m_Script: {fileID: -1936749209, guid: 89f0137620f6af44b9ba852b4190e64e, type: 3}");
                    Debug.Log("Updated references for TMP Settings: " + assetPath);
                }

                // TMP_ColorGradient
                if (assetDataFile.Contains("m_Script: {fileID: 11500000, guid: e90e18dd4a044ff4394833216e6bf4d2, type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 11500000, guid: e90e18dd4a044ff4394833216e6bf4d2, type: 3}", "m_Script: {fileID: 2108210716, guid: 89f0137620f6af44b9ba852b4190e64e, type: 3}");
                    Debug.Log("Updated references for TMP Color Gradient: " + assetPath);
                }

                if (hasFileChanged)
                {
                    switch (saveOptions)
                    {
                        case SaveAssetDialogueOptions.Unset:
                            if (EditorUtility.DisplayDialog("Save Modified Asset(s)?", "Are you sure you want to save all modified assets?", "YES", "NO"))
                            {
                                File.WriteAllText(projectPath + "/" + assetPath, assetDataFile);
                                saveOptions = SaveAssetDialogueOptions.SaveAll;
                                AssetDatabase.Refresh();
                            }
                            else
                            {
                                saveOptions = SaveAssetDialogueOptions.DoNotSave;
                            }
                            break;
                        case SaveAssetDialogueOptions.SaveAll:
                            File.WriteAllText(projectPath + "/" + assetPath, assetDataFile);
                            AssetDatabase.Refresh();
                            break;
                        case SaveAssetDialogueOptions.DoNotSave:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GenerateUniqueGUID()
        {
            string monoGuid = System.Guid.NewGuid().ToString();

            char[] charGuid = new char[32];
            int index = 0;
            for (int i = 0; i < monoGuid.Length; i++)
            {
                if (monoGuid[i] != '-')
                    charGuid[index++] = monoGuid[i];
            }

            string guid = new string(charGuid);

            // Make sure new GUID is not already used by some other asset.
            if (AssetDatabase.GUIDToAssetPath(guid) != string.Empty)
                guid = GenerateUniqueGUID();

            return guid;
        }


        private static void ConvertGUIDFromDLLToSource()
        {
            // Check if Asset Serialization Mode is ForceText
            if (UnityEditor.EditorSettings.serializationMode != SerializationMode.ForceText)
                UnityEditor.EditorSettings.serializationMode = SerializationMode.ForceText;

            if (UnityEditor.EditorSettings.externalVersionControl != "Visible Meta Files")
                UnityEditor.EditorSettings.externalVersionControl = "Visible Meta Files";

            string projectPath = Path.GetFullPath("Assets/..");


            string[] allProjectObjects = AssetDatabase.FindAssets("t:Prefab t:Scene t:ScriptableObject"); // new string[] { "Assets/TextMesh Pro/Examples/Textures" });
            SaveAssetDialogueOptions saveOptions = SaveAssetDialogueOptions.Unset;

            for (int i = 0; i < allProjectObjects.Length; i++)
            {
                string guid = allProjectObjects[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                //Debug.Log("Searching asset: " + assetPath);

                string assetDataFile = File.ReadAllText(projectPath + "/" + assetPath);
                //StringBuilder assetDataFile = new StringBuilder(File.ReadAllText(projectPath + "/" + assetPath));

                bool hasFileChanged = false;

                string assemblyGUID = string.Empty;
                if (assetDataFile.Contains("guid: b5bd0d848a86e48409fe56688d66ecb5"))
                    assemblyGUID = "b5bd0d848a86e48409fe56688d66ecb5";
                else if (assetDataFile.Contains("guid: 89f0137620f6af44b9ba852b4190e64e"))
                    assemblyGUID = "89f0137620f6af44b9ba852b4190e64e";

                // TMP_Text script
                if (assetDataFile.Contains("m_Script: {fileID: -1385168320, guid: " + assemblyGUID + ", type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: -1385168320, guid: " + assemblyGUID + ", type: 3}", "m_Script: {fileID: 11500000, guid: 9ec8dc9c3fa2e5d41b939b5888d2f1e8, type: 3}");
                    Debug.Log("Updated references for TMP_Text script in file: " + assetPath);
                }

                // TextMeshPro component
                if (assetDataFile.Contains("m_Script: {fileID: -806885394, guid: " + assemblyGUID + ", type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: -806885394, guid: " + assemblyGUID + ", type: 3}", "m_Script: {fileID: 11500000, guid: 1a1578b9753d2604f98d608cb4239e2f, type: 3}");
                    Debug.Log("Updated references for TextMeshPro component in file: " + assetPath);
                }

                // TextMeshProUGUI component
                if (assetDataFile.Contains("m_Script: {fileID: 1453722849, guid: " + assemblyGUID + ", type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 1453722849, guid: " + assemblyGUID + ", type: 3}", "m_Script: {fileID: 11500000, guid: 496f2e385b0c62542b5c739ccfafd8da, type: 3}");
                    Debug.Log("Updated references for TextMeshProUGUI component in file: " + assetPath);
                }

                // SubMeshUI component
                if (assetDataFile.Contains("m_Script: {fileID: 1453722849, guid: " + assemblyGUID + ", type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 1453722849, guid: " + assemblyGUID + ", type: 3}", "m_Script: {fileID: 11500000, guid: a5378e1f14d974d419f811d6b0861f20, type: 3}");
                    Debug.Log("Updated references for SubMeshUI component in file: " + assetPath);
                }

                // SubMesh component
                if (assetDataFile.Contains("m_Script: {fileID: 1330537494, guid: " + assemblyGUID + ", type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 1330537494, guid: " + assemblyGUID + ", type: 3}", "m_Script: {fileID: 11500000, guid: bd950677b2d06c74494b1c1118584fff, type: 3}");
                    Debug.Log("Updated references for SubMesh component in file: " + assetPath);
                }

                // TMP_InputField component
                if (assetDataFile.Contains("m_Script: {fileID: -1620774994, guid: " + assemblyGUID + ", type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: -1620774994, guid: " + assemblyGUID + ", type: 3}", "m_Script: {fileID: 11500000, guid: 7b85855a3deaa2e44ac6741a6bbc85f6, type: 3}");
                    Debug.Log("Updated references for SubMesh component in file: " + assetPath);
                }

                // TMP_FontAsset
                if (assetDataFile.Contains("m_Script: {fileID: -667331979, guid: " + assemblyGUID + ", type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: -667331979, guid: " + assemblyGUID + ", type: 3}", "m_Script: {fileID: 11500000, guid: 74dfce233ddb29b4294c3e23c1d3650d, type: 3}");
                    Debug.Log("Updated references for TMP Font Asset: " + assetPath);
                }

                // TMP_SpriteAsset
                if (assetDataFile.Contains("m_Script: {fileID: 2019389346, guid: " + assemblyGUID + ", type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 2019389346, guid: " + assemblyGUID + ", type: 3}", "m_Script: {fileID: 11500000, guid: 90940d439ca0ef746af0b48419b92d2e, type: 3}");
                    Debug.Log("Updated references for TMP Sprite Asset: " + assetPath);
                }

                // TMP_Settings
                if (assetDataFile.Contains("m_Script: {fileID: -395462249, guid: " + assemblyGUID + ", type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: -395462249, guid: " + assemblyGUID + ", type: 3}", "m_Script: {fileID: 11500000, guid: aafc3c7b9e915d64e8ec3d2c88b3a231, type: 3}");
                    Debug.Log("Updated references for TMP Settings: " + assetPath);
                }

                // TMP_Stylesheet
                if (assetDataFile.Contains("m_Script: {fileID: -1936749209, guid: " + assemblyGUID + ", type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: -1936749209, guid: " + assemblyGUID + ", type: 3}", "m_Script: {fileID: 11500000, guid: 13259b4ce497b194eb52a33d8eda0bdc, type: 3}");
                    Debug.Log("Updated references for TMP Settings: " + assetPath);
                }

                // TMP_ColorGradient
                if (assetDataFile.Contains("m_Script: {fileID: 2108210716, guid: " + assemblyGUID + ", type: 3}"))
                {
                    hasFileChanged = true;
                    assetDataFile = assetDataFile.Replace("m_Script: {fileID: 2108210716, guid: " + assemblyGUID + ", type: 3}", "m_Script: {fileID: 11500000, guid: e90e18dd4a044ff4394833216e6bf4d2, type: 3}");
                    Debug.Log("Updated references for TMP Color Gradient: " + assetPath);
                }

                if (hasFileChanged)
                {
                    switch (saveOptions)
                    {
                        case SaveAssetDialogueOptions.Unset:
                            if (EditorUtility.DisplayDialog("Save Modified Asset(s)?", "Are you sure you want to save all modified assets?", "YES", "NO"))
                            {
                                File.WriteAllText(projectPath + "/" + assetPath, assetDataFile);
                                saveOptions = SaveAssetDialogueOptions.SaveAll;
                                AssetDatabase.Refresh();
                            }
                            else
                            {
                                saveOptions = SaveAssetDialogueOptions.DoNotSave;
                            }
                            break;
                        case SaveAssetDialogueOptions.SaveAll:
                            File.WriteAllText(projectPath + "/" + assetPath, assetDataFile);
                            AssetDatabase.Refresh();
                            break;
                        case SaveAssetDialogueOptions.DoNotSave:
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private static void SetProjectSerializationAndSourceControlModes()
        {
            // Make sure Asset Serialization mode is set to ForceText with Visible Meta Files.
            m_ProjectAssetSerializationMode = EditorSettings.serializationMode;
            if (m_ProjectAssetSerializationMode != SerializationMode.ForceText)
                UnityEditor.EditorSettings.serializationMode = SerializationMode.ForceText;

            m_ProjectExternalVersionControl = EditorSettings.externalVersionControl;
            if (m_ProjectExternalVersionControl != "Visible Meta Files")
                UnityEditor.EditorSettings.externalVersionControl = "Visible Meta Files";
        }


        /// <summary>
        /// 
        /// </summary>
        private static void RestoreProjectSerializationAndSourceControlModes()
        {
            // Make sure Asset Serialization mode is set to ForceText with Visible Meta Files.
            if (m_ProjectAssetSerializationMode != EditorSettings.serializationMode)
                EditorSettings.serializationMode = m_ProjectAssetSerializationMode;

            if (m_ProjectExternalVersionControl != EditorSettings.externalVersionControl)
                EditorSettings.externalVersionControl = m_ProjectExternalVersionControl;
        }

        [System.Serializable]
        private struct AssetConversionRecord
        {
            public string referencedResource;
            public string target;
            public string replacement;
        }


        /// <summary>
        /// 
        /// </summary>
        [System.Serializable]
        private class AssetConversionData
        {
            public List<AssetConversionRecord> assetRecords;
        }
    }
}
