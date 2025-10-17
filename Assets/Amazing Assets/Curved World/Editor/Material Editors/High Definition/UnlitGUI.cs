// Curved World <http://u3d.as/1W8h>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;


namespace UnityEditor.Rendering.HighDefinition
{
    /// <summary>
    /// GUI for HDRP unlit shaders (does not include shader graphs)
    /// </summary>
    class CurvedWorldUnlitGUI : HDShaderGUI
    {
        MaterialUIBlockList uiBlocks = new MaterialUIBlockList
        {
            new CurvedWorldUIBlock(MaterialUIBlock.ExpandableBit.Other),
            new SurfaceOptionUIBlock(MaterialUIBlock.ExpandableBit.Base, features: SurfaceOptionUIBlock.Features.Unlit),
            new UnlitSurfaceInputsUIBlock(MaterialUIBlock.ExpandableBit.Input),
            new TransparencyUIBlock(MaterialUIBlock.ExpandableBit.Transparency),
            new EmissionUIBlock(MaterialUIBlock.ExpandableBit.Emissive),
            new AdvancedOptionsUIBlock(MaterialUIBlock.ExpandableBit.Advance, AdvancedOptionsUIBlock.Features.Instancing | AdvancedOptionsUIBlock.Features.AddPrecomputedVelocity)
        };

        protected override void OnMaterialGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            uiBlocks.OnGUI(materialEditor, props);
        }

        public override void ValidateMaterial(Material material) => UnlitAPI.ValidateMaterial(material);
    }
} 
