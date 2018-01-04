using UnityEngine;
using System.Collections;

namespace TMPro
{
    [System.Serializable]
    public class TMP_ColorGradient : ScriptableObject
    {
        public Color topLeft;
        public Color topRight;
        public Color bottomLeft;
        public Color bottomRight;

        private static Color k_defaultColor = Color.white;

        /// <summary>
        /// Default Constructor which sets each of the colors as white.
        /// </summary>
        public TMP_ColorGradient()
        {
            this.topLeft = k_defaultColor;
            this.topRight = k_defaultColor;
            this.bottomLeft = k_defaultColor;
            this.bottomRight = k_defaultColor;
        }

        /// <summary>
        /// Constructor allowing to set the default color of the Color Gradient.
        /// </summary>
        /// <param name="color"></param>
        public TMP_ColorGradient(Color color)
        {
            this.topLeft = color;
            this.topRight = color;
            this.bottomLeft = color;
            this.bottomRight = color;
        }

        /// <summary>
        /// The vertex colors at the corners of the characters.
        /// </summary>
        /// <param name="color0">Top left color.</param>
        /// <param name="color1">Top right color.</param>
        /// <param name="color2">Bottom left color.</param>
        /// <param name="color3">Bottom right color.</param>
        public TMP_ColorGradient(Color color0, Color color1, Color color2, Color color3)
        {
            this.topLeft = color0;
            this.topRight = color1;
            this.bottomLeft = color2;
            this.bottomRight = color3;
        }
    }
}
