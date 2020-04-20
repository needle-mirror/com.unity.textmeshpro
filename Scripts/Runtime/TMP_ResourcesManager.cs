using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TMPro
{
    /// <summary>
    ///
    /// </summary>
    public class TMP_ResourceManager
    {
        private static readonly TMP_ResourceManager s_instance = new TMP_ResourceManager();

        static TMP_ResourceManager() { }

        // ======================================================
        // FONT ASSET MANAGEMENT - Fields, Properties and Functions
        // ======================================================

        private static readonly List<TMP_FontAsset> s_FontAssetReferences = new List<TMP_FontAsset>();
        private static readonly Dictionary<int, TMP_FontAsset> s_FontAssetReferenceLookup = new Dictionary<int, TMP_FontAsset>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="fontAsset"></param>
        public static void AddFontAsset(TMP_FontAsset fontAsset)
        {
            int hashcode = fontAsset.hashCode;

            if (s_FontAssetReferenceLookup.ContainsKey(hashcode))
                return;

            s_FontAssetReferences.Add(fontAsset);
            s_FontAssetReferenceLookup.Add(hashcode, fontAsset);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hashcode"></param>
        /// <param name="fontAsset"></param>
        /// <returns></returns>
        public static bool TryGetFontAsset(int hashcode, out TMP_FontAsset fontAsset)
        {
            fontAsset = null;

            return s_FontAssetReferenceLookup.TryGetValue(hashcode, out fontAsset);
        }


        internal static void RebuildFontAssetCache(int instanceID)
        {
            // Iterate over loaded font assets to update affected font assets
            for (int i = 0; i < s_FontAssetReferences.Count; i++)
            {
                TMP_FontAsset fontAsset = s_FontAssetReferences[i];

                if (fontAsset.FallbackSearchQueryLookup.Contains(instanceID))
                    fontAsset.ReadFontAssetDefinition();
            }
        }
    }
}
