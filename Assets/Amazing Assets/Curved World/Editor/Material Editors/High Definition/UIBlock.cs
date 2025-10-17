// Curved World <http://u3d.as/1W8h>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using UnityEngine;


namespace UnityEditor.Rendering.HighDefinition
{
    public class CurvedWorldUIBlock : MaterialUIBlock
    {
        internal class Styles
        {
            public static GUIContent header { get; } = EditorGUIUtility.TrTextContent("Curved World");
        }

        MaterialProperty _CurvedWorldBendSettings = null;


        public CurvedWorldUIBlock(ExpandableBit expandableBit)
            : base(expandableBit, Styles.header)
        {

        }

        public override void LoadMaterialProperties()
        {
            _CurvedWorldBendSettings = FindProperty("_CurvedWorldBendSettings");
        }

        protected override void OnGUIOpen()
        {
            materialEditor.ShaderProperty(_CurvedWorldBendSettings, " ");
        }
    }
}
