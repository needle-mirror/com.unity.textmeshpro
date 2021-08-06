#if HDRP_7_5_OR_NEWER
using UnityEditor;
using UnityEngine;
using UnityEditor.Rendering.HighDefinition;


namespace TMPro.EditorUtilities
{
    /// <summary>
    /// Common GUI for Lit ShaderGraphs
    /// </summary>
    internal class HDRP_UnlitShaderGUI : HDShaderGUI
    {
        const SurfaceOptionUIBlock.Features surfaceOptionFeatures = SurfaceOptionUIBlock.Features.Unlit;

        MaterialUIBlockList m_UIBlocks = new MaterialUIBlockList
        {
            new SurfaceOptionUIBlock(MaterialUIBlock.Expandable.Base, features: surfaceOptionFeatures),
            new ShaderGraphUIBlock(MaterialUIBlock.Expandable.ShaderGraph, ShaderGraphUIBlock.Features.Unlit),
            new AdvancedOptionsUIBlock(MaterialUIBlock.Expandable.Advance, ~AdvancedOptionsUIBlock.Features.SpecularOcclusion)
        };

        /// <summary>List of UI Blocks used to render the material inspector.</summary>
        protected MaterialUIBlockList uiBlocks => m_UIBlocks;

        /// <summary>
        /// Implement your custom GUI in this function. To display a UI similar to HDRP shaders, use a MaterialUIBlockList.
        /// </summary>
        /// <param name="materialEditor">The current material editor.</param>
        /// <param name="props">The list of properties the material has.</param>
        protected override void OnMaterialGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            using (var changed = new EditorGUI.ChangeCheckScope())
            {
                m_UIBlocks.OnGUI(materialEditor, props);
                ApplyKeywordsAndPassesIfNeeded(changed.changed, m_UIBlocks.materials);
            }
        }

        /// <summary>
        /// Sets up the keywords and passes for the Unlit Shader Graph material you pass in.
        /// </summary>
        /// <param name="material">The target material.</param>
        public static void SetupUnlitKeywordsAndPass(Material material)
        {
            SynchronizeShaderGraphProperties(material);
            UnlitGUI.SetupUnlitMaterialKeywordsAndPass(material);
        }

        /// <summary>
        /// Sets up the keywords and passes for the current selected material.
        /// </summary>
        /// <param name="material">The selected material.</param>
        protected override void SetupMaterialKeywordsAndPassInternal(Material material) => SetupUnlitKeywordsAndPass(material);
    }
}
#endif
