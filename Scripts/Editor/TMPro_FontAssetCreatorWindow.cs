using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.IO;


namespace TMPro.EditorUtilities
{
    public class TMPro_FontAssetCreatorWindow : EditorWindow
    {
        [MenuItem("Window/TextMeshPro/Font Asset Creator", false, 2025)]
        public static void ShowFontAtlasCreatorWindow()
        {
            var window = GetWindow<TMPro_FontAssetCreatorWindow>();
            window.titleContent = new GUIContent("Font Creator");
            window.Focus();

            // Make sure TMP Essential Resources have been imported.
            window.CheckEssentialResources();
        }


        public static void ShowFontAtlasCreatorWindow(Font sourceFontFile)
        {
            var window = GetWindow<TMPro_FontAssetCreatorWindow>();

            window.titleContent = new GUIContent("Font Creator");
            window.Focus();

            window.ClearGeneratedData();
            window.m_LegacyFontAsset = null;
            window.m_SelectedFontAsset = null;

            // Override selected font asset
            window.m_SourceFontFile = sourceFontFile;

            // Make sure TMP Essential Resources have been imported.
            window.CheckEssentialResources();
        }


        public static void ShowFontAtlasCreatorWindow(TMP_FontAsset fontAsset)
        {
            var window = GetWindow<TMPro_FontAssetCreatorWindow>();

            window.titleContent = new GUIContent("Asset Creator");
            window.Focus();

            // Clear any previously generated data
            window.ClearGeneratedData();
            window.m_LegacyFontAsset = null;

            // Load font asset creation settings if we have valid settings
            if (string.IsNullOrEmpty(fontAsset.creationSettings.sourceFontFileGUID) == false)
            {
                window.LoadFontCreationSettings(fontAsset.creationSettings);
                window.m_ReferencedFontAsset = fontAsset;
                window.m_SavedFontAtlas = fontAsset.atlas;
            }
            else
            {
                window.m_WarningMessage = "Font Asset [" + fontAsset.name + "] does not contain any previous \"Font Asset Creation Settings\". This usually means [" + fontAsset.name + "] was created before this new functionality was added.";
                window.m_SourceFontFile = null;
                window.m_LegacyFontAsset = fontAsset;
            }

            // Even if we don't have any saved generation settings, we still want to pre-select the source font file.
            window.m_SelectedFontAsset = fontAsset;

            // Make sure TMP Essential Resources have been imported.
            window.CheckEssentialResources();
        }

        private string[] FontSizingOptions = { "Auto Sizing", "Custom Size" };
        private int m_PointSizeSamplingMode = 0;
        private string[] FontResolutionLabels = { "8", "16","32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" };
        private int[] FontAtlasResolutions = { 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };
        private string[] FontCharacterSets = { "ASCII", "Extended ASCII", "ASCII Lowercase", "ASCII Uppercase", "Numbers + Symbols", "Custom Range", "Unicode Range (Hex)", "Custom Characters", "Characters from File" };
        private enum FontPackingModes { Fast = 0, Optimum = 4 };
        private FontPackingModes m_PackingMode = 0;

        private int m_CharacterSetSelectionMode = 0;
        private enum PreviewSelectionTypes { PreviewFont, PreviewTexture, PreviewDistanceField };
        private PreviewSelectionTypes previewSelection;

        private string m_CharacterSequence = "";
        private string m_Output_feedback = "";
        private string output_name_label = "Font: ";
        private string output_size_label = "Pt. Size: ";
        private string output_count_label = "Characters packed: ";
        private string m_WarningMessage;
        private int m_character_Count;
        private Vector2 output_ScrollPosition;

        [System.Serializable]
        class FontAssetCreationSettingsContainer
        {
            public List<FontAssetCreationSettings> fontAssetCreationSettings;
        }

        private FontAssetCreationSettingsContainer m_FontAssetCreationSettingsContainer;
        //private string[] m_FontCreationPresets = new string[] { "Recent 1", "Recent 2", "Recent 3", "Recent 4" };
        private int m_FontAssetCreationSettingsCurrentIndex = 0;
        private const string k_FontAssetCreationSettingsContainerKey = "TextMeshPro.FontAssetCreator.RecentFontAssetCreationSettings.Container";
        private const string k_FontAssetCreationSettingsCurrentIndexKey = "TextMeshPro.FontAssetCreator.RecentFontAssetCreationSettings.CurrentIndex";

        //private Thread MainThread;
        private Color[] Output = null;
        private bool isDistanceMapReady = false;
        private bool isRepaintNeeded = false;

        private Rect progressRect;
        public static float ProgressPercentage;
        private float m_renderingProgress;
        private bool isRenderingDone = false;
        private bool m_IsProcessing = false;
        private bool m_IsGenerationDisabled = false;
        private bool isGenerationCancelled = false;
        private bool m_IsFontAtlasInvalid;

        private Font m_SourceFontFile;
        private TMP_FontAsset m_SelectedFontAsset;
        private TMP_FontAsset m_ReferencedFontAsset;
        private TMP_FontAsset m_LegacyFontAsset;
        private TextAsset m_CharactersFromFile;
        private int m_PointSize;

        private int m_Padding = 5;
        private FaceStyles m_FontStyle = FaceStyles.Normal;
        private float m_FontStyleValue = 2;
        private RenderModes m_RenderMode = RenderModes.DistanceField16;
        private int m_AtlasWidth = 512;
        private int m_AtlasHeight = 512;

        private int font_scaledownFactor = 1;


        private FT_FaceInfo m_font_faceInfo;
        private FT_GlyphInfo[] m_font_glyphInfo;
        private byte[] m_texture_buffer;
        private Texture2D m_Font_Atlas;
        private Texture2D m_SavedFontAtlas;
        //private Texture2D m_texture_Atlas;
        //private int m_packingMethod = 0;

        private Texture2D m_destination_Atlas;
        private bool includeKerningPairs = false;
        private int[] m_kerningSet;

        // Image Down Sampling Fields
        //private Texture2D sdf_Atlas;
        //private int downscale;

        // Diagnostics
        private System.Diagnostics.Stopwatch m_stopWatch;

        private EditorWindow m_editorWindow;
        private Vector2 m_previewWindow_Size = new Vector2(768, 768);
        private Rect m_UI_Panel_Size;


        public void OnEnable()
        {
            // Used for Diagnostics
            m_stopWatch = new System.Diagnostics.Stopwatch();

            m_editorWindow = this;
            UpdateEditorWindowSize(768, 768);

            // Get the UI Skin and Styles for the various Editors
            TMP_UIStyleManager.GetUIStyles();

            // Initialize & Get shader property IDs.
            ShaderUtilities.GetShaderPropertyIDs();

            // Load last selected preset if we are not already in the process of regenerating an existing font asset (via the Context menu)
            if (EditorPrefs.HasKey(k_FontAssetCreationSettingsContainerKey))
            {
                if (m_FontAssetCreationSettingsContainer == null)
                    m_FontAssetCreationSettingsContainer = JsonUtility.FromJson<FontAssetCreationSettingsContainer>(EditorPrefs.GetString(k_FontAssetCreationSettingsContainerKey));

                if (m_FontAssetCreationSettingsContainer.fontAssetCreationSettings != null && m_FontAssetCreationSettingsContainer.fontAssetCreationSettings.Count > 0)
                {
                    // Load Font Asset Creation Settings preset.
                    if (EditorPrefs.HasKey(k_FontAssetCreationSettingsCurrentIndexKey))
                        m_FontAssetCreationSettingsCurrentIndex = EditorPrefs.GetInt(k_FontAssetCreationSettingsCurrentIndexKey);

                    LoadFontCreationSettings(m_FontAssetCreationSettingsContainer.fontAssetCreationSettings[m_FontAssetCreationSettingsCurrentIndex]);
                }
            }

            // Debug Link to received message from Native Code
            //TMPro_FontPlugin.LinkDebugLog(); // Link with C++ Plugin to get Debug output
        }


