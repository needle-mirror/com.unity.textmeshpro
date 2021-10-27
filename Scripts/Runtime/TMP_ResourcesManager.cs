/*
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;


namespace TMPro
{
    /// <summary>
    ///
    /// </summary>
    public class TMP_ResourceManager
    {
        // ======================================================
        // TEXT SETTINGS MANAGEMENT
        // ======================================================

        private static TMP_Settings s_TextSettings;

        internal static TMP_Settings GetTextSettings()
        {
            if (s_TextSettings == null)
            {
                // Try loading the TMP Settings from a Resources folder in the user project.
                s_TextSettings = Resources.Load<TMP_Settings>("TextSettings"); // ?? ScriptableObject.CreateInstance<TMP_Settings>();

                #if UNITY_EDITOR
                if (s_TextSettings == null)
                {
                    // Open TMP Resources Importer to enable the user to import the TMP Essential Resources and option TMP Examples & Extras
                    TMP_PackageResourceImporterWindow.ShowPackageImporterWindow();
                }
                #endif
            }

            return s_TextSettings;
        }

        // ======================================================
        // FONT ASSET MANAGEMENT - Fields, Properties and Functions
        // ======================================================

        struct FontAssetRef
        {
            public int nameHashCode;
            public int familyNameHashCode;
            public int styleNameHashCode;
            public long familyNameAndStyleHashCode;
            public readonly FontAsset fontAsset;

            public FontAssetRef(int nameHashCode, int familyNameHashCode, int styleNameHashCode, FontAsset fontAsset)
            {
                this.nameHashCode = nameHashCode;
                this.familyNameHashCode = familyNameHashCode;
                this.styleNameHashCode = styleNameHashCode;
                this.familyNameAndStyleHashCode = (long) styleNameHashCode << 32 | (uint) familyNameHashCode;
                this.fontAsset = fontAsset;
            }
        }

        static readonly Dictionary<int, FontAssetRef> s_FontAssetReferences = new Dictionary<int, FontAssetRef>();
        static readonly Dictionary<int, FontAsset> s_FontAssetNameReferenceLookup = new Dictionary<int, FontAsset>();
        static readonly Dictionary<long, FontAsset> s_FontAssetFamilyNameAndStyleReferenceLookup = new Dictionary<long, FontAsset>();
        static readonly List<int> s_FontAssetRemovalList = new List<int>(16);

        static readonly int k_RegularStyleHashCode = TMP_TextUtilities.GetHashCode("Regular");

        /// <summary>
        /// Add font asset to resource manager.
        /// </summary>
        /// <param name="fontAsset">Font asset to be added to the resource manager.</param>
        //[System.Obsolete("AddFontAsset() has been deprecated. Use TextResourceManager.AddFontAsset() instead. (UnityUpgradable) -> TextResourceManager.AddFontAsset()")]
        public static void AddFontAsset(FontAsset fontAsset)
        {
            // Obsolete. Use TextResourceManager.AddFontAsset() instead.
        }

        /// <summary>
        /// Remove font asset from resource manager.
        /// </summary>
        /// <param name="fontAsset">Font asset to be removed from the resource manager.</param>
        //[System.Obsolete("RemovedFontAsset() has been deprecated. Use TextResourceManager.RemoveFontAsset() instead. (UnityUpgradable) -> TextResourceManager.RemoveFontAsset()")]
        public static void RemoveFontAsset(FontAsset fontAsset)
        {
            // Obsolete. Use TextResourceManager.RemoveFontAsset() instead.
        }

        /// <summary>
        /// Try getting a reference to the font asset using the hash code calculated from its file name.
        /// </summary>
        /// <param name="nameHashcode"></param>
        /// <param name="fontAsset"></param>
        /// <returns></returns>
        internal static bool TryGetFontAssetByName(int nameHashcode, out FontAsset fontAsset)
        {
            fontAsset = null;

            return s_FontAssetNameReferenceLookup.TryGetValue(nameHashcode, out fontAsset);
        }

        /// <summary>
        /// Try getting a reference to the font asset using the hash code calculated from font's family and style name.
        /// </summary>
        /// <param name="familyNameHashCode"></param>
        /// <param name="styleNameHashCode"></param>
        /// <param name="fontAsset"></param>
        /// <returns></returns>
        internal static bool TryGetFontAssetByFamilyName(int familyNameHashCode, int styleNameHashCode, out FontAsset fontAsset)
        {
            fontAsset = null;

            if (styleNameHashCode == 0)
                styleNameHashCode = k_RegularStyleHashCode;

            long familyAndStyleNameHashCode = (long) styleNameHashCode << 32 | (uint) familyNameHashCode;

            return s_FontAssetFamilyNameAndStyleReferenceLookup.TryGetValue(familyAndStyleNameHashCode, out fontAsset);
        }

        /// <summary>
        ///
        /// </summary>
        internal static void RebuildFontAssetCache()
        {
            // Iterate over loaded font assets to update affected font assets
            foreach (var pair in s_FontAssetReferences)
            {
                FontAssetRef fontAssetRef = pair.Value;

                FontAsset fontAsset = fontAssetRef.fontAsset;

                if (fontAsset == null)
                {
                    // Remove font asset from our lookup dictionaries
                    s_FontAssetNameReferenceLookup.Remove(fontAssetRef.nameHashCode);
                    s_FontAssetFamilyNameAndStyleReferenceLookup.Remove(fontAssetRef.familyNameAndStyleHashCode);

                    // Add font asset to our removal list
                    s_FontAssetRemovalList.Add(pair.Key);
                    continue;
                }

                fontAsset.InitializeCharacterLookupDictionary();
                fontAsset.AddSynthesizedCharactersAndFaceMetrics();
            }

            // Remove font assets in our removal list from our font asset references
            for (int i = 0; i < s_FontAssetRemovalList.Count; i++)
            {
                s_FontAssetReferences.Remove(s_FontAssetRemovalList[i]);
            }
            s_FontAssetRemovalList.Clear();

            TextEventManager.ON_FONT_PROPERTY_CHANGED(true, null);
        }
    }
}
*/
