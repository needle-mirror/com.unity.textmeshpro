using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TMPro
{
    public class TMP_TextParsingUtilities
    {
        private static readonly TMP_TextParsingUtilities s_Instance = new TMP_TextParsingUtilities();

        /// <summary>
        /// Default constructor
        /// </summary>
        static TMP_TextParsingUtilities() { }


        /// <summary>
        /// Get a singleton instance of the TextModuleUtilities.
        /// </summary>
        public static TMP_TextParsingUtilities instance
        {
            get { return s_Instance; }
        }


        /// <summary>
        /// Function returning the hashcode value of a given string.
        /// </summary>
        public static int GetHashCode(string s)
        {
            int hashCode = 0;

            for (int i = 0; i < s.Length; i++)
                hashCode = ((hashCode << 5) + hashCode) ^ ToUpperASCIIFast(s[i]);

            return hashCode;
        }

        public static int GetHashCodeCaseSensitive(string s)
        {
            int hashCode = 0;

            for (int i = 0; i < s.Length; i++)
                hashCode = ((hashCode << 5) + hashCode) ^ s[i];

            return hashCode;
        }


        /// <summary>
        /// Table used to convert character to lowercase.
        /// </summary>
        const string k_LookupStringL = "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@abcdefghijklmnopqrstuvwxyz[-]^_`abcdefghijklmnopqrstuvwxyz{|}~-";

        /// <summary>
        /// Table used to convert character to uppercase.
        /// </summary>
        const string k_LookupStringU = "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-";


        /// <summary>
        /// Get lowercase version of this ASCII character.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToLowerASCIIFast(char c)
        {
            if (c > k_LookupStringL.Length - 1)
                return c;

            return k_LookupStringL[c];
        }


        /// <summary>
        /// Get uppercase version of this ASCII character.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToUpperASCIIFast(char c)
        {
            if (c > k_LookupStringU.Length - 1)
                return c;

            return k_LookupStringU[c];
        }


        /// <summary>
        /// Get uppercase version of this ASCII character.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToUpperASCIIFast(uint c)
        {
            if (c > k_LookupStringU.Length - 1)
                return c;

            return k_LookupStringU[(int)c];
        }


        /// <summary>
        /// Get lowercase version of this ASCII character.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToLowerASCIIFast(uint c)
        {
            if (c > k_LookupStringL.Length - 1)
                return c;

            return k_LookupStringL[(int)c];
        }


        /// <summary>
        /// Check if Unicode is High Surrogate
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHighSurrogate(uint c)
        {
            return c > 0xD800 && c < 0xDBFF;
        }

        /// <summary>
        /// Check if Unicode is Low Surrogate
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLowSurrogate(uint c)
        {
            return c > 0xDC00 && c < 0xDFFF;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="highSurrogate"></param>
        /// <param name="lowSurrogate"></param>
        /// <returns></returns>
        internal static uint ConvertToUTF32(uint highSurrogate, uint lowSurrogate)
        {
            return ((highSurrogate - CodePoint.HIGH_SURROGATE_START) * 0x400) + ((lowSurrogate - CodePoint.LOW_SURROGATE_START) + CodePoint.UNICODE_PLANE01_START);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        internal static bool IsDiacriticalMark(uint c)
        {
            return c >= 0x300 && c <= 0x36F || c >= 0x1AB0 && c <= 0x1AFF || c >= 0x1DC0 && c <= 0x1DFF || c >= 0x20D0 && c <= 0x20FF || c >= 0xFE20 && c <= 0xFE2F;
        }

        internal static bool IsBaseGlyph(uint c)
        {
            return !(c >= 0x300 && c <= 0x36F || c >= 0x1AB0 && c <= 0x1AFF || c >= 0x1DC0 && c <= 0x1DFF || c >= 0x20D0 && c <= 0x20FF || c >= 0xFE20 && c <= 0xFE2F ||
                // Thai
                c == 0xE31 || c >= 0xE34 && c <= 0xE3A || c >= 0xE47 && c <= 0xE4E ||
                // Hebrew
                c >= 0x591 && c <= 0x5BD || c == 0x5BF || c >= 0x5C1 && c <= 0x5C2 || c >= 0x5C4 && c <= 0x5C5 || c == 0x5C7
                );
        }

    }
}
