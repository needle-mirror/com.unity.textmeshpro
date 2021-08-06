using System;
using UnityEngine;


namespace TMPro
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public struct SingleSubstitutionRecord
    {

    }

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public struct MultipleSubstitutionRecord
    {
        /// <summary>
        ///
        /// </summary>
        public uint targetGlyphID { get { return m_TargetGlyphID; } set { m_TargetGlyphID = value; } }

        public uint[] substituteGlyphIDs { get { return m_SubstituteGlyphIDs; } set { m_SubstituteGlyphIDs = value; } }


        // =============================================
        // Private backing fields for public properties.
        // =============================================

        [SerializeField]
        private uint m_TargetGlyphID;

        [SerializeField]
        private uint[] m_SubstituteGlyphIDs;
    }

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public struct AlternateSubstitutionRecord
    {

    }

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public struct LigatureSubstitutionRecord
    {
        /// <summary>
        ///
        /// </summary>
        public uint[] componentGlyphIDs { get { return m_ComponentGlyphIDs; } set { m_ComponentGlyphIDs = value; } }

        /// <summary>
        ///
        /// </summary>
        public uint ligatureGlyphID { get { return m_LigatureGlyphID; } set { m_LigatureGlyphID = value; } }

        // =============================================
        // Private backing fields for public properties.
        // =============================================

        [SerializeField]
        private uint[] m_ComponentGlyphIDs;

        [SerializeField]
        private uint m_LigatureGlyphID;
    }
}
