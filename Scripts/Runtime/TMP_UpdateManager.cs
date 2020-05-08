using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using System.Collections.Generic;


namespace TMPro
{

    public class TMP_UpdateManager
    {
        private static TMP_UpdateManager s_Instance;

        private readonly HashSet<TMP_Text> m_LayoutRebuildQueue = new HashSet<TMP_Text>();
        private readonly HashSet<TMP_Text> m_GraphicRebuildQueue = new HashSet<TMP_Text>();
        private readonly HashSet<TMP_Text> m_InternalUpdateQueue = new HashSet<TMP_Text>();


        /// <summary>
        /// Get a singleton instance of the registry
        /// </summary>
        static TMP_UpdateManager instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new TMP_UpdateManager();

                return s_Instance;
            }
        }

        /// <summary>
        /// Register to receive rendering callbacks.
        /// </summary>
        TMP_UpdateManager()
        {
            Canvas.willRenderCanvases += DoRebuilds;
        }

        /// <summary>
        /// Function used as a replacement for LateUpdate() to handle SDF Scale updates and Legacy Animation updates.
        /// </summary>
        /// <param name="textObject"></param>
        internal static void RegisterTextObjectForUpdate(TMP_Text textObject)
        {
            Profiler.BeginSample("TMP.RegisterTextObjectForUpdate");

            instance.InternalRegisterTextObjectForUpdate(textObject);

            Profiler.EndSample();
        }

        private void InternalRegisterTextObjectForUpdate(TMP_Text textObject)
        {
            if (m_InternalUpdateQueue.Contains(textObject))
                return;

            m_InternalUpdateQueue.Add(textObject);
        }

        /// <summary>
        /// Function to register elements which require a layout rebuild.
        /// </summary>
        /// <param name="element"></param>
        public static void RegisterTextElementForLayoutRebuild(TMP_Text element)
        {
            instance.InternalRegisterTextElementForLayoutRebuild(element);
        }

        private void InternalRegisterTextElementForLayoutRebuild(TMP_Text element)
        {
            if (m_LayoutRebuildQueue.Contains(element))
                return;

            m_LayoutRebuildQueue.Add(element);
        }

        /// <summary>
        /// Function to register elements which require a layout rebuild.
        /// </summary>
        /// <param name="element"></param>
        public static void RegisterTextElementForGraphicRebuild(TMP_Text element)
        {
            Profiler.BeginSample("TMP.RegisterTextElementForGraphicRebuild");

            instance.InternalRegisterTextElementForGraphicRebuild(element);

            Profiler.EndSample();
        }

        private void InternalRegisterTextElementForGraphicRebuild(TMP_Text element)
        {
            if (m_GraphicRebuildQueue.Contains(element))
                return;

            m_GraphicRebuildQueue.Add(element);
        }

        /// <summary>
        /// Callback which occurs just before the cam is rendered.
        /// </summary>
        void OnCameraPreCull()
        {
            DoRebuilds();
        }

        /// <summary>
        /// Process the rebuild requests in the rebuild queues.
        /// </summary>
        void DoRebuilds()
        {
            Profiler.BeginSample("TMP.DoRebuilds");

            // Handle text objects that require an update either as a result of scale changes or legacy animation.
            foreach (var textObject in m_InternalUpdateQueue)
                textObject.InternalUpdate();

            // Handle Layout Rebuild Phase
            foreach (var textObject in m_LayoutRebuildQueue)
                textObject.Rebuild(CanvasUpdate.Prelayout);

            if (m_LayoutRebuildQueue.Count > 0)
                m_LayoutRebuildQueue.Clear();

            // Handle Graphic Rebuild Phase
            foreach (var textObject in m_GraphicRebuildQueue)
                textObject.Rebuild(CanvasUpdate.PreRender);

            // If there are no objects in the queue, we don't need to clear the lists again.
            if (m_GraphicRebuildQueue.Count > 0)
                m_GraphicRebuildQueue.Clear();

            Profiler.EndSample();
        }

        internal static void UnRegisterTextObjectForUpdate(TMP_Text textObject)
        {
            Profiler.BeginSample("TMP.UnRegisterTextObjectForUpdate");

            instance.InternalUnRegisterTextObjectForUpdate(textObject);

            Profiler.EndSample();
        }

        /// <summary>
        /// Function to unregister elements which no longer require a rebuild.
        /// </summary>
        /// <param name="element"></param>
        public static void UnRegisterTextElementForRebuild(TMP_Text element)
        {
            instance.InternalUnRegisterTextElementForGraphicRebuild(element);
            instance.InternalUnRegisterTextElementForLayoutRebuild(element);
            instance.InternalUnRegisterTextObjectForUpdate(element);
        }

        private void InternalUnRegisterTextElementForGraphicRebuild(TMP_Text element)
        {
            Profiler.BeginSample("TMP.InternalUnRegisterTextElementForGraphicRebuild");

            m_GraphicRebuildQueue.Remove(element);

            Profiler.EndSample();
        }

        private void InternalUnRegisterTextElementForLayoutRebuild(TMP_Text element)
        {
            m_LayoutRebuildQueue.Remove(element);
        }

        private void InternalUnRegisterTextObjectForUpdate(TMP_Text textObject)
        {
            m_InternalUpdateQueue.Remove(textObject);
        }
    }
}