        public void OnDisable()
        {
            //Debug.Log("TextMeshPro Editor Window has been disabled.");

            // Cancel font asset generation just in case one is in progress.
            TMPro_FontPlugin.SendCancellationRequest(CancellationRequestType.WindowClosed);

            // Destroy Engine only if it has been initialized already
            TMPro_FontPlugin.Destroy_FontEngine();

            // Cleaning up allocated Texture2D
            if (m_destination_Atlas != null && EditorUtility.IsPersistent(m_destination_Atlas) == false)
            {
                //Debug.Log("Destroying destination_Atlas!");
                DestroyImmediate(m_destination_Atlas);
            }

            if (m_Font_Atlas != null && EditorUtility.IsPersistent(m_Font_Atlas) == false)
            {
                //Debug.Log("Destroying font_Atlas!");
                DestroyImmediate(m_Font_Atlas);
            }

            // Remove Glyph Report if one was created.
            if (File.Exists("Assets/TextMesh Pro/Glyph Report.txt"))
            {
                File.Delete("Assets/TextMesh Pro/Glyph Report.txt");
                File.Delete("Assets/TextMesh Pro/Glyph Report.txt.meta");

                AssetDatabase.Refresh();
            }

            // Save Font Asset Creation Settings Index
            SaveCreationSettingsToEditorPrefs(SaveFontCreationSettings());
            EditorPrefs.SetInt(k_FontAssetCreationSettingsCurrentIndexKey, m_FontAssetCreationSettingsCurrentIndex);

            // Unregister to event
            TMPro_EventManager.RESOURCE_LOAD_EVENT.Remove(ON_RESOURCES_LOADED);

            Resources.UnloadUnusedAssets();
        }


        // Event received when TMP resources have been loaded.
        void ON_RESOURCES_LOADED()
        {
            TMPro_EventManager.RESOURCE_LOAD_EVENT.Remove(ON_RESOURCES_LOADED);

            m_IsGenerationDisabled = false;
        }

        // Make sure TMP Essential Resources have been imported.
        void CheckEssentialResources()
        {
            if (TMP_Settings.instance == null)
            {
                if (m_IsGenerationDisabled == false)
                    TMPro_EventManager.RESOURCE_LOAD_EVENT.Add(ON_RESOURCES_LOADED);

                m_IsGenerationDisabled = true;
            }
        }


        public void OnGUI()
        {
            GUILayout.BeginHorizontal(GUILayout.Width(310));
            DrawControls();

            DrawPreview();
            GUILayout.EndHorizontal();
        }


        public void Update()
        {
            if (isDistanceMapReady)
            {
                if (m_Font_Atlas != null)
                {
                    m_destination_Atlas = new Texture2D(m_Font_Atlas.width / font_scaledownFactor, m_Font_Atlas.height / font_scaledownFactor, TextureFormat.Alpha8, false, true);
                    m_destination_Atlas.SetPixels(Output);
                    m_destination_Atlas.Apply(false, true);
                }
                //else if (m_texture_Atlas != null)
                //{
                //    m_destination_Atlas = new Texture2D(m_texture_Atlas.width / font_scaledownFactor, m_texture_Atlas.height / font_scaledownFactor, TextureFormat.Alpha8, false, true);
                //    m_destination_Atlas.SetPixels(Output);
                //    m_destination_Atlas.Apply(false, true);
                //}

                isDistanceMapReady = false;
                Repaint();

                // Saving File for Debug
                //var pngData = destination_Atlas.EncodeToPNG();
                //File.WriteAllBytes("Assets/Textures/Debug SDF.png", pngData);
            }

            if (isRepaintNeeded)
            {
                //Debug.Log("Repainting...");
                isRepaintNeeded = false;
                Repaint();
            }

            // Update Progress bar is we are Rendering a Font.
            if (m_IsProcessing)
            {
                m_renderingProgress = TMPro_FontPlugin.Check_RenderProgress();

                isRepaintNeeded = true;
            }

            // Update Feedback Window & Create Font Texture once Rendering is done.
            if (isRenderingDone)
            {
                // Stop StopWatch
                m_stopWatch.Stop();
                Debug.Log("Font Atlas generation completed in: " + m_stopWatch.Elapsed.TotalMilliseconds.ToString("0.000 ms."));
                m_stopWatch.Reset();

                m_IsProcessing = false;
                isRenderingDone = false;

                if (isGenerationCancelled == false)
                {
                    UpdateRenderFeedbackWindow();
                    CreateFontTexture();
                }
            }
        }


        /// <summary>
        /// Method which returns the character corresponding to a decimal value.
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        int[] ParseNumberSequence(string sequence)
        {
            List<int> unicode_list = new List<int>();
            string[] sequences = sequence.Split(',');

            foreach (string seq in sequences)
            {
                string[] s1 = seq.Split('-');

                if (s1.Length == 1)
                    try
                    {
                        unicode_list.Add(int.Parse(s1[0]));
                    }
                    catch
                    {
                        Debug.Log("No characters selected or invalid format.");
                    }
                else
                {
                    for (int j = int.Parse(s1[0]); j < int.Parse(s1[1]) + 1; j++)
                    {
                        unicode_list.Add(j);
                    }
                }
            }

            return unicode_list.ToArray();
        }


        /// <summary>
        /// Method which returns the character (decimal value) from a hex sequence.
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        int[] ParseHexNumberSequence(string sequence)
        {
            List<int> unicode_list = new List<int>();
            string[] sequences = sequence.Split(',');

            foreach (string seq in sequences)
            {
                string[] s1 = seq.Split('-');

                if (s1.Length == 1)
                    try
                    {
                        unicode_list.Add(int.Parse(s1[0], NumberStyles.AllowHexSpecifier));
                    }
                    catch
                    {
                        Debug.Log("No characters selected or invalid format.");
                    }
                else
                {
                    for (int j = int.Parse(s1[0], NumberStyles.AllowHexSpecifier); j < int.Parse(s1[1], NumberStyles.AllowHexSpecifier) + 1; j++)
                    {
                        unicode_list.Add(j);
                    }
                }
            }

            return unicode_list.ToArray();
        }



        void DrawControls()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("<b>TextMeshPro - Font Asset Creator</b>", TMP_UIStyleManager.Section_Label, GUILayout.Width(300));

            Rect rect = EditorGUILayout.GetControlRect(false, 25);
            GUI.Label(rect, m_SelectedFontAsset != null ? string.Format("Creation Settings ({0})", m_SelectedFontAsset.name) : "Creation Settings", TMP_UIStyleManager.Section_Label);

            // Display Recent Font Asset Creation Settings
            //EditorGUI.BeginChangeCheck();
            //rect.x += 170; rect.y += 4; rect.width = rect.width - 175;
            //m_FontAssetCreationSettingsCurrentIndex = EditorGUI.Popup(rect, m_FontAssetCreationSettingsCurrentIndex, m_FontCreationPresets);
            //if (EditorGUI.EndChangeCheck())
            //{
            //    // Load creation settings
            //    LoadFontCreationSettings(m_FontAssetCreationSettingsContainer.fontAssetCreationSettings[m_FontAssetCreationSettingsCurrentIndex]);
            //    m_IsFontAtlasInvalid = true;
            //    m_SelectedFontAsset = null;
            //}

            GUILayout.BeginVertical(TMP_UIStyleManager.TextureAreaBox, GUILayout.Width(300));

            EditorGUIUtility.labelWidth = 120f;
            EditorGUIUtility.fieldWidth = 160f;

