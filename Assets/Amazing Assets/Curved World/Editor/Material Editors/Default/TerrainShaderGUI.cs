// Curved World <http://u3d.as/1W8h>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using UnityEditor;


namespace AmazingAssets.CurvedWorld.Editor
{
    internal class TerrainShaderGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            CurvedWorld.Editor.MaterialProperties.InitCurvedWorldMaterialProperties(properties);
            CurvedWorld.Editor.MaterialProperties.DrawCurvedWorldMaterialProperties(materialEditor, MaterialProperties.Style.None, false, false);
        }
    }
}
