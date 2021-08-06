using UnityEngine;
using System.Collections;


namespace TMPro
{
    public enum CaretPosition { None, Left, Right }

    /// <summary>
    /// Structure which contains the character index and position of caret relative to the character.
    /// </summary>
    public struct CaretInfo
    {
        public int index;
        public CaretPosition position;

        public CaretInfo(int index, CaretPosition position)
        {
            this.index = index;
            this.position = position;
        }
    }

    public static class TMP_TextUtilities
    {
        private static Vector3[] m_rectWorldCorners = new Vector3[4];


        // TEXT INPUT COMPONENT RELATED FUNCTIONS

        /// <summary>
        ///
        /// </summary>
        /// <param name="textComponent">A reference to the text object.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns></returns>
        //public static CaretInfo GetCursorInsertionIndex(TMP_Text textComponent, Vector3 position, Camera camera)
        //{
        //    int index = TMP_TextUtilities.FindNearestCharacter(textComponent, position, camera, false);

        //    RectTransform rectTransform = textComponent.rectTransform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

        //    TMP_CharacterInfo cInfo = textComponent.textInfo.characterInfo[index];

        //    // Get Bottom Left and Top Right position of the current character
        //    Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
        //    //Vector3 tl = rectTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
        //    Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);
        //    //Vector3 br = rectTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

        //    float insertPosition = (position.x - bl.x) / (tr.x - bl.x);

        //    if (insertPosition < 0.5f)
        //        return new CaretInfo(index, CaretPosition.Left);
        //    else
        //        return new CaretInfo(index, CaretPosition.Right);
        //}


        /// <summary>
        /// Function returning the index of the character whose origin is closest to the cursor.
        /// </summary>
        /// <param name="textComponent">A reference to the text object.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns></returns>
        public static int GetCursorIndexFromPosition(TMP_Text textComponent, Vector3 position, Camera camera)
        {
            int index = TMP_TextUtilities.FindNearestCharacter(textComponent, position, camera, false);

            RectTransform rectTransform = textComponent.rectTransform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            TMP_CharacterInfo cInfo = textComponent.textInfo.characterInfo[index];

            // Get Bottom Left and Top Right position of the current character
            Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
            Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);

            float insertPosition = (position.x - bl.x) / (tr.x - bl.x);

            if (insertPosition < 0.5f)
                return index;
            else
                return index + 1;

        }


        /// <summary>
        /// Function returning the index of the character whose origin is closest to the cursor.
        /// </summary>
        /// <param name="textComponent">A reference to the text object.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <param name="cursor">The position of the cursor insertion position relative to the position.</param>
        /// <returns></returns>
        //public static int GetCursorIndexFromPosition(TMP_Text textComponent, Vector3 position, Camera camera, out CaretPosition cursor)
        //{
        //    int index = TMP_TextUtilities.FindNearestCharacter(textComponent, position, camera, false);

        //    RectTransform rectTransform = textComponent.rectTransform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

        //    TMP_CharacterInfo cInfo = textComponent.textInfo.characterInfo[index];

        //    // Get Bottom Left and Top Right position of the current character
        //    Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
        //    Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);

        //    float insertPosition = (position.x - bl.x) / (tr.x - bl.x);

        //    if (insertPosition < 0.5f)
        //    {
        //        cursor = CaretPosition.Left;
        //        return index;
        //    }
        //    else
        //    {
        //        cursor = CaretPosition.Right;
        //        return index;
        //    }
        //}


        /// <summary>
        /// Function returning the index of the character whose origin is closest to the cursor.
        /// </summary>
        /// <param name="textComponent">A reference to the text object.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <param name="cursor">The position of the cursor insertion position relative to the position.</param>
        /// <returns></returns>
        public static int GetCursorIndexFromPosition(TMP_Text textComponent, Vector3 position, Camera camera, out CaretPosition cursor)
        {
            int line = TMP_TextUtilities.FindNearestLine(textComponent, position, camera);

            int index = FindNearestCharacterOnLine(textComponent, position, line, camera, false);

            // Special handling if line contains only one character.
            if (textComponent.textInfo.lineInfo[line].characterCount == 1)
            {
                cursor = CaretPosition.Left;
                return index;
            }

            RectTransform rectTransform = textComponent.rectTransform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            TMP_CharacterInfo cInfo = textComponent.textInfo.characterInfo[index];

            // Get Bottom Left and Top Right position of the current character
            Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
            Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);

            float insertPosition = (position.x - bl.x) / (tr.x - bl.x);