            // Disable Options if already generating a font atlas texture.
            EditorGUI.BeginDisabledGroup(m_IsProcessing);
            {
                // FONT TTF SELECTION
                EditorGUI.BeginChangeCheck();
                m_SourceFontFile = EditorGUILayout.ObjectField("Font Source", m_SourceFontFile, typeof(Font), false) as Font;
                if (EditorGUI.EndChangeCheck())
                {
                    m_SelectedFontAsset = null;
                    m_IsFontAtlasInvalid = true;
                }

                // FONT SIZING
                EditorGUI.BeginChangeCheck();
                if (m_PointSizeSamplingMode == 0)
                {
                    m_PointSizeSamplingMode = EditorGUILayout.Popup("Font Size", m_PointSizeSamplingMode, FontSizingOptions);
                }
                else
                {
                    EditorGUIUtility.labelWidth = 120f;
                    EditorGUIUtility.fieldWidth = 80f;

                    GUILayout.BeginHorizontal();
                    m_PointSizeSamplingMode = EditorGUILayout.Popup("Font Size", m_PointSizeSamplingMode, FontSizingOptions);
                    m_PointSize = EditorGUILayout.IntField(m_PointSize);
                    GUILayout.EndHorizontal();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    m_IsFontAtlasInvalid = true;
                }

                EditorGUIUtility.labelWidth = 120f;
                EditorGUIUtility.fieldWidth = 160f;

                // FONT PADDING
                EditorGUI.BeginChangeCheck();
                m_Padding = EditorGUILayout.IntField("Font Padding", m_Padding);
                m_Padding = (int)Mathf.Clamp(m_Padding, 0f, 64f);
                if (EditorGUI.EndChangeCheck())
                {
                    m_IsFontAtlasInvalid = true;
                }

                // FONT PACKING METHOD SELECTION
                EditorGUI.BeginChangeCheck();
                m_PackingMode = (FontPackingModes)EditorGUILayout.EnumPopup("Packing Method", m_PackingMode);
                if (EditorGUI.EndChangeCheck())
                {
                    m_IsFontAtlasInvalid = true;
                }

                // FONT ATLAS RESOLUTION SELECTION
                GUILayout.BeginHorizontal();
                GUI.changed = false;

                EditorGUIUtility.labelWidth = 120f;
                EditorGUIUtility.fieldWidth = 40f;

                EditorGUI.BeginChangeCheck();
                GUILayout.Label("Atlas Resolution:", GUILayout.Width(116));
                m_AtlasWidth = EditorGUILayout.IntPopup(m_AtlasWidth, FontResolutionLabels, FontAtlasResolutions);
                m_AtlasHeight = EditorGUILayout.IntPopup(m_AtlasHeight, FontResolutionLabels, FontAtlasResolutions);
                if (EditorGUI.EndChangeCheck())
                {
                    m_IsFontAtlasInvalid = true;
                }

                GUILayout.EndHorizontal();

                // FONT CHARACTER SET SELECTION
                EditorGUI.BeginChangeCheck();
                bool hasSelectionChanged = false;
                m_CharacterSetSelectionMode = EditorGUILayout.Popup("Character Set", m_CharacterSetSelectionMode, FontCharacterSets);
                if (EditorGUI.EndChangeCheck())
                {
                    m_CharacterSequence = "";
                    hasSelectionChanged = true;
                    m_IsFontAtlasInvalid = true;
                }

                switch (m_CharacterSetSelectionMode)
                {
                    case 0: // ASCII
                            //characterSequence = "32 - 126, 130, 132 - 135, 139, 145 - 151, 153, 155, 161, 166 - 167, 169 - 174, 176, 181 - 183, 186 - 187, 191, 8210 - 8226, 8230, 8240, 8242 - 8244, 8249 - 8250, 8252 - 8254, 8260, 8286";
                        m_CharacterSequence = "32 - 126, 160, 8203, 8230, 9633";
                        break;

                    case 1: // EXTENDED ASCII
                        m_CharacterSequence = "32 - 126, 160 - 255, 8192 - 8303, 8364, 8482, 9633";
                        // Could add 9632 for missing glyph
                        break;

                    case 2: // Lowercase
                        m_CharacterSequence = "32 - 64, 91 - 126, 160";
                        break;

                    case 3: // Uppercase
                        m_CharacterSequence = "32 - 96, 123 - 126, 160";
                        break;

                    case 4: // Numbers & Symbols
                        m_CharacterSequence = "32 - 64, 91 - 96, 123 - 126, 160";
                        break;

                    case 5: // Custom Range
                        EditorGUILayout.BeginVertical(TMP_UIStyleManager.TextureAreaBox);
                        GUILayout.Label("Enter a sequence of decimal values to define the characters to be included in the font asset or retrieve one from another font asset.", TMP_UIStyleManager.Label);
                        GUILayout.Space(10f);

                        EditorGUI.BeginChangeCheck();
                        m_ReferencedFontAsset = EditorGUILayout.ObjectField("Select Font Asset", m_ReferencedFontAsset, typeof(TMP_FontAsset), false) as TMP_FontAsset;
                        if (EditorGUI.EndChangeCheck() || hasSelectionChanged)
                        {
                            if (m_ReferencedFontAsset != null)
                                m_CharacterSequence = TMP_EditorUtility.GetDecimalCharacterSequence(TMP_FontAsset.GetCharactersArray(m_ReferencedFontAsset));
                            
                            m_IsFontAtlasInvalid = true;
                        }

                        EditorGUIUtility.labelWidth = 120;

                        // Filter out unwanted characters.
                        char chr = Event.current.character;
                        if ((chr < '0' || chr > '9') && (chr < ',' || chr > '-'))
                        {
                            Event.current.character = '\0';
                        }

                        GUILayout.Label("Character Sequence (Decimal)", TMP_UIStyleManager.Section_Label);
                        EditorGUI.BeginChangeCheck();
                        m_CharacterSequence = EditorGUILayout.TextArea(m_CharacterSequence, TMP_UIStyleManager.TextAreaBoxWindow, GUILayout.Height(120), GUILayout.MaxWidth(285));
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_IsFontAtlasInvalid = true;
                        }
                        
                        EditorGUILayout.EndVertical();
                        break;

                    case 6: // Unicode HEX Range
                        EditorGUILayout.BeginVertical(TMP_UIStyleManager.TextureAreaBox);
                        GUILayout.Label("Enter a sequence of Unicode (hex) values to define the characters to be included in the font asset or retrieve one from another font asset.", TMP_UIStyleManager.Label);
                        GUILayout.Space(10f);

                        EditorGUI.BeginChangeCheck();
                        m_ReferencedFontAsset = EditorGUILayout.ObjectField("Select Font Asset", m_ReferencedFontAsset, typeof(TMP_FontAsset), false) as TMP_FontAsset;
                        if (EditorGUI.EndChangeCheck() || hasSelectionChanged)
                        {
                            if (m_ReferencedFontAsset != null)
                                m_CharacterSequence = TMP_EditorUtility.GetUnicodeCharacterSequence(TMP_FontAsset.GetCharactersArray(m_ReferencedFontAsset));

                            m_IsFontAtlasInvalid = true;
                        }

                        EditorGUIUtility.labelWidth = 120;


                        // Filter out unwanted characters.
                        chr = Event.current.character;
                        if ((chr < '0' || chr > '9') && (chr < 'a' || chr > 'f') && (chr < 'A' || chr > 'F') && (chr < ',' || chr > '-'))
                        {
                            Event.current.character = '\0';
                        }
                        GUILayout.Label("Character Sequence (Hex)", TMP_UIStyleManager.Section_Label);
                        EditorGUI.BeginChangeCheck();
                        m_CharacterSequence = EditorGUILayout.TextArea(m_CharacterSequence, TMP_UIStyleManager.TextAreaBoxWindow, GUILayout.Height(120), GUILayout.MaxWidth(285));
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_IsFontAtlasInvalid = true;
                        }

                        EditorGUILayout.EndVertical();
                        break;

                    case 7: // Characters from Font Asset
                        EditorGUILayout.BeginVertical(TMP_UIStyleManager.TextureAreaBox);
                        GUILayout.Label("Type the characters to be included in the font asset or retrieve them from another font asset.", TMP_UIStyleManager.Label);
                        GUILayout.Space(10f);

                        EditorGUI.BeginChangeCheck();
                        m_ReferencedFontAsset = EditorGUILayout.ObjectField("Select Font Asset", m_ReferencedFontAsset, typeof(TMP_FontAsset), false) as TMP_FontAsset;
                        if (EditorGUI.EndChangeCheck() || hasSelectionChanged)
                        {
                            if (m_ReferencedFontAsset != null)
                                m_CharacterSequence = TMP_FontAsset.GetCharacters(m_ReferencedFontAsset);

                            m_IsFontAtlasInvalid = true;
                        }

                        EditorGUIUtility.labelWidth = 120;

                        EditorGUI.indentLevel = 0;

