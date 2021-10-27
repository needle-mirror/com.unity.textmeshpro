using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEditor;
using UnityEditor.TextCore.Text;


namespace TMPro.EditorUtilities
{
    public class TMPro_FontAssetCreatorWindow : EditorWindow
    {
        private static FontAssetCreatorWindow m_Window;

        [MenuItem("Window/TextMeshPro/Font Asset Creator", false, 2025)]
        public static void ShowFontAtlasCreatorWindow()
        {
            m_Window = GetWindow<FontAssetCreatorWindow>();
            m_Window.titleContent = new GUIContent("Font Asset Creator");
            m_Window.Focus();

            // Make sure TMP Essential Resources have been imported.
            CheckEssentialResources();
        }

        // Make sure TMP Essential Resources have been imported.
        static void CheckEssentialResources()
        {
            if (TMP_Settings.instance == null)
            {
                m_Window.Close();
                TextEventManager.RESOURCE_LOAD_EVENT.Add(ON_RESOURCES_LOADED);
            }
        }

        // Event received when TMP resources have been loaded.
        static void ON_RESOURCES_LOADED()
        {
            TextEventManager.RESOURCE_LOAD_EVENT.Remove(ON_RESOURCES_LOADED);

            ShowFontAtlasCreatorWindow();
        }
    }
}
