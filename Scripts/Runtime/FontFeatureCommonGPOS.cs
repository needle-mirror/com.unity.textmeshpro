using System;
using UnityEngine;


namespace TMPro
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public struct GlyphAnchorPoint
    {
        /// <summary>
        /// The x coordinate of the anchor point relative to the glyph.
        /// </summary>
        public float xCoordinate { get { return m_XCoordinate; } set { m_XCoordinate = value; } }

        /// <summary>
        /// The y coordinate of the anchor point relative to the glyph.
        /// </summary>
        public float yCoordinate { get { return m_YCoordinate; } set { m_YCoordinate = value; } }

        // =============================================
        // Private backing fields for public properties.
        // =============================================

        [SerializeField]
        private float m_XCoordinate;

        [SerializeField]
        private float m_YCoordinate;
    }

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public struct MarkPositionAdjustment
    {
        /// <summary>
        /// The x coordinate of the anchor point relative to the glyph.
        /// </summary>
        public float xPositionAdjustment { get { return m_XPositionAdjustment; } set { m_XPositionAdjustment = value; } }

        /// <summary>
        /// The y coordinate of the anchor point relative to the glyph.
        /// </summary>
        public float yPositionAdjustment { get { return m_YPositionAdjustment; } set { m_YPositionAdjustment = value; } }


        public MarkPositionAdjustment(float x, float y)
        {
            m_XPositionAdjustment = x;
            m_YPositionAdjustment = y;
        }

        // =============================================
        // Private backing fields for public properties.
        // =============================================

        [SerializeField]
        private float m_XPositionAdjustment;

        [SerializeField]
        private float m_YPositionAdjustment;
    };


    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public struct MarkToBaseAdjustmentRecord
    {
        /// <summary>
        /// The index of the base glyph
        /// </summary>
        public uint baseGlyphID { get { return m_BaseGlyphID; } set { m_BaseGlyphID = value; } }

        /// <summary>
        ///
        /// </summary>
        public GlyphAnchorPoint baseGlyphAnchorPoint { get { return m_BaseGlyphAnchorPoint; } set { m_BaseGlyphAnchorPoint = value; } }

        /// <summary>
        /// The index of the mark glyph
        /// </summary>
        public uint markGlyphID { get { return m_MarkGlyphID; } set { m_MarkGlyphID = value; } }

        /// <summary>
        ///
        /// </summary>
        public MarkPositionAdjustment markPositionAdjustment { get { return m_MarkPositionAdjustment; } set { m_MarkPositionAdjustment = value; } }

        // =============================================
        // Private backing fields for public properties.
        // =============================================

        [SerializeField]
        private uint m_BaseGlyphID;

        [SerializeField]
        private GlyphAnchorPoint m_BaseGlyphAnchorPoint;

        [SerializeField]
        private uint m_MarkGlyphID;

        [SerializeField]
        private MarkPositionAdjustment m_MarkPositionAdjustment;
    }

    [Serializable]
    public struct MarkToMarkAdjustmentRecord
    {
        /// <summary>
        /// The index of the base glyph
        /// </summary>
        public uint baseMarkGlyphID { get { return m_BaseMarkGlyphID; } set { m_BaseMarkGlyphID = value; } }

        /// <summary>
        ///
        /// </summary>
        public GlyphAnchorPoint baseMarkGlyphAnchorPoint { get { return m_BaseMarkGlyphAnchorPoint; } set { m_BaseMarkGlyphAnchorPoint = value; } }

        /// <summary>
        /// The index of the mark glyph
        /// </summary>
        public uint combiningMarkGlyphID { get { return m_CombiningMarkGlyphID; } set { m_CombiningMarkGlyphID = value; } }

        /// <summary>
        ///
        /// </summary>
        public MarkPositionAdjustment combiningMarkPositionAdjustment { get { return m_CombiningMarkPositionAdjustment; } set { m_CombiningMarkPositionAdjustment = value; } }

        // =============================================
        // Private backing fields for public properties.
        // =============================================

        [SerializeField]
        private uint m_BaseMarkGlyphID;

        [SerializeField]
        private GlyphAnchorPoint m_BaseMarkGlyphAnchorPoint;

        [SerializeField]
        private uint m_CombiningMarkGlyphID;

        [SerializeField]
        private MarkPositionAdjustment m_CombiningMarkPositionAdjustment;
    }
}