            if (insertPosition < 0.5f)
            {
                cursor = CaretPosition.Left;
                return index;
            }
            else
            {
                cursor = CaretPosition.Right;
                return index;
            }
        }


        /// <summary>
        /// Function returning the line nearest to the position.
        /// </summary>
        /// <param name="textComponent"></param>
        /// <param name="position"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static int FindNearestLine(TMP_Text text, Vector3 position, Camera camera)
        {
            RectTransform rectTransform = text.rectTransform;

            float distance = Mathf.Infinity;
            int closest = -1;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.lineCount; i++)
            {
                TMP_LineInfo lineInfo = text.textInfo.lineInfo[i];

                float ascender = rectTransform.TransformPoint(new Vector3(0, lineInfo.ascender, 0)).y;
                float descender = rectTransform.TransformPoint(new Vector3(0, lineInfo.descender, 0)).y;

                if (ascender > position.y && descender < position.y)
                {
                    //Debug.Log("Position is on line " + i);
                    return i;
                }

                float d0 = Mathf.Abs(ascender - position.y);
                float d1 = Mathf.Abs(descender - position.y);

                float d = Mathf.Min(d0, d1);
                if (d < distance)
                {
                    distance = d;
                    closest = i;
                }
            }

            //Debug.Log("Closest line to position is " + closest);
            return closest;
        }


        /// <summary>
        /// Function returning the nearest character to position on a given line.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="position"></param>
        /// <param name="line"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static int FindNearestCharacterOnLine(TMP_Text text, Vector3 position, int line, Camera camera, bool visibleOnly)
        {
            RectTransform rectTransform = text.rectTransform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            int firstCharacter = text.textInfo.lineInfo[line].firstCharacterIndex;
            int lastCharacter = text.textInfo.lineInfo[line].lastCharacterIndex;

            float distanceSqr = Mathf.Infinity;
            int closest = lastCharacter;

            for (int i = firstCharacter; i < lastCharacter; i++)
            {
                // Get current character info.
                TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
                if (visibleOnly && !cInfo.isVisible) continue;

                // Get Bottom Left and Top Right position of the current character
                Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
                Vector3 tl = rectTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
                Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);
                Vector3 br = rectTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

                if (PointIntersectRectangle(position, bl, tl, tr, br))
                {
                    closest = i;
                    break;
                }

                // Find the closest corner to position.
                float dbl = DistanceToLine(bl, tl, position);
                float dtl = DistanceToLine(tl, tr, position);
                float dtr = DistanceToLine(tr, br, position);
                float dbr = DistanceToLine(br, bl, position);

                float d = dbl < dtl ? dbl : dtl;
                d = d < dtr ? d : dtr;
                d = d < dbr ? d : dbr;

                if (distanceSqr > d)
                {
                    distanceSqr = d;
                    closest = i;
                }
            }
            return closest;
        }


        /// <summary>
        /// Function used to determine if the position intersects with the RectTransform.
        /// </summary>
        /// <param name="rectTransform">A reference to the RectTranform of the text object.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns></returns>
        public static bool IsIntersectingRectTransform(RectTransform rectTransform, Vector3 position, Camera camera)
        {
            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            rectTransform.GetWorldCorners(m_rectWorldCorners);

            if (PointIntersectRectangle(position, m_rectWorldCorners[0], m_rectWorldCorners[1], m_rectWorldCorners[2], m_rectWorldCorners[3]))
            {
                return true;
            }

            return false;
        }


        // CHARACTER HANDLING

        /// <summary>
        /// Function returning the index of the character at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which is rendering the text or whichever one might be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <param name="visibleOnly">Only check for visible characters.</param>
        /// <returns></returns>
        public static int FindIntersectingCharacter(TMP_Text text, Vector3 position, Camera camera, bool visibleOnly)
        {
            RectTransform rectTransform = text.rectTransform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.characterCount; i++)
            {
                // Get current character info.
                TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
                if (visibleOnly && !cInfo.isVisible) continue;

                // Get Bottom Left and Top Right position of the current character
                Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
                Vector3 tl = rectTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
                Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);
                Vector3 br = rectTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

                if (PointIntersectRectangle(position, bl, tl, tr, br))
                    return i;

            }
            return -1;
        }


        /// <summary>
        /// Function returning the index of the character at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The camera which is rendering the text object.</param>
        /// <param name="visibleOnly">Only check for visible characters.</param>
        /// <returns></returns>
        //public static int FindIntersectingCharacter(TextMeshPro text, Vector3 position, Camera camera, bool visibleOnly)
        //{
        //    Transform textTransform = text.transform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.characterCount; i++)
        //    {
        //        // Get current character info.
        //        TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
        //        if ((visibleOnly && !cInfo.isVisible) || (text.OverflowMode == TextOverflowModes.Page && cInfo.pageNumber + 1 != text.pageToDisplay))
        //            continue;

        //        // Get Bottom Left and Top Right position of the current character
        //        Vector3 bl = textTransform.TransformPoint(cInfo.bottomLeft);
        //        Vector3 tl = textTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
        //        Vector3 tr = textTransform.TransformPoint(cInfo.topRight);
        //        Vector3 br = textTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

        //        if (PointIntersectRectangle(position, bl, tl, tr, br))
        //            return i;

        //    }

        //    return -1;
        //}


        /// <summary>
        /// Function to find the nearest character to position.
        /// </summary>
        /// <param name="text">A reference to the TMP Text component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <param name="visibleOnly">Only check for visible characters.</param>
        /// <returns></returns>
        public static int FindNearestCharacter(TMP_Text text, Vector3 position, Camera camera, bool visibleOnly)
        {
            RectTransform rectTransform = text.rectTransform;

            float distanceSqr = Mathf.Infinity;
            int closest = 0;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.characterCount; i++)
            {
                // Get current character info.
                TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
                if (visibleOnly && !cInfo.isVisible) continue;

                // Get Bottom Left and Top Right position of the current character
                Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
                Vector3 tl = rectTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
                Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);
                Vector3 br = rectTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

                if (PointIntersectRectangle(position, bl, tl, tr, br))
                    return i;

                // Find the closest corner to position.
                float dbl = DistanceToLine(bl, tl, position);
                float dtl = DistanceToLine(tl, tr, position);
                float dtr = DistanceToLine(tr, br, position);
                float dbr = DistanceToLine(br, bl, position);

                float d = dbl < dtl ? dbl : dtl;
                d = d < dtr ? d : dtr;
                d = d < dbr ? d : dbr;

                if (distanceSqr > d)
                {
                    distanceSqr = d;
                    closest = i;
                }
            }

            return closest;
        }


        /// <summary>
        /// Function to find the nearest character to position.
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <param name="visibleOnly">Only check for visible characters.</param>
        /// <returns></returns>
        //public static int FindNearestCharacter(TextMeshProUGUI text, Vector3 position, Camera camera, bool visibleOnly)
        //{
        //    RectTransform rectTransform = text.rectTransform;

        //    float distanceSqr = Mathf.Infinity;
        //    int closest = 0;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.characterCount; i++)
        //    {
        //        // Get current character info.
        //        TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
        //        if ((visibleOnly && !cInfo.isVisible) || (text.OverflowMode == TextOverflowModes.Page && cInfo.pageNumber + 1 != text.pageToDisplay))
        //            continue;

        //        // Get Bottom Left and Top Right position of the current character
        //        Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
        //        Vector3 tl = rectTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
        //        Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);
        //        Vector3 br = rectTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

        //        if (PointIntersectRectangle(position, bl, tl, tr, br))
        //            return i;

        //        // Find the closest corner to position.
        //        float dbl = DistanceToLine(bl, tl, position);
        //        float dtl = DistanceToLine(tl, tr, position);
        //        float dtr = DistanceToLine(tr, br, position);
        //        float dbr = DistanceToLine(br, bl, position);

        //        float d = dbl < dtl ? dbl : dtl;
        //        d = d < dtr ? d : dtr;
        //        d = d < dbr ? d : dbr;

        //        if (distanceSqr > d)
        //        {
        //            distanceSqr = d;
        //            closest = i;
        //        }
        //    }

        //    //Debug.Log("Returning nearest character at index: " + closest);

        //    return closest;
        //}


        /// <summary>
        /// Function to find the nearest character to position.
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The camera which is rendering the text object.</param>
        /// <param name="visibleOnly">Only check for visible characters.</param>
        /// <returns></returns>
        //public static int FindNearestCharacter(TextMeshPro text, Vector3 position, Camera camera, bool visibleOnly)
        //{
        //    Transform textTransform = text.transform;

        //    float distanceSqr = Mathf.Infinity;
        //    int closest = 0;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.characterCount; i++)
        //    {
        //        // Get current character info.
        //        TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
        //        if ((visibleOnly && !cInfo.isVisible) || (text.OverflowMode == TextOverflowModes.Page && cInfo.pageNumber + 1 != text.pageToDisplay))
        //            continue;

        //        // Get Bottom Left and Top Right position of the current character
        //        Vector3 bl = textTransform.TransformPoint(cInfo.bottomLeft);
        //        Vector3 tl = textTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
        //        Vector3 tr = textTransform.TransformPoint(cInfo.topRight);
        //        Vector3 br = textTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

        //        if (PointIntersectRectangle(position, bl, tl, tr, br))
        //            return i;

        //        // Find the closest corner to position.
        //        float dbl = DistanceToLine(bl, tl, position); // (position - bl).sqrMagnitude;
        //        float dtl = DistanceToLine(tl, tr, position); // (position - tl).sqrMagnitude;
        //        float dtr = DistanceToLine(tr, br, position); // (position - tr).sqrMagnitude;
        //        float dbr = DistanceToLine(br, bl, position); // (position - br).sqrMagnitude;

        //        float d = dbl < dtl ? dbl : dtl;
        //        d = d < dtr ? d : dtr;
        //        d = d < dbr ? d : dbr;

        //        if (distanceSqr > d)
        //        {
        //            distanceSqr = d;
        //            closest = i;
        //        }
        //    }

        //    //Debug.Log("Returning nearest character at index: " + closest);

        //    return closest;
        //}


        // WORD HANDLING
        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TMP_Text component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns></returns>
        public static int FindIntersectingWord(TMP_Text text, Vector3 position, Camera camera)
        {
            RectTransform rectTransform = text.rectTransform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.wordCount; i++)
            {
                TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                float maxAscender = -Mathf.Infinity;
                float minDescender = Mathf.Infinity;

                // Iterate through each character of the word
                for (int j = 0; j < wInfo.characterCount; j++)
                {
                    int characterIndex = wInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    bool isCharacterVisible = currentCharInfo.isVisible;

                    // Track maximum Ascender and minimum Descender for each word.
                    maxAscender = Mathf.Max(maxAscender, currentCharInfo.ascender);
                    minDescender = Mathf.Min(minDescender, currentCharInfo.descender);

                    if (isBeginRegion == false && isCharacterVisible)
                    {
                        isBeginRegion = true;

                        bl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0);
                        tl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0);

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (wInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
                            tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

                            // Transform coordinates to be relative to transform and account min descender and max ascender.
                            bl = rectTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
                            tl = rectTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
                            tr = rectTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
                            br = rectTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == wInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
                        tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

                        // Transform coordinates to be relative to transform and account min descender and max ascender.
                        bl = rectTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
                        tl = rectTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
                        br = rectTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
                        tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

                        // Transform coordinates to be relative to transform and account min descender and max ascender.
                        bl = rectTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
                        tl = rectTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
                        br = rectTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

                        maxAscender = -Mathf.Infinity;
                        minDescender = Mathf.Infinity;

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }

                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

            }

            return -1;
        }


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns></returns>
        //public static int FindIntersectingWord(TextMeshProUGUI text, Vector3 position, Camera camera)
        //{
        //    RectTransform rectTransform = text.rectTransform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.wordCount; i++)
        //    {
        //        TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        float maxAscender = -Mathf.Infinity;
        //        float minDescender = Mathf.Infinity;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < wInfo.characterCount; j++)
        //        {
        //            int characterIndex = wInfo.firstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
        //                                      currentCharInfo.lineNumber > text.maxVisibleLines ||
        //                                     (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

        //            // Track maximum Ascender and minimum Descender for each word.
        //            maxAscender = Mathf.Max(maxAscender, currentCharInfo.ascender);
        //            minDescender = Mathf.Min(minDescender, currentCharInfo.descender);

        //            if (isBeginRegion == false && isCharacterVisible)
        //            {
        //                isBeginRegion = true;

        //                bl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0);
        //                tl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0);

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (wInfo.characterCount == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
        //                    tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

        //                    // Transform coordinates to be relative to transform and account min descender and max ascender.
        //                    bl = rectTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
        //                    tl = rectTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
        //                    tr = rectTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
        //                    br = rectTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == wInfo.characterCount - 1)
        //            {
        //                isBeginRegion = false;

        //                br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
        //                tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

        //                // Transform coordinates to be relative to transform and account min descender and max ascender.
        //                bl = rectTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
        //                tl = rectTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
        //                br = rectTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
        //                tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

        //                // Transform coordinates to be relative to transform and account min descender and max ascender.
        //                bl = rectTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
        //                tl = rectTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
        //                br = rectTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

        //                maxAscender = -Mathf.Infinity;
        //                minDescender = Mathf.Infinity;

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }

        //        //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

        //    }

        //    return -1;
        //}


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The camera which is rendering the text object.</param>
        /// <returns></returns>
        //public static int FindIntersectingWord(TextMeshPro text, Vector3 position, Camera camera)
        //{
        //    Transform textTransform = text.transform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.wordCount; i++)
        //    {
        //        TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        float maxAscender = -Mathf.Infinity;
        //        float minDescender = Mathf.Infinity;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < wInfo.characterCount; j++)
        //        {
        //            int characterIndex = wInfo.firstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
        //                                      currentCharInfo.lineNumber > text.maxVisibleLines ||
        //                                     (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

        //            // Track maximum Ascender and minimum Descender for each word.
        //            maxAscender = Mathf.Max(maxAscender, currentCharInfo.ascender);
        //            minDescender = Mathf.Min(minDescender, currentCharInfo.descender);

        //            if (isBeginRegion == false && isCharacterVisible)
        //            {
        //                isBeginRegion = true;

        //                bl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0);
        //                tl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0);

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (wInfo.characterCount == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
        //                    tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

        //                    // Transform coordinates to be relative to transform and account min descender and max ascender.
        //                    bl = textTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
        //                    tl = textTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
        //                    tr = textTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
        //                    br = textTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == wInfo.characterCount - 1)
        //            {
        //                isBeginRegion = false;

        //                br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
        //                tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

        //                // Transform coordinates to be relative to transform and account min descender and max ascender.
        //                bl = textTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
        //                tl = textTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
        //                br = textTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
        //                tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

        //                // Transform coordinates to be relative to transform and account min descender and max ascender.
        //                bl = textTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
        //                tl = textTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
        //                br = textTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

        //                // Reset maxAscender and minDescender for next word segment.
        //                maxAscender = -Mathf.Infinity;
        //                minDescender = Mathf.Infinity;

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }
        //    }

        //    return -1;
        //}


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TMP_Text component.</param>
        /// <param name="position"></param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns></returns>
        public static int FindNearestWord(TMP_Text text, Vector3 position, Camera camera)
        {
            RectTransform rectTransform = text.rectTransform;

            float distanceSqr = Mathf.Infinity;
            int closest = 0;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.wordCount; i++)
            {
                TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                // Iterate through each character of the word
                for (int j = 0; j < wInfo.characterCount; j++)
                {
                    int characterIndex = wInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    bool isCharacterVisible = currentCharInfo.isVisible;

                    if (isBeginRegion == false && isCharacterVisible)
                    {
                        isBeginRegion = true;

                        bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
                        tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (wInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                            tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            // Find the closest line segment to position.
                            float dbl = DistanceToLine(bl, tl, position);
                            float dtl = DistanceToLine(tl, tr, position);
                            float dtr = DistanceToLine(tr, br, position);
                            float dbr = DistanceToLine(br, bl, position);

                            float d = dbl < dtl ? dbl : dtl;
                            d = d < dtr ? d : dtr;
                            d = d < dbr ? d : dbr;

                            if (distanceSqr > d)
                            {
                                distanceSqr = d;
                                closest = i;
                            }
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == wInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        // Find the closest line segment to position.
                        float dbl = DistanceToLine(bl, tl, position);
                        float dtl = DistanceToLine(tl, tr, position);
                        float dtr = DistanceToLine(tr, br, position);
                        float dbr = DistanceToLine(br, bl, position);

                        float d = dbl < dtl ? dbl : dtl;
                        d = d < dtr ? d : dtr;
                        d = d < dbr ? d : dbr;

                        if (distanceSqr > d)
                        {
                            distanceSqr = d;
                            closest = i;
                        }
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        // Find the closest line segment to position.
                        float dbl = DistanceToLine(bl, tl, position);
                        float dtl = DistanceToLine(tl, tr, position);
                        float dtr = DistanceToLine(tr, br, position);
                        float dbr = DistanceToLine(br, bl, position);

                        float d = dbl < dtl ? dbl : dtl;
                        d = d < dtr ? d : dtr;
                        d = d < dbr ? d : dbr;

                        if (distanceSqr > d)
                        {
                            distanceSqr = d;
                            closest = i;
                        }
                    }
                }

                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");
            }

            return closest;
        }

        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position"></param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns></returns>
        //public static int FindNearestWord(TextMeshProUGUI text, Vector3 position, Camera camera)
        //{
        //    RectTransform rectTransform = text.rectTransform;

        //    float distanceSqr = Mathf.Infinity;
        //    int closest = 0;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.wordCount; i++)
        //    {
        //        TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < wInfo.characterCount; j++)
        //        {
        //            int characterIndex = wInfo.firstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
        //                                      currentCharInfo.lineNumber > text.maxVisibleLines ||
        //                                     (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

        //            if (isBeginRegion == false && isCharacterVisible)
        //            {
        //                isBeginRegion = true;

        //                bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
        //                tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (wInfo.characterCount == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                    tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == wInfo.characterCount - 1)
        //            {
        //                isBeginRegion = false;

        //                br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }

        //        // Find the closest line segment to position.
        //        float dbl = DistanceToLine(bl, tl, position); // (position - bl).sqrMagnitude;
        //        float dtl = DistanceToLine(tl, tr, position); // (position - tl).sqrMagnitude;
        //        float dtr = DistanceToLine(tr, br, position); // (position - tr).sqrMagnitude;
        //        float dbr = DistanceToLine(br, bl, position); // (position - br).sqrMagnitude;

        //        float d = dbl < dtl ? dbl : dtl;
        //        d = d < dtr ? d : dtr;
        //        d = d < dbr ? d : dbr;

        //        if (distanceSqr > d)
        //        {
        //            distanceSqr = d;
        //            closest = i;
        //        }
        //        //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

        //    }

        //    return closest;
        //}


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The camera which is rendering the text object.</param>
        /// <returns></returns>
        //public static int FindNearestWord(TextMeshPro text, Vector3 position, Camera camera)
        //{
        //    Transform textTransform = text.transform;

        //    float distanceSqr = Mathf.Infinity;
        //    int closest = 0;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.wordCount; i++)
        //    {
        //        TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < wInfo.characterCount; j++)
        //        {
        //            int characterIndex = wInfo.firstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
        //                                      currentCharInfo.lineNumber > text.maxVisibleLines ||
        //                                     (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

        //            if (isBeginRegion == false && isCharacterVisible)
        //            {
        //                isBeginRegion = true;

        //                bl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
        //                tl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (wInfo.characterCount == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                    tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == wInfo.characterCount - 1)
        //            {
        //                isBeginRegion = false;

        //                br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }

        //         // Find the closest line segment to position.
        //        float dbl = DistanceToLine(bl, tl, position);
        //        float dtl = DistanceToLine(tl, tr, position);
        //        float dtr = DistanceToLine(tr, br, position);
        //        float dbr = DistanceToLine(br, bl, position);

        //        float d = dbl < dtl ? dbl : dtl;
        //        d = d < dtr ? d : dtr;
        //        d = d < dbr ? d : dbr;

        //        if (distanceSqr > d)
        //        {
        //            distanceSqr = d;
        //            closest = i;
        //        }
        //        //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

        //    }

        //    return closest;

        //}


        /// <summary>
        /// Function returning the line intersecting the position.
        /// </summary>
        /// <param name="textComponent"></param>
        /// <param name="position"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static int FindIntersectingLine(TMP_Text text, Vector3 position, Camera camera)
        {
            RectTransform rectTransform = text.rectTransform;

            int closest = -1;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.lineCount; i++)
            {
                TMP_LineInfo lineInfo = text.textInfo.lineInfo[i];

                float ascender = rectTransform.TransformPoint(new Vector3(0, lineInfo.ascender, 0)).y;
                float descender = rectTransform.TransformPoint(new Vector3(0, lineInfo.descender, 0)).y;

                if (ascender > position.y && descender < position.y)
                {
                    //Debug.Log("Position is on line " + i);
                    return i;
                }
            }

            //Debug.Log("Closest line to position is " + closest);
            return closest;
        }


        /// <summary>
        /// Function returning the index of the Link at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TMP_Text component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns></returns>
        public static int FindIntersectingLink(TMP_Text text, Vector3 position, Camera camera)
        {
            Transform rectTransform = text.transform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.linkCount; i++)
            {
                TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                // Iterate through each character of the word
                for (int j = 0; j < linkInfo.linkTextLength; j++)
                {
                    int characterIndex = linkInfo.linkTextfirstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    // Check if Link characters are on the current page
                    if (text.overflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) continue;

                    if (isBeginRegion == false)
                    {
                        isBeginRegion = true;

                        bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
                        tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (linkInfo.linkTextLength == 1)
                        {
                            isBeginRegion = false;

                            br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                            tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == linkInfo.linkTextLength - 1)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }

                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

            }

            return -1;
        }

        /// <summary>
        /// Function returning the index of the Link at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns></returns>
        //public static int FindIntersectingLink(TextMeshProUGUI text, Vector3 position, Camera camera)
        //{
        //    Transform rectTransform = text.transform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.linkCount; i++)
        //    {
        //        TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < linkInfo.linkTextLength; j++)
        //        {
        //            int characterIndex = linkInfo.linkTextfirstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            // Check if Link characters are on the current page
        //            if (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) continue;

        //            if (isBeginRegion == false)
        //            {
        //                isBeginRegion = true;

        //                bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
        //                tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (linkInfo.linkTextLength == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                    tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == linkInfo.linkTextLength - 1)
        //            {
        //                isBeginRegion = false;

        //                br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }

        //        //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

        //    }

        //    return -1;
        //}


        /// <summary>
        /// Function returning the index of the Link at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The camera which is rendering the text object.</param>
        /// <returns></returns>
        //public static int FindIntersectingLink(TextMeshPro text, Vector3 position, Camera camera)
        //{
        //    Transform textTransform = text.transform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.linkCount; i++)
        //    {
        //        TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < linkInfo.linkTextLength; j++)
        //        {
        //            int characterIndex = linkInfo.linkTextfirstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            // Check if Link characters are on the current page
        //            if (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) continue;

        //            if (isBeginRegion == false)
        //            {
        //                isBeginRegion = true;

        //                bl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
        //                tl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (linkInfo.linkTextLength == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                    tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == linkInfo.linkTextLength - 1)
        //            {
        //                isBeginRegion = false;

        //                br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }

        //        //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

        //    }

        //    return -1;
        //}


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TMP_Text component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns></returns>
        public static int FindNearestLink(TMP_Text text, Vector3 position, Camera camera)
        {
            RectTransform rectTransform = text.rectTransform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            float distanceSqr = Mathf.Infinity;
            int closest = 0;

            for (int i = 0; i < text.textInfo.linkCount; i++)
            {
                TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                // Iterate through each character of the link
                for (int j = 0; j < linkInfo.linkTextLength; j++)
                {
                    int characterIndex = linkInfo.linkTextfirstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    // Check if Link characters are on the current page
                    if (text.overflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) continue;

                    if (isBeginRegion == false)
                    {
                        isBeginRegion = true;

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
                        tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

                        // If Link is one character
                        if (linkInfo.linkTextLength == 1)
                        {
                            isBeginRegion = false;

                            br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                            tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            // Find the closest line segment to position.
                            float dbl = DistanceToLine(bl, tl, position);
                            float dtl = DistanceToLine(tl, tr, position);
                            float dtr = DistanceToLine(tr, br, position);
                            float dbr = DistanceToLine(br, bl, position);

                            float d = dbl < dtl ? dbl : dtl;
                            d = d < dtr ? d : dtr;
                            d = d < dbr ? d : dbr;

                            if (distanceSqr > d)
                            {
                                distanceSqr = d;
                                closest = i;
                            }

                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == linkInfo.linkTextLength - 1)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        // Find the closest line segment to position.
                        float dbl = DistanceToLine(bl, tl, position);
                        float dtl = DistanceToLine(tl, tr, position);
                        float dtr = DistanceToLine(tr, br, position);
                        float dbr = DistanceToLine(br, bl, position);

                        float d = dbl < dtl ? dbl : dtl;
                        d = d < dtr ? d : dtr;
                        d = d < dbr ? d : dbr;

                        if (distanceSqr > d)
                        {
                            distanceSqr = d;
                            closest = i;
                        }

                    }
                    // If Link is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        // Find the closest line segment to position.
                        float dbl = DistanceToLine(bl, tl, position);
                        float dtl = DistanceToLine(tl, tr, position);
                        float dtr = DistanceToLine(tr, br, position);
                        float dbr = DistanceToLine(br, bl, position);

                        float d = dbl < dtl ? dbl : dtl;
                        d = d < dtr ? d : dtr;
                        d = d < dbr ? d : dbr;

                        if (distanceSqr > d)
                        {
                            distanceSqr = d;
                            closest = i;
                        }
                    }
                }

                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

            }

            return closest;
        }


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns></returns>
        //public static int FindNearestLink(TextMeshProUGUI text, Vector3 position, Camera camera)
        //{
        //    RectTransform rectTransform = text.rectTransform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

        //    float distanceSqr = Mathf.Infinity;
        //    int closest = 0;

        //    for (int i = 0; i < text.textInfo.linkCount; i++)
        //    {
        //        TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < linkInfo.linkTextLength; j++)
        //        {
        //            int characterIndex = linkInfo.linkTextfirstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            if (isBeginRegion == false)
        //            {
        //                isBeginRegion = true;

        //                bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
        //                tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (linkInfo.linkTextLength == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                    tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == linkInfo.linkTextLength - 1)
        //            {
        //                isBeginRegion = false;

        //                br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }

        //        // Find the closest line segment to position.
        //        float dbl = DistanceToLine(bl, tl, position); // (position - bl).sqrMagnitude;
        //        float dtl = DistanceToLine(tl, tr, position); // (position - tl).sqrMagnitude;
        //        float dtr = DistanceToLine(tr, br, position); // (position - tr).sqrMagnitude;
        //        float dbr = DistanceToLine(br, bl, position); // (position - br).sqrMagnitude;

        //        float d = dbl < dtl ? dbl : dtl;
        //        d = d < dtr ? d : dtr;
        //        d = d < dbr ? d : dbr;

        //        if (distanceSqr > d)
        //        {
        //            distanceSqr = d;
        //            closest = i;
        //        }
        //        //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

        //    }

        //    return closest;
        //}


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The camera which is rendering the text object.</param>
        /// <returns></returns>
        //public static int FindNearestLink(TextMeshPro text, Vector3 position, Camera camera)
        //{
        //    Transform textTransform = text.transform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

        //    float distanceSqr = Mathf.Infinity;
        //    int closest = 0;

        //    for (int i = 0; i < text.textInfo.linkCount; i++)
        //    {
        //        TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < linkInfo.linkTextLength; j++)
        //        {
        //            int characterIndex = linkInfo.linkTextfirstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            if (isBeginRegion == false)
        //            {
        //                isBeginRegion = true;

        //                bl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
        //                tl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (linkInfo.linkTextLength == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                    tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == linkInfo.linkTextLength - 1)
        //            {
        //                isBeginRegion = false;

        //                br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }

        //        // Find the closest line segment to position.
        //        float dbl = DistanceToLine(bl, tl, position);
        //        float dtl = DistanceToLine(tl, tr, position);
        //        float dtr = DistanceToLine(tr, br, position);
        //        float dbr = DistanceToLine(br, bl, position);

        //        float d = dbl < dtl ? dbl : dtl;
        //        d = d < dtr ? d : dtr;
        //        d = d < dbr ? d : dbr;

        //        if (distanceSqr > d)
        //        {
        //            distanceSqr = d;
        //            closest = i;
        //        }
        //        //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

        //    }
        //    return closest;
        //}



        /// <summary>
        /// Function to check if a Point is contained within a Rectangle.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static bool PointIntersectRectangle(Vector3 m, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            Vector3 ab = b - a;
            Vector3 am = m - a;
            Vector3 bc = c - b;
            Vector3 bm = m - b;

            float abamDot = Vector3.Dot(ab, am);
            float bcbmDot = Vector3.Dot(bc, bm);

            return 0 <= abamDot && abamDot <= Vector3.Dot(ab, ab) && 0 <= bcbmDot && bcbmDot <= Vector3.Dot(bc, bc);
        }


        /// <summary>
        /// Method to convert ScreenPoint to WorldPoint aligned with Rectangle
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="screenPoint"></param>
        /// <param name="cam"></param>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public static bool ScreenPointToWorldPointInRectangle(Transform transform, Vector2 screenPoint, Camera cam, out Vector3 worldPoint)
        {
            worldPoint = (Vector3)Vector2.zero;
            Ray ray = RectTransformUtility.ScreenPointToRay(cam, screenPoint);

            float enter;
            if (!new Plane(transform.rotation * Vector3.back, transform.position).Raycast(ray, out enter))
                return false;

            worldPoint = ray.GetPoint(enter);

            return true;
        }


        private struct LineSegment
        {
            public Vector3 Point1;
            public Vector3 Point2;

            public LineSegment(Vector3 p1, Vector3 p2)
            {
                Point1 = p1;
                Point2 = p2;
            }
        }


        /// <summary>
        /// Function returning the point of intersection between a line and a plane.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <param name="normal"></param>
        /// <param name="intersectingPoint"></param>
        /// <returns></returns>
        private static bool IntersectLinePlane(LineSegment line, Vector3 point, Vector3 normal, out Vector3 intersectingPoint)
        {
            intersectingPoint = Vector3.zero;
            Vector3 u = line.Point2 - line.Point1;
            Vector3 w = line.Point1 - point;

            float D = Vector3.Dot(normal, u);
            float N = -Vector3.Dot(normal, w);

            if (Mathf.Abs(D) < Mathf.Epsilon)   // if line is parallel & co-planar to plane
            {
                if (N == 0)
                    return true;
                else
                    return false;
            }

            float sI = N / D;

            if (sI < 0 || sI > 1) // Line parallel to plane
                return false;

            intersectingPoint = line.Point1 + sI * u;

            return true;
        }


        /// <summary>
        /// Function returning the Square Distance from a Point to a Line.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float DistanceToLine(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 n = b - a;
            Vector3 pa = a - point;

            float c = Vector3.Dot( n, pa );

            // Closest point is a
            if ( c > 0.0f )
                return Vector3.Dot( pa, pa );

            Vector3 bp = point - b;

            // Closest point is b
            if (Vector3.Dot( n, bp ) > 0.0f )
                return Vector3.Dot( bp, bp );

            // Closest point is between a and b
            Vector3 e = pa - n * (c / Vector3.Dot( n, n ));

            return Vector3.Dot( e, e );
        }


        /// <summary>
        /// Function returning the Square Distance from a Point to a Line and Direction.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="point"></param>
        /// <param name="direction">-1 left, 0 in between, 1 right</param>
        /// <returns></returns>
        //public static float DistanceToLineDirectional(Vector3 a, Vector3 b, Vector3 point, ref int direction)
        //{
        //    Vector3 n = b - a;
        //    Vector3 pa = a - point;

        //    float c = Vector3.Dot(n, pa);
        //    direction = -1;

        //    // Closest point is a
        //    if (c > 0.0f)
        //        return Vector3.Dot(pa, pa);

        //    Vector3 bp = point - b;
        //    direction = 1;

        //    // Closest point is b
        //    if (Vector3.Dot(n, bp) > 0.0f)
        //        return Vector3.Dot(bp, bp);

        //    // Closest point is between a and b
        //    Vector3 e = pa - n * (c / Vector3.Dot(n, n));

        //    direction = 0;
        //    return Vector3.Dot(e, e);
        //}


        /// <summary>
        /// Table used to convert character to lowercase.
        /// </summary>
        const string k_lookupStringL = "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@abcdefghijklmnopqrstuvwxyz[-]^_`abcdefghijklmnopqrstuvwxyz{|}~-";

        /// <summary>
        /// Table used to convert character to uppercase.
        /// </summary>
        const string k_lookupStringU = "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-";


        /// <summary>
        /// Get lowercase version of this ASCII character.
        /// </summary>
        public static char ToLowerFast(char c)
        {
            if (c > k_lookupStringL.Length - 1)
                return c;

            return k_lookupStringL[c];
        }

        /// <summary>
        /// Get uppercase version of this ASCII character.
        /// </summary>
        public static char ToUpperFast(char c)
        {
            if (c > k_lookupStringU.Length - 1)
                return c;

            return k_lookupStringU[c];
        }

        /// <summary>
        /// Get uppercase version of this ASCII character.
        /// </summary>
        internal static uint ToUpperASCIIFast(uint c)
        {
            if (c > k_lookupStringU.Length - 1)
                return c;

            return k_lookupStringU[(int)c];
        }

        /// <summary>
        /// Returns the case insensitive hashcode for the given string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int GetHashCode(string s)
        {
            if (string.IsNullOrEmpty(s))
                return 0;

            int hashCode = 0;

            for (int i = 0; i < s.Length; i++)
                hashCode = ((hashCode << 5) + hashCode) ^ ToUpperFast(s[i]);

            return hashCode;
        }

        /// <summary>
        /// Function which returns a simple hashcode from a string.
        /// </summary>
        /// <returns></returns>
        public static int GetSimpleHashCode(string s)
        {
            int hashCode = 0;

            for (int i = 0; i < s.Length; i++)
                hashCode = ((hashCode << 5) + hashCode) ^ s[i];

            return hashCode;
        }

        /// <summary>
        /// Function which returns a simple hashcode from a string converted to lowercase.
        /// </summary>
        /// <returns></returns>
        public static uint GetSimpleHashCodeLowercase(string s)
        {
            uint hashCode = 5381;

            for (int i = 0; i < s.Length; i++)
                hashCode = (hashCode << 5) + hashCode ^ ToLowerFast(s[i]);

            return hashCode;
        }

        /// <summary>
        /// Function which returns a simple hash code from a string converted to uppercase.
        /// </summary>
        /// <param name="s">The string from which to compute the hash code.</param>
        /// <returns>The computed hash code.</returns>
        public static uint GetHashCodeCaseInSensitive(string s)
        {
            uint hashCode = 0;

            for (int i = 0; i < s.Length; i++)
                hashCode = (hashCode << 5) + hashCode ^ ToUpperFast(s[i]);

            return hashCode;
        }


        /// <summary>
        /// Function to convert Hex to Int
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static int HexToInt(char hex)
        {
            switch (hex)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'A': return 10;
                case 'B': return 11;
                case 'C': return 12;
                case 'D': return 13;
                case 'E': return 14;
                case 'F': return 15;
                case 'a': return 10;
                case 'b': return 11;
                case 'c': return 12;
                case 'd': return 13;
                case 'e': return 14;
                case 'f': return 15;
            }
            return 15;
        }


        /// <summary>
        /// Function to convert a properly formatted string which contains an hex value to its decimal value.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int StringHexToInt(string s)
        {
            int value = 0;

            for (int i = 0; i < s.Length; i++)
            {
                value += HexToInt(s[i]) * (int)Mathf.Pow(16, (s.Length - 1) - i);
            }

            return value;
        }

        static readonly uint[] crc32Table = {
        0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419, 0x706af48f, 0xe963a535, 0x9e6495a3,
        0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91,
        0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de, 0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7,
        0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9, 0xfa0f3d63, 0x8d080df5,
        0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
        0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59,
        0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599, 0xb8bda50f,
        0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924, 0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d,
        0x76dc4190, 0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f, 0x9fbfe4a5, 0xe8b8d433,
        0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
        0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457,
        0x65b0d9c6, 0x12b7e950, 0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65,
        0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2, 0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb,
        0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9,
        0x5005713c, 0x270241aa, 0xbe0b1010, 0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
        0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17, 0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad,
        0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683,
        0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8, 0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1,
        0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb, 0x196c3671, 0x6e6b06e7,
        0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
        0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
        0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef, 0x4669be79,
        0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236, 0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f,
        0xc5ba3bbe, 0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d,
        0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
        0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21,
        0x86d3d2d4, 0xf1d4e242, 0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777,
        0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c, 0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45,
        0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db,
        0xaed16a4a, 0xd9d65adc, 0x40df0b66, 0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
        0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605, 0xcdd70693, 0x54de5729, 0x23d967bf,
        0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d };

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        internal static uint CRCBegin()
        {
            return 0xffffffff;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="crc"></param>
        /// <returns></returns>
        internal static uint CRCDone(uint crc)
        {
            return crc ^ 0xffffffff;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="crc"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static uint CRCFeed(uint crc, int value)
        {
            return (crc >> 8) ^ crc32Table[(crc & 0xFF) ^ value];
        }

        private static byte[] m_bytes = new byte[4];

        internal static uint CRCFeed(uint crc, float value)
        {
            unsafe
            {
                int val = *(int*)&value;

                fixed (byte* b = m_bytes)
                {
                    *((int*)b) = val;
                }

                for (int i = 0; i < 4; i++)
                {
                    crc = (crc >> 8) ^ crc32Table[(crc & 0xFF) ^ m_bytes[i]];
                }

                return crc;
            }
        }

        internal static uint CRCFeed(uint crc, Color value)
        {
            crc = CRCFeed(crc, value.r);
            crc = CRCFeed(crc, value.g);
            crc = CRCFeed(crc, value.b);
            crc = CRCFeed(crc, value.a);

            return crc;
        }
    }
}
