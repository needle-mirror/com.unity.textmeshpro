using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;


namespace TMPro.EditorUtilities
{
    internal struct GlyphProxy
    {
        public uint index;
        public GlyphRect glyphRect;
        public GlyphMetrics metrics;
        public int atlasIndex;
    }

    internal static class TMP_PropertyDrawerUtilities
    {
        internal static bool s_RefreshGlyphProxyLookup;

        internal static void RefreshGlyphProxyLookup(SerializedObject so, Dictionary<uint, GlyphProxy> glyphLookup)
        {
            if (glyphLookup != null)
            {
                glyphLookup.Clear();
                PopulateGlyphProxyLookupDictionary(so, glyphLookup);
            }

            s_RefreshGlyphProxyLookup = false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="so"></param>
        /// <param name="lookupDictionary"></param>
        internal static void PopulateGlyphProxyLookupDictionary(SerializedObject so, Dictionary<uint, GlyphProxy> lookupDictionary)
        {
            if (lookupDictionary == null)
                return;

            // Get reference to serialized property for the glyph table
            SerializedProperty glyphTable = so.FindProperty("m_GlyphTable");

            for (int i = 0; i < glyphTable.arraySize; i++)
            {
                SerializedProperty glyphProperty = glyphTable.GetArrayElementAtIndex(i);
                GlyphProxy proxy = GetGlyphProxyFromSerializedProperty(glyphProperty);

                lookupDictionary.Add(proxy.index, proxy);
            }
        }

        internal static void PopulateSpriteGlyphProxyLookupDictionary(SerializedObject so, Dictionary<uint, GlyphProxy> lookupDictionary)
        {
            if (lookupDictionary == null)
                return;

            // Get reference to serialized property for the glyph table
            SerializedProperty glyphTable = so.FindProperty("m_SpriteGlyphTable");

            for (int i = 0; i < glyphTable.arraySize; i++)
            {
                SerializedProperty glyphProperty = glyphTable.GetArrayElementAtIndex(i);
                GlyphProxy proxy = GetGlyphProxyFromSerializedProperty(glyphProperty);

                lookupDictionary.Add(proxy.index, proxy);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        static GlyphProxy GetGlyphProxyFromSerializedProperty(SerializedProperty property)
        {
            GlyphProxy proxy = new GlyphProxy();
            proxy.index = (uint)property.FindPropertyRelative("m_Index").intValue;

            SerializedProperty glyphRectProperty = property.FindPropertyRelative("m_GlyphRect");
            proxy.glyphRect = new GlyphRect();
            proxy.glyphRect.x = glyphRectProperty.FindPropertyRelative("m_X").intValue;
            proxy.glyphRect.y = glyphRectProperty.FindPropertyRelative("m_Y").intValue;
            proxy.glyphRect.width = glyphRectProperty.FindPropertyRelative("m_Width").intValue;
            proxy.glyphRect.height = glyphRectProperty.FindPropertyRelative("m_Height").intValue;

            SerializedProperty glyphMetricsProperty = property.FindPropertyRelative("m_Metrics");
            proxy.metrics = new GlyphMetrics();
            proxy.metrics.horizontalBearingX = glyphMetricsProperty.FindPropertyRelative("m_HorizontalBearingX").floatValue;
            proxy.metrics.horizontalBearingY = glyphMetricsProperty.FindPropertyRelative("m_HorizontalBearingY").floatValue;
            proxy.metrics.horizontalAdvance = glyphMetricsProperty.FindPropertyRelative("m_HorizontalAdvance").floatValue;
            proxy.metrics.width = glyphMetricsProperty.FindPropertyRelative("m_Width").floatValue;
            proxy.metrics.height = glyphMetricsProperty.FindPropertyRelative("m_Height").floatValue;

            return proxy;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="glyphIndex"></param>
        /// <param name="texture"></param>
        /// <returns></returns>
        internal static bool TryGetAtlasTextureFromSerializedObject(SerializedObject serializedObject, int glyphIndex, out Texture2D texture)
        {
            SerializedProperty atlasTextureProperty = serializedObject.FindProperty("m_AtlasTextures");

            texture = atlasTextureProperty.GetArrayElementAtIndex(glyphIndex).objectReferenceValue as Texture2D;

            if (texture == null)
                return false;

            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="texture"></param>
        /// <param name="mat"></param>
        /// <returns></returns>
        internal static bool TryGetMaterial(SerializedObject serializedObject, Texture2D texture, out Material mat)
        {
            GlyphRenderMode atlasRenderMode = (GlyphRenderMode)serializedObject.FindProperty("m_AtlasRenderMode").intValue;

            if (((GlyphRasterModes)atlasRenderMode & GlyphRasterModes.RASTER_MODE_BITMAP) == GlyphRasterModes.RASTER_MODE_BITMAP)
            {
                #if TEXTCORE_FONT_ENGINE_1_5_OR_NEWER
                if (atlasRenderMode == GlyphRenderMode.COLOR || atlasRenderMode == GlyphRenderMode.COLOR_HINTED)
                    mat = TMP_FontAssetEditor.internalRGBABitmapMaterial;
                else
                    mat = TMP_FontAssetEditor.internalBitmapMaterial;
                #else
                mat = TMP_FontAssetEditor.internalBitmapMaterial;
                #endif

                if (mat == null)
                    return false;

                mat.mainTexture = texture;
            }
            else
            {
                mat = TMP_FontAssetEditor.internalSDFMaterial;

                if (mat == null)
                    return false;

                int padding = serializedObject.FindProperty("m_AtlasPadding").intValue;
                mat.mainTexture = texture;
                mat.SetFloat(ShaderUtilities.ID_GradientScale, padding + 1);
            }

            return true;
        }
    }
}
