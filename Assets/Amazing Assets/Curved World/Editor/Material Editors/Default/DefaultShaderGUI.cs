// Curved World <http://u3d.as/1W8h>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using UnityEditor;


namespace AmazingAssets.CurvedWorld.Editor
{
    internal class DefaultShaderGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            CurvedWorld.Editor.MaterialProperties.InitCurvedWorldMaterialProperties(properties);
            CurvedWorld.Editor.MaterialProperties.DrawCurvedWorldMaterialProperties(materialEditor, MaterialProperties.Style.HelpBox, false, false);
            
            base.OnGUI(materialEditor, properties);
        }
    }
}
