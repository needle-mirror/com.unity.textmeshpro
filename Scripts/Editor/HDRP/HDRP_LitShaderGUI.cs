#if HDRP_7_5_OR_NEWER
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Rendering.HighDefinition;

// Include material common properties names
using static UnityEngine.Rendering.HighDefinition.HDMaterialProperties;

namespace TMPro.EditorUtilities
{
    /// <summary>
    /// Common GUI for Lit ShaderGraphs
    /// </summary>
    internal class HDRP_LitShaderGUI : HDShaderGUI
    {
        // For surface option shader graph we only want all unlit features but alpha clip and back then front rendering
        const SurfaceOptionUIBlock.Features   surfaceOptionFeatures = SurfaceOptionUIBlock.Features.Lit
            | SurfaceOptionUIBlock.Features.ShowDepthOffsetOnly;

        const EmissionUIBlock.Features emissionFeatures = EmissionUIBlock.Features.All ^ EmissionUIBlock.Features.EnableEmissionForGI;

        MaterialUIBlockList m_UIBlocks = new MaterialUIBlockList
        {
            new SurfaceOptionUIBlock(MaterialUIBlock.Expandable.Base, features: surfaceOptionFeatures),
            new ShaderGraphUIBlock(MaterialUIBlock.Expandable.ShaderGraph),
            //new EmissionUIBlock(MaterialUIBlock.Expandable.Emissive),
            new AdvancedOptionsUIBlock(MaterialUIBlock.Expandable.Advance, ~AdvancedOptionsUIBlock.Features.SpecularOcclusion)
        };

        protected MaterialUIBlockList uiBlocks => m_UIBlocks;

        /// <summary>
        /// Implement your custom GUI in this function. To display a UI similar to HDRP shaders, use a MaterialUIBlock.
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

        //const string kUVEmissive = "_UVEmissive";

        /// <summary>
        /// Sets up the keywords and passes for a Lit Shader Graph material.
        /// </summary>
        /// <param name="material">The target material.</param>
        public static void SetupMaterialKeywordsAndPass(Material material)
        {
            SynchronizeShaderGraphProperties(material);

            BaseLitGUI.SetupBaseLitKeywords(material);
            BaseLitGUI.SetupBaseLitMaterialPass(material);

            bool receiveSSR = false;
            if (material.GetSurfaceType() == SurfaceType.Transparent)
                receiveSSR = material.HasProperty(kReceivesSSRTransparent) ? material.GetFloat(kReceivesSSRTransparent) != 0 : false;
            else
                receiveSSR = material.HasProperty(kReceivesSSR) ? material.GetFloat(kReceivesSSR) != 0 : false;
            bool useSplitLighting = material.HasProperty(kUseSplitLighting) ? material.GetInt(kUseSplitLighting) != 0: false;
            BaseLitGUI.SetupStencil(material, receiveSSR, useSplitLighting);

            if (material.HasProperty(kAddPrecomputedVelocity))
                CoreUtils.SetKeyword(material, "_ADD_PRECOMPUTED_VELOCITY", material.GetInt(kAddPrecomputedVelocity) != 0);

            /*if (material.HasProperty(kUVEmissive) && material.HasProperty(kEmissiveColorMap))
            {
                CoreUtils.SetKeyword(material, "_EMISSIVE_MAPPING_PLANAR", ((UVEmissiveMapping)material.GetFloat(kUVEmissive)) == UVEmissiveMapping.Planar && material.GetTexture(kEmissiveColorMap));
                CoreUtils.SetKeyword(material, "_EMISSIVE_MAPPING_TRIPLANAR", ((UVEmissiveMapping)material.GetFloat(kUVEmissive)) == UVEmissiveMapping.Triplanar && material.GetTexture(kEmissiveColorMap));
                CoreUtils.SetKeyword(material, "_EMISSIVE_MAPPING_BASE", ((UVEmissiveMapping)material.GetFloat(kUVEmissive)) == UVEmissiveMapping.SameAsBase && material.GetTexture(kEmissiveColorMap));
                CoreUtils.SetKeyword(material, "_EMISSIVE_COLOR_MAP", material.GetTexture(kEmissiveColorMap));
            }*/
        }

        protected override void SetupMaterialKeywordsAndPassInternal(Material material) => SetupMaterialKeywordsAndPass(material);
    }
}
#endif