                        GUILayout.Label("Custom Character List", TMP_UIStyleManager.Section_Label);
                        EditorGUI.BeginChangeCheck();
                        m_CharacterSequence = EditorGUILayout.TextArea(m_CharacterSequence, TMP_UIStyleManager.TextAreaBoxWindow, GUILayout.Height(120), GUILayout.MaxWidth(285));
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_IsFontAtlasInvalid = true;
                        }

                        EditorGUILayout.EndVertical();
                        break;

                    case 8: // Character List from File
                        EditorGUI.BeginChangeCheck();
                        m_CharactersFromFile = EditorGUILayout.ObjectField("Character File", m_CharactersFromFile, typeof(TextAsset), false) as TextAsset;
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_IsFontAtlasInvalid = true;
                        }

                        if (m_CharactersFromFile != null)
                        {
                            m_CharacterSequence = m_CharactersFromFile.text;
                        }
                        break;
                }

                EditorGUIUtility.labelWidth = 120f;
                EditorGUIUtility.fieldWidth = 80f;

                // FONT STYLE SELECTION
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();
                m_FontStyle = (FaceStyles)EditorGUILayout.EnumPopup("Font Style:", m_FontStyle);
                m_FontStyleValue = EditorGUILayout.IntField((int)m_FontStyleValue);
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    m_IsFontAtlasInvalid = true;
                }

                // Render Mode Selection
                EditorGUI.BeginChangeCheck();
                m_RenderMode = (RenderModes)EditorGUILayout.EnumPopup("Font Render Mode:", m_RenderMode);
                if (EditorGUI.EndChangeCheck())
                {
                    //m_availableShaderNames = UpdateShaderList(font_renderMode, out m_availableShaders);
                    m_IsFontAtlasInvalid = true;
                }

                includeKerningPairs = EditorGUILayout.Toggle("Get Kerning Pairs?", includeKerningPairs);
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(5);

            if (!string.IsNullOrEmpty(m_WarningMessage))
            {
                EditorGUILayout.HelpBox(m_WarningMessage, MessageType.Warning);
            }

            EditorGUIUtility.labelWidth = 120f;
            EditorGUIUtility.fieldWidth = 160f;

            GUILayout.Space(15);

            // GENERATE FONT ASSET
            // Generation options are disabled if we do not have a source font file selected or are already generating.
            bool isEnabled = GUI.enabled = m_SourceFontFile == null || m_IsProcessing || m_IsGenerationDisabled ? false : true;
            if (GUILayout.Button("Generate Font Atlas") && m_CharacterSequence.Length != 0 && GUI.enabled)
            {
                if (m_IsProcessing == false && m_SourceFontFile != null)
                {
                    int error_Code;

                    DestroyImmediate(m_Font_Atlas);
                    m_Font_Atlas = null;
                    m_SavedFontAtlas = null;

                    error_Code = TMPro_FontPlugin.Initialize_FontEngine(); // Initialize Font Engine
                    if (error_Code != 0)
                    {
                        if (error_Code == 0xF0)
                        {
                            //Debug.Log("Font Library was already initialized!");
                            error_Code = 0;
                        }
                        else
                            Debug.Log("Error Code: " + error_Code + "  occurred while Initializing the FreeType Library.");
                    }

                    string fontPath = AssetDatabase.GetAssetPath(m_SourceFontFile); // Get file path of TTF Font.

                    if (error_Code == 0)
                    {
                        error_Code = TMPro_FontPlugin.Load_TrueType_Font(fontPath); // Load the selected font.

                        if (error_Code != 0)
                        {
                            if (error_Code == 0xF1)
                            {
                                //Debug.Log("Font was already loaded!");
                                error_Code = 0;
                            }
                            else
                                Debug.Log("Error Code: " + error_Code + "  occurred while Loading the [" + m_SourceFontFile.name + "] font file. This typically results from the use of an incompatible or corrupted font file.");
                        }
                    }

                    if (error_Code == 0)
                    {
                        if (m_PointSizeSamplingMode == 0) m_PointSize = 72; // If Auto set size to 72 pts.

                        error_Code = TMPro_FontPlugin.FT_Size_Font(m_PointSize); // Load the selected font and size it accordingly.
                        if (error_Code != 0)
                            Debug.Log("Error Code: " + error_Code + "  occurred while Sizing the font.");
                    }

                    // Define an array containing the characters we will render.
                    if (error_Code == 0)
                    {
                        int[] character_Set = null;
                        if (m_CharacterSetSelectionMode == 7 || m_CharacterSetSelectionMode == 8)
                        {
                            List<int> char_List = new List<int>();
                            
                            for (int i = 0; i < m_CharacterSequence.Length; i++)
                            {
                                // Check to make sure we don't include duplicates
                                if (char_List.FindIndex(item => item == m_CharacterSequence[i]) == -1)
                                    char_List.Add(m_CharacterSequence[i]);
                                else
                                {
                                    //Debug.Log("Character [" + characterSequence[i] + "] is a duplicate.");
                                }
                            }

                            character_Set = char_List.ToArray();
                        }
                        else if (m_CharacterSetSelectionMode == 6)
                        {
                            character_Set = ParseHexNumberSequence(m_CharacterSequence);
                        }
                        else
                        {
                            character_Set = ParseNumberSequence(m_CharacterSequence);
                        }

                        m_character_Count = character_Set.Length;

                        m_texture_buffer = new byte[m_AtlasWidth * m_AtlasHeight];

                        m_font_faceInfo = new FT_FaceInfo();

                        m_font_glyphInfo = new FT_GlyphInfo[m_character_Count];

                        int padding = m_Padding;

                        bool autoSizing = m_PointSizeSamplingMode == 0 ? true : false;

                        float strokeSize = m_FontStyleValue;
                        if (m_RenderMode == RenderModes.DistanceField16) strokeSize = m_FontStyleValue * 16;
                        if (m_RenderMode == RenderModes.DistanceField32) strokeSize = m_FontStyleValue * 32;
                        
                        m_IsProcessing = true;
                        isGenerationCancelled = false;

                        // Start Stop Watch
                        m_stopWatch = System.Diagnostics.Stopwatch.StartNew();

                        ThreadPool.QueueUserWorkItem(SomeTask =>
                        {
                            isRenderingDone = false;

                            error_Code = TMPro_FontPlugin.Render_Characters(m_texture_buffer, m_AtlasWidth, m_AtlasHeight, padding, character_Set, m_character_Count, m_FontStyle, strokeSize, autoSizing, m_RenderMode,(int)m_PackingMode, ref m_font_faceInfo, m_font_glyphInfo);
                            isRenderingDone = true;
                        });
                        
                        previewSelection = PreviewSelectionTypes.PreviewFont;
                    }

                    SaveCreationSettingsToEditorPrefs(SaveFontCreationSettings());
                }
            }

            // FONT RENDERING PROGRESS BAR
            GUILayout.Space(1);
            progressRect = EditorGUILayout.GetControlRect(false, 18);

            GUI.enabled = true;
            EditorGUI.ProgressBar(progressRect, Mathf.Max(0.01f, m_renderingProgress), "Generation Progress");
            progressRect.x += 266;
            progressRect.y += 1;
            progressRect.width = 20;
            progressRect.height = 16;
            GUI.enabled = m_IsProcessing ? true : false;
            if (GUI.Button(progressRect, "X"))
            {
                TMPro_FontPlugin.SendCancellationRequest(CancellationRequestType.CancelInProgess);
                m_renderingProgress = 0;
                m_IsProcessing = false;
                isGenerationCancelled = true;
            }
            GUI.enabled = isEnabled;

            // FONT STATUS & INFORMATION
            GUISkin skin = GUI.skin;
            GUI.skin = TMP_UIStyleManager.TMP_GUISkin;

            GUILayout.Space(5);
            GUILayout.BeginVertical(TMP_UIStyleManager.TextAreaBoxWindow);
            output_ScrollPosition = EditorGUILayout.BeginScrollView(output_ScrollPosition, GUILayout.Height(145));
            EditorGUILayout.LabelField(m_Output_feedback, TMP_UIStyleManager.Label);
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUI.skin = skin;

            GUILayout.Space(10);

            // SAVE TEXTURE & CREATE and SAVE FONT XML FILE
            GUILayout.BeginHorizontal();
            {
                GUI.enabled = m_Font_Atlas != null ? true : false;    // Enable Save Button if font_Atlas is not Null.
                if (GUILayout.Button("Save") && GUI.enabled)
                {
                    if (m_SelectedFontAsset == null)
                    {
                        if (m_LegacyFontAsset != null)
                            SaveNewFontAssetWithSameName(m_LegacyFontAsset);
                        else
                            SaveNewFontAsset(m_SourceFontFile);
                    }
                    else
                    {
                        // Save over exiting Font Asset
                        string filePath = Path.GetFullPath(AssetDatabase.GetAssetPath(m_SelectedFontAsset)).Replace('\\', '/');

                        if (m_RenderMode < RenderModes.DistanceField16) // ((int)m_RenderMode & 0x10) == 0x10)
                            Save_Normal_FontAsset(filePath);
                        else if (m_RenderMode >= RenderModes.DistanceField16) // ((RasterModes)m_RenderMode & RasterModes.Raster_Mode_SDF) == RasterModes.Raster_Mode_SDF || m_RenderMode == RenderModes.DistanceFieldAA)
                            Save_SDF_FontAsset(filePath);
                    }
                }

                if (GUILayout.Button("Save As...") && GUI.enabled)
                {
                    if (m_SelectedFontAsset == null)
                    {
                        SaveNewFontAsset(m_SourceFontFile);
                    }
                    else
                    {
                        SaveNewFontAssetWithSameName(m_SelectedFontAsset);
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUI.enabled = true; // Re-enable GUI

            GUILayout.Space(5);

            GUILayout.EndVertical();

            GUILayout.Space(25);

            // If any options have been changed clear the font atlas as it is now invalid.
            if (m_IsFontAtlasInvalid)
                ClearGeneratedData();
 
            // Figure out the size of the current UI Panel
            rect = EditorGUILayout.GetControlRect(false, 5);
            if (Event.current.type == EventType.Repaint)
                m_UI_Panel_Size = rect;

            GUILayout.EndVertical();
        }


        /// <summary>
        /// Clear the previously generated data.
        /// </summary>
        void ClearGeneratedData()
        {
            m_IsFontAtlasInvalid = false;

            if (m_Font_Atlas != null)
            {
                DestroyImmediate(m_Font_Atlas);
                m_Font_Atlas = null;
            }

            m_SavedFontAtlas = null;

            m_Output_feedback = string.Empty;
            m_WarningMessage = string.Empty;
        }


        /// <summary>
        /// Function to update the feedback window showing the results of the latest generation.
        /// </summary>
        void UpdateRenderFeedbackWindow()
        {
            m_PointSize = m_font_faceInfo.pointSize;

            string missingGlyphReport = string.Empty;

            string colorTag = m_font_faceInfo.characterCount == m_character_Count ? "<color=#C0ffff>" : "<color=#ffff00>";
            string colorTag2 = "<color=#C0ffff>";

            missingGlyphReport = output_name_label + "<b>" + colorTag2 + m_font_faceInfo.name + "</color></b>";

            if (missingGlyphReport.Length > 60)
                missingGlyphReport += "\n" + output_size_label + "<b>" + colorTag2 + m_font_faceInfo.pointSize + "</color></b>";
            else
                missingGlyphReport += "  " + output_size_label + "<b>" + colorTag2 + m_font_faceInfo.pointSize + "</color></b>";

            missingGlyphReport += "\n" + output_count_label + "<b>" + colorTag + m_font_faceInfo.characterCount + "/" + m_character_Count + "</color></b>";

            // Report missing requested glyph
            missingGlyphReport += "\n\n<color=#ffff00><b>Missing Characters</b></color>";
            missingGlyphReport += "\n----------------------------------------";

            m_Output_feedback = missingGlyphReport;

            for (int i = 0; i < m_character_Count; i++)
            {
                if (m_font_glyphInfo[i].x == -1)
                {
                    missingGlyphReport += "\nID: <color=#C0ffff>" + m_font_glyphInfo[i].id + "\t</color>Hex: <color=#C0ffff>" + m_font_glyphInfo[i].id.ToString("X") + "\t</color>Char [<color=#C0ffff>" + (char)m_font_glyphInfo[i].id + "</color>]";

                    if (missingGlyphReport.Length < 16300)
                        m_Output_feedback = missingGlyphReport;
                }
            }

            if (missingGlyphReport.Length > 16300)
                m_Output_feedback += "\n\n<color=#ffff00>Report truncated.</color>\n<color=#c0ffff>See</color> \"TextMesh Pro\\Glyph Report.txt\"";

            // Save Missing Glyph Report file
            if (Directory.Exists("Assets/TextMesh Pro"))
            {
                missingGlyphReport = System.Text.RegularExpressions.Regex.Replace(missingGlyphReport, @"<[^>]*>", string.Empty);
                File.WriteAllText("Assets/TextMesh Pro/Glyph Report.txt", missingGlyphReport);
                AssetDatabase.Refresh();
            }

            //GUIUtility.systemCopyBuffer = missingGlyphReport;
        }


        void CreateFontTexture()
        {
            m_Font_Atlas = new Texture2D(m_AtlasWidth, m_AtlasHeight, TextureFormat.Alpha8, false, true);

            Color32[] colors = new Color32[m_AtlasWidth * m_AtlasHeight];

            for (int i = 0; i < (m_AtlasWidth * m_AtlasHeight); i++)
            {
                byte c = m_texture_buffer[i];
                colors[i] = new Color32(c, c, c, c);
            }
            // Clear allocation of 
            m_texture_buffer = null;

            if (m_RenderMode == RenderModes.Raster || m_RenderMode == RenderModes.RasterHinted)
                m_Font_Atlas.filterMode = FilterMode.Point;

            m_Font_Atlas.SetPixels32(colors, 0);
            m_Font_Atlas.Apply(false, true);

            // Saving File for Debug
            //var pngData = m_font_Atlas.EncodeToPNG();
            //File.WriteAllBytes("Assets/Textures/Debug Font Texture.png", pngData);	

            UpdateEditorWindowSize(m_Font_Atlas.width, m_Font_Atlas.height);
        }


        /// <summary>
        /// Open Save Dialog to provide the option save the font asset using the name of the source font file. This also appends SDF to the name if using any of the SDF Font Asset creation modes.
        /// </summary>
        /// <param name="sourceObject"></param>
        void SaveNewFontAsset(Object sourceObject)
        {
            string filePath;

            // Save new Font Asset and open save file requester at Source Font File location.
            string saveDirectory = new FileInfo(AssetDatabase.GetAssetPath(sourceObject)).DirectoryName;

            if (m_RenderMode < RenderModes.DistanceField16) // ((int)m_RenderMode & 0x10) == 0x10)
            {
                filePath = EditorUtility.SaveFilePanel("Save TextMesh Pro! Font Asset File", saveDirectory, sourceObject.name, "asset");

                if (filePath.Length == 0)
                    return;

                Save_Normal_FontAsset(filePath);
            }
            else if (m_RenderMode >= RenderModes.DistanceField16) // ((RasterModes)m_RenderMode & RasterModes.Raster_Mode_SDF) == RasterModes.Raster_Mode_SDF || m_RenderMode == RenderModes.DistanceFieldAA)
            {
                filePath = EditorUtility.SaveFilePanel("Save TextMesh Pro! Font Asset File", saveDirectory, sourceObject.name + " SDF", "asset");

                if (filePath.Length == 0)
                    return;

                Save_SDF_FontAsset(filePath);
            }
        }


        /// <summary>
        /// Open Save Dialog to provide the option to save the font asset under the same name.
        /// </summary>
        /// <param name="sourceObject"></param>
        void SaveNewFontAssetWithSameName(Object sourceObject)
        {
            string filePath;

            // Save new Font Asset and open save file requester at Source Font File location.
            string saveDirectory = new FileInfo(AssetDatabase.GetAssetPath(sourceObject)).DirectoryName;

            filePath = EditorUtility.SaveFilePanel("Save TextMesh Pro! Font Asset File", saveDirectory, sourceObject.name, "asset");

            if (filePath.Length == 0)
                return;

            if (m_RenderMode < RenderModes.DistanceField16) // ((int)m_RenderMode & 0x10) == 0x10)
            {
                Save_Normal_FontAsset(filePath);
            }
            else if (m_RenderMode >= RenderModes.DistanceField16) // ((RasterModes)m_RenderMode & RasterModes.Raster_Mode_SDF) == RasterModes.Raster_Mode_SDF || m_RenderMode == RenderModes.DistanceFieldAA)
            {
                Save_SDF_FontAsset(filePath);
            }
        }


        void Save_Normal_FontAsset(string filePath)
        {
            filePath = filePath.Substring(0, filePath.Length - 6); // Trim file extension from filePath.

            string dataPath = Application.dataPath;

            if (filePath.IndexOf(dataPath, System.StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                Debug.LogError("You're saving the font asset in a directory outside of this project folder. This is not supported. Please select a directory under \"" + dataPath + "\"");
                return;
            }

            string relativeAssetPath = filePath.Substring(dataPath.Length - 6);
            string tex_DirName = Path.GetDirectoryName(relativeAssetPath);
            string tex_FileName = Path.GetFileNameWithoutExtension(relativeAssetPath);
            string tex_Path_NoExt = tex_DirName + "/" + tex_FileName;

            // Check if TextMeshPro font asset already exists. If not, create a new one. Otherwise update the existing one.
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath(tex_Path_NoExt + ".asset", typeof(TMP_FontAsset)) as TMP_FontAsset;
            if (fontAsset == null)
            {
                //Debug.Log("Creating TextMeshPro font asset!");
                fontAsset = ScriptableObject.CreateInstance<TMP_FontAsset>(); // Create new TextMeshPro Font Asset.
                AssetDatabase.CreateAsset(fontAsset, tex_Path_NoExt + ".asset");

                //Set Font Asset Type
                fontAsset.fontAssetType = TMP_FontAsset.FontAssetTypes.Bitmap;

                // Reference to the source font file
                //font_asset.sourceFontFile = font_TTF as Font;

                // Add FaceInfo to Font Asset
                FaceInfo face = GetFaceInfo(m_font_faceInfo, 1);
                fontAsset.AddFaceInfo(face);

                // Add GlyphInfo[] to Font Asset
                TMP_Glyph[] glyphs = GetGlyphInfo(m_font_glyphInfo, 1);
                fontAsset.AddGlyphInfo(glyphs);

                // Get and Add Kerning Pairs to Font Asset
                if (includeKerningPairs)
                {
                    string fontFilePath = AssetDatabase.GetAssetPath(m_SourceFontFile);
                    KerningTable kerningTable = GetKerningTable(fontFilePath, (int)face.PointSize);
                    fontAsset.AddKerningInfo(kerningTable);
                }


                // Add Font Atlas as Sub-Asset
                fontAsset.atlas = m_Font_Atlas;
                m_Font_Atlas.name = tex_FileName + " Atlas";

                AssetDatabase.AddObjectToAsset(m_Font_Atlas, fontAsset);

                // Create new Material and Add it as Sub-Asset
                Shader default_Shader = Shader.Find("TextMeshPro/Bitmap"); // m_shaderSelection;
                Material tmp_material = new Material(default_Shader);
                tmp_material.name = tex_FileName + " Material";
                tmp_material.SetTexture(ShaderUtilities.ID_MainTex, m_Font_Atlas);
                fontAsset.material = tmp_material;

                AssetDatabase.AddObjectToAsset(tmp_material, fontAsset);

            }
            else
            {
                // Find all Materials referencing this font atlas.
                Material[] material_references = TMP_EditorUtility.FindMaterialReferences(fontAsset);

                // Destroy Assets that will be replaced.
                DestroyImmediate(fontAsset.atlas, true);

                //Set Font Asset Type
                fontAsset.fontAssetType = TMP_FontAsset.FontAssetTypes.Bitmap;

                // Add FaceInfo to Font Asset
                FaceInfo face = GetFaceInfo(m_font_faceInfo, 1);
                fontAsset.AddFaceInfo(face);

                // Add GlyphInfo[] to Font Asset
                TMP_Glyph[] glyphs = GetGlyphInfo(m_font_glyphInfo, 1);
                fontAsset.AddGlyphInfo(glyphs);

                // Get and Add Kerning Pairs to Font Asset
                if (includeKerningPairs)
                {
                    string fontFilePath = AssetDatabase.GetAssetPath(m_SourceFontFile);
                    KerningTable kerningTable = GetKerningTable(fontFilePath, (int)face.PointSize);
                    fontAsset.AddKerningInfo(kerningTable);
                }

                // Add Font Atlas as Sub-Asset
                fontAsset.atlas = m_Font_Atlas;
                m_Font_Atlas.name = tex_FileName + " Atlas";

                // Special handling due to a bug in earlier versions of Unity.
                m_Font_Atlas.hideFlags = HideFlags.None;
                fontAsset.material.hideFlags = HideFlags.None;

                AssetDatabase.AddObjectToAsset(m_Font_Atlas, fontAsset);

                // Assign new font atlas texture to the existing material.
                fontAsset.material.SetTexture(ShaderUtilities.ID_MainTex, fontAsset.atlas);

                // Update the Texture reference on the Material
                for (int i = 0; i < material_references.Length; i++)
                {
                    material_references[i].SetTexture(ShaderUtilities.ID_MainTex, m_Font_Atlas);
                }
            }

            // Save Font Asset creation settings
            m_SelectedFontAsset = fontAsset;
            m_LegacyFontAsset = null;
            fontAsset.creationSettings = SaveFontCreationSettings();

            AssetDatabase.SaveAssets();

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(fontAsset));  // Re-import font asset to get the new updated version.

            //EditorUtility.SetDirty(font_asset);
            fontAsset.ReadFontDefinition();

            AssetDatabase.Refresh();

            m_Font_Atlas = null;

            // NEED TO GENERATE AN EVENT TO FORCE A REDRAW OF ANY TEXTMESHPRO INSTANCES THAT MIGHT BE USING THIS FONT ASSET
            TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);
        }


        void Save_SDF_FontAsset(string filePath)
        {
            filePath = filePath.Substring(0, filePath.Length - 6); // Trim file extension from filePath.

            string dataPath = Application.dataPath;

            if (filePath.IndexOf(dataPath, System.StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                Debug.LogError("You're saving the font asset in a directory outside of this project folder. This is not supported. Please select a directory under \"" + dataPath + "\"");
                return;
            }

            string relativeAssetPath = filePath.Substring(dataPath.Length - 6);
            string tex_DirName = Path.GetDirectoryName(relativeAssetPath);
            string tex_FileName = Path.GetFileNameWithoutExtension(relativeAssetPath);
            string tex_Path_NoExt = tex_DirName + "/" + tex_FileName;


            // Check if TextMeshPro font asset already exists. If not, create a new one. Otherwise update the existing one.
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(tex_Path_NoExt + ".asset");
            if (fontAsset == null)
            {
                //Debug.Log("Creating TextMeshPro font asset!");
                fontAsset = ScriptableObject.CreateInstance<TMP_FontAsset>(); // Create new TextMeshPro Font Asset.
                AssetDatabase.CreateAsset(fontAsset, tex_Path_NoExt + ".asset");

                // Reference to the source font file
                //font_asset.sourceFontFile = font_TTF as Font;

                //Set Font Asset Type
                fontAsset.fontAssetType = TMP_FontAsset.FontAssetTypes.SDF;

                //if (m_destination_Atlas != null)
                //    m_font_Atlas = m_destination_Atlas;

                // If using the C# SDF creation mode, we need the scale down factor.
                int scaleDownFactor = 1; // ((RasterModes)m_RenderMode & RasterModes.Raster_Mode_SDF) == RasterModes.Raster_Mode_SDF || m_RenderMode == RenderModes.DistanceFieldAA ? 1 : font_scaledownFactor;

                // Add FaceInfo to Font Asset
                FaceInfo face = GetFaceInfo(m_font_faceInfo, scaleDownFactor);
                fontAsset.AddFaceInfo(face);

                // Add GlyphInfo[] to Font Asset
                TMP_Glyph[] glyphs = GetGlyphInfo(m_font_glyphInfo, scaleDownFactor);
                fontAsset.AddGlyphInfo(glyphs);

                // Get and Add Kerning Pairs to Font Asset
                if (includeKerningPairs)
                {
                    string fontFilePath = AssetDatabase.GetAssetPath(m_SourceFontFile);
                    KerningTable kerningTable = GetKerningTable(fontFilePath, (int)face.PointSize);
                    fontAsset.AddKerningInfo(kerningTable);
                }

                // Add Line Breaking Rules
                //LineBreakingTable lineBreakingTable = new LineBreakingTable();
                //

                // Add Font Atlas as Sub-Asset
                fontAsset.atlas = m_Font_Atlas;
                m_Font_Atlas.name = tex_FileName + " Atlas";

                AssetDatabase.AddObjectToAsset(m_Font_Atlas, fontAsset);

                // Create new Material and Add it as Sub-Asset
                Shader default_Shader = Shader.Find("TextMeshPro/Distance Field"); //m_shaderSelection;
                Material tmp_material = new Material(default_Shader);

                tmp_material.name = tex_FileName + " Material";
                tmp_material.SetTexture(ShaderUtilities.ID_MainTex, m_Font_Atlas);
                tmp_material.SetFloat(ShaderUtilities.ID_TextureWidth, m_Font_Atlas.width);
                tmp_material.SetFloat(ShaderUtilities.ID_TextureHeight, m_Font_Atlas.height);

                int spread = m_Padding + 1;
                tmp_material.SetFloat(ShaderUtilities.ID_GradientScale, spread); // Spread = Padding for Brute Force SDF.

                tmp_material.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
                tmp_material.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);

                fontAsset.material = tmp_material;

                AssetDatabase.AddObjectToAsset(tmp_material, fontAsset);

            }
            else
            {
                // Find all Materials referencing this font atlas.
                Material[] material_references = TMP_EditorUtility.FindMaterialReferences(fontAsset);

                // Destroy Assets that will be replaced.
                DestroyImmediate(fontAsset.atlas, true);

                //Set Font Asset Type
                fontAsset.fontAssetType = TMP_FontAsset.FontAssetTypes.SDF;

                int scaleDownFactor = 1; // ((RasterModes)m_RenderMode & RasterModes.Raster_Mode_SDF) == RasterModes.Raster_Mode_SDF || m_RenderMode == RenderModes.DistanceFieldAA ? 1 : font_scaledownFactor;
                // Add FaceInfo to Font Asset  
                FaceInfo face = GetFaceInfo(m_font_faceInfo, scaleDownFactor);
                fontAsset.AddFaceInfo(face);

                // Add GlyphInfo[] to Font Asset
                TMP_Glyph[] glyphs = GetGlyphInfo(m_font_glyphInfo, scaleDownFactor);
                fontAsset.AddGlyphInfo(glyphs);

                // Get and Add Kerning Pairs to Font Asset
                if (includeKerningPairs)
                {
                    string fontFilePath = AssetDatabase.GetAssetPath(m_SourceFontFile);
                    KerningTable kerningTable = GetKerningTable(fontFilePath, (int)face.PointSize);
                    fontAsset.AddKerningInfo(kerningTable);
                }

                // Add Font Atlas as Sub-Asset
                fontAsset.atlas = m_Font_Atlas;
                m_Font_Atlas.name = tex_FileName + " Atlas";

                // Special handling due to a bug in earlier versions of Unity.
                m_Font_Atlas.hideFlags = HideFlags.None;
                fontAsset.material.hideFlags = HideFlags.None;

                AssetDatabase.AddObjectToAsset(m_Font_Atlas, fontAsset);

                // Assign new font atlas texture to the existing material.
                fontAsset.material.SetTexture(ShaderUtilities.ID_MainTex, fontAsset.atlas);

                // Update the Texture reference on the Material
                for (int i = 0; i < material_references.Length; i++)
                {
                    material_references[i].SetTexture(ShaderUtilities.ID_MainTex, m_Font_Atlas);
                    material_references[i].SetFloat(ShaderUtilities.ID_TextureWidth, m_Font_Atlas.width);
                    material_references[i].SetFloat(ShaderUtilities.ID_TextureHeight, m_Font_Atlas.height);

                    int spread = m_Padding + 1;
                    material_references[i].SetFloat(ShaderUtilities.ID_GradientScale, spread); // Spread = Padding for Brute Force SDF.

                    material_references[i].SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
                    material_references[i].SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);
                }
            }

            // Saving File for Debug
            //var pngData = destination_Atlas.EncodeToPNG();
            //File.WriteAllBytes("Assets/Textures/Debug Distance Field.png", pngData);

            // Save Font Asset creation settings
            m_SelectedFontAsset = fontAsset;
            m_LegacyFontAsset = null;
            fontAsset.creationSettings = SaveFontCreationSettings();

            AssetDatabase.SaveAssets();

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(fontAsset));  // Re-import font asset to get the new updated version.

            fontAsset.ReadFontDefinition();

            AssetDatabase.Refresh();

            m_Font_Atlas = null;

            // NEED TO GENERATE AN EVENT TO FORCE A REDRAW OF ANY TEXTMESHPRO INSTANCES THAT MIGHT BE USING THIS FONT ASSET
            TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);
        }


        /// <summary>
        /// Internal method to save the Font Asset Creation Settings
        /// </summary>
        /// <returns></returns>
        FontAssetCreationSettings SaveFontCreationSettings()
        {
            FontAssetCreationSettings settings = new FontAssetCreationSettings();

            //settings.sourceFontFileName = m_SourceFontFile.name;
            settings.sourceFontFileGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_SourceFontFile));
            settings.pointSizeSamplingMode = m_PointSizeSamplingMode;
            settings.pointSize = m_PointSize;
            settings.padding = m_Padding;
            settings.packingMode = (int)m_PackingMode;
            settings.atlasWidth = m_AtlasWidth;
            settings.atlasHeight = m_AtlasHeight;
            settings.characterSetSelectionMode = m_CharacterSetSelectionMode;
            settings.characterSequence = m_CharacterSequence;
            settings.referencedFontAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_ReferencedFontAsset));
            settings.referencedTextAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_CharactersFromFile));
            settings.fontStyle = (int)m_FontStyle;
            settings.fontStyleModifier = m_FontStyleValue;
            settings.renderMode = (int)m_RenderMode;
            settings.includeFontFeatures = includeKerningPairs;

            return settings;
        }


        /// <summary>
        /// Internal method to load the Font Asset Creation Settings
        /// </summary>
        /// <param name="settings"></param>
        void LoadFontCreationSettings(FontAssetCreationSettings settings)
        {
            m_SourceFontFile = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(settings.sourceFontFileGUID));
            m_PointSizeSamplingMode  = settings.pointSizeSamplingMode;
            m_PointSize = settings.pointSize;
            m_Padding = settings.padding;
            m_PackingMode = (FontPackingModes)settings.packingMode;
            m_AtlasWidth = settings.atlasWidth;
            m_AtlasHeight = settings.atlasHeight;
            m_CharacterSetSelectionMode = settings.characterSetSelectionMode;
            m_CharacterSequence = settings.characterSequence;
            m_ReferencedFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(settings.referencedFontAssetGUID));
            m_CharactersFromFile = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(settings.referencedTextAssetGUID));
            m_FontStyle = (FaceStyles)settings.fontStyle;
            m_FontStyleValue = settings.fontStyleModifier;
            m_RenderMode = (RenderModes)settings.renderMode;
            includeKerningPairs = settings.includeFontFeatures;
        }


        /// <summary>
        /// Save the latest font asset creation settings to EditorPrefs.
        /// </summary>
        /// <param name="settings"></param>
        void SaveCreationSettingsToEditorPrefs(FontAssetCreationSettings settings)
        {
            // Create new list if one does not already exist
            if (m_FontAssetCreationSettingsContainer == null)
            {
                m_FontAssetCreationSettingsContainer = new FontAssetCreationSettingsContainer();
                m_FontAssetCreationSettingsContainer.fontAssetCreationSettings = new List<FontAssetCreationSettings>();
            }

            // Add new creation settings to the list
            m_FontAssetCreationSettingsContainer.fontAssetCreationSettings.Add(settings);

            // Since list should only contain the most 4 recent settings, we remove the first element if list exceeds 4 elements.
            if (m_FontAssetCreationSettingsContainer.fontAssetCreationSettings.Count > 4)
                m_FontAssetCreationSettingsContainer.fontAssetCreationSettings.RemoveAt(0);

            m_FontAssetCreationSettingsCurrentIndex = m_FontAssetCreationSettingsContainer.fontAssetCreationSettings.Count - 1;

            // Serialize list to JSON
            string serializedSettings = JsonUtility.ToJson(m_FontAssetCreationSettingsContainer, true);

            EditorPrefs.SetString(k_FontAssetCreationSettingsContainerKey, serializedSettings);
        }



        void UpdateEditorWindowSize(float width, float height)
        {
            m_previewWindow_Size = new Vector2(768, 768);

            if (width > height)
            {
                m_previewWindow_Size = new Vector2(768, height / (width / 768));
            }
            else if (height > width)
            {
                m_previewWindow_Size = new Vector2(width / (height / 768), 768);
            }

            m_editorWindow.minSize = new Vector2(m_previewWindow_Size.x + 330, Mathf.Max(m_UI_Panel_Size.y + 20f, m_previewWindow_Size.y + 20f));
            m_editorWindow.maxSize = m_editorWindow.minSize + new Vector2(.25f, 0);
        }


        void DrawPreview()
        {

            // Display Texture Area
            GUILayout.BeginVertical(TMP_UIStyleManager.TextureAreaBox);

            Rect pixelRect = GUILayoutUtility.GetRect(m_previewWindow_Size.x, m_previewWindow_Size.y, TMP_UIStyleManager.Section_Label);

            if (m_destination_Atlas != null && previewSelection == PreviewSelectionTypes.PreviewDistanceField)
            {
                EditorGUI.DrawTextureAlpha(new Rect(pixelRect.x, pixelRect.y, m_previewWindow_Size.x, m_previewWindow_Size.y), m_destination_Atlas, ScaleMode.ScaleToFit);
            }
            //else if (m_texture_Atlas != null && previewSelection == PreviewSelectionTypes.PreviewTexture)
            //{
            //    GUI.DrawTexture(new Rect(pixelRect.x, pixelRect.y, m_previewWindow_Size.x, m_previewWindow_Size.y), m_texture_Atlas, ScaleMode.ScaleToFit); 
            //}
            else if (m_Font_Atlas != null && previewSelection == PreviewSelectionTypes.PreviewFont)
            {
                EditorGUI.DrawTextureAlpha(new Rect(pixelRect.x, pixelRect.y, m_previewWindow_Size.x, m_previewWindow_Size.y), m_Font_Atlas, ScaleMode.ScaleToFit);
            }
            else if (m_SavedFontAtlas != null && previewSelection == PreviewSelectionTypes.PreviewFont)
            {
                EditorGUI.DrawTextureAlpha(new Rect(pixelRect.x, pixelRect.y, m_previewWindow_Size.x, m_previewWindow_Size.y), m_SavedFontAtlas, ScaleMode.ScaleToFit);
            }

            GUILayout.EndVertical();
        }


        // Convert from FT_FaceInfo to FaceInfo
        FaceInfo GetFaceInfo(FT_FaceInfo ft_face, int scaleFactor)
        {
            FaceInfo face = new FaceInfo();

            face.Name = ft_face.name;
            face.PointSize = (float)ft_face.pointSize / scaleFactor;
            face.Padding = ft_face.padding / scaleFactor;
            face.LineHeight = ft_face.lineHeight / scaleFactor;
            face.CapHeight = 0;
            face.Baseline = 0;
            face.Ascender = ft_face.ascender / scaleFactor;
            face.Descender = ft_face.descender / scaleFactor;
            face.CenterLine = ft_face.centerLine / scaleFactor;
            face.Underline = ft_face.underline / scaleFactor;
            face.UnderlineThickness = ft_face.underlineThickness == 0 ? 5 : ft_face.underlineThickness / scaleFactor; // Set Thickness to 5 if TTF value is Zero.
            face.strikethrough = (face.Ascender + face.Descender) / 2.75f;
            face.strikethroughThickness = face.UnderlineThickness;
            face.SuperscriptOffset = face.Ascender;
            face.SubscriptOffset = face.Underline;
            face.SubSize = 0.5f;
            //face.CharacterCount = ft_face.characterCount;
            face.AtlasWidth = ft_face.atlasWidth / scaleFactor;
            face.AtlasHeight = ft_face.atlasHeight / scaleFactor;

            return face;
        }


        // Convert from FT_GlyphInfo[] to GlyphInfo[]
        TMP_Glyph[] GetGlyphInfo(FT_GlyphInfo[] ft_glyphs, int scaleFactor)
        {
            List<TMP_Glyph> glyphs = new List<TMP_Glyph>();
            List<int> kerningSet = new List<int>();

            for (int i = 0; i < ft_glyphs.Length; i++)
            {
                TMP_Glyph g = new TMP_Glyph();

                g.id = ft_glyphs[i].id;
                g.x = ft_glyphs[i].x / scaleFactor;
                g.y = ft_glyphs[i].y / scaleFactor;
                g.width = ft_glyphs[i].width / scaleFactor;
                g.height = ft_glyphs[i].height / scaleFactor;
                g.xOffset = ft_glyphs[i].xOffset / scaleFactor;
                g.yOffset = ft_glyphs[i].yOffset / scaleFactor;
                g.xAdvance = ft_glyphs[i].xAdvance / scaleFactor;

                // Filter out characters with missing glyphs.
                if (g.x == -1)
                    continue;

                glyphs.Add(g);
                kerningSet.Add(g.id);
            }

            m_kerningSet = kerningSet.ToArray();

            return glyphs.ToArray();
        }


        // Get Kerning Pairs
        public KerningTable GetKerningTable(string fontFilePath, int pointSize)
        {
            KerningTable kerningInfo = new KerningTable();
            kerningInfo.kerningPairs = new List<KerningPair>();

            // Temporary Array to hold the kerning pairs from the Native Plug-in.
            FT_KerningPair[] kerningPairs = new FT_KerningPair[7500];

            int kpCount = TMPro_FontPlugin.FT_GetKerningPairs(fontFilePath, m_kerningSet, m_kerningSet.Length, kerningPairs);

            for (int i = 0; i < kpCount; i++)
            {
                // Proceed to add each kerning pairs.
                KerningPair kp = new KerningPair((uint)kerningPairs[i].ascII_Left, (uint)kerningPairs[i].ascII_Right, kerningPairs[i].xAdvanceOffset * pointSize);

                // Filter kerning pairs to avoid duplicates
                int index = kerningInfo.kerningPairs.FindIndex(item => item.firstGlyph == kp.firstGlyph && item.secondGlyph == kp.secondGlyph);

                if (index == -1)
                    kerningInfo.kerningPairs.Add(kp);
                else
                    if (!TMP_Settings.warningsDisabled) Debug.LogWarning("Kerning Key for [" + kp.firstGlyph + "] and [" + kp.secondGlyph + "] is a duplicate.");

            }

            return kerningInfo;
        }


        private string[] UpdateShaderList(RenderModes mode, out Shader[] shaders)
        {
            // Get shaders for the given RenderModes.
            string searchPattern = "t:Shader" + " TMP_"; // + fontAsset.name.Split(new char[] { ' ' })[0];

            if (mode >= RenderModes.DistanceField16) // ((RasterModes)mode & RasterModes.Raster_Mode_SDF) == RasterModes.Raster_Mode_SDF || mode == RenderModes.DistanceFieldAA)
                searchPattern += " SDF";
            else
                searchPattern += " Bitmap";

            // Get materials matching the search pattern.
            string[] shaderGUIDs = AssetDatabase.FindAssets(searchPattern);

            string[] shaderList = new string[shaderGUIDs.Length];
            shaders = new Shader[shaderGUIDs.Length];

            for (int i = 0; i < shaderGUIDs.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(shaderGUIDs[i]);
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                shaders[i] = shader;

                string name = shader.name.Replace("TextMeshPro/", "");
                name = name.Replace("Mobile/", "Mobile - ");
                shaderList[i] = name;
            }

            return shaderList;
        }

    }
}