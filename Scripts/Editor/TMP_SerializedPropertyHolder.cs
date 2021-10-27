using UnityEngine;
using UnityEngine.TextCore.Text;


namespace TMPro
{
    class TMP_SerializedPropertyHolder : ScriptableObject
    {
        public FontAsset fontAsset;
        public uint firstCharacter;
        public uint secondCharacter;

        public TMP_GlyphPairAdjustmentRecord glyphPairAdjustmentRecord = new TMP_GlyphPairAdjustmentRecord(new TMP_GlyphAdjustmentRecord(), new TMP_GlyphAdjustmentRecord());
    }
}
