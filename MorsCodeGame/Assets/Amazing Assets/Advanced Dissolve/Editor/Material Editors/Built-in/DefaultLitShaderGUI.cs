// Advanced Dissolve <https://u3d.as/16cX>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using UnityEngine;
using UnityEditor;


namespace AmazingAssets.AdvancedDissolve.Editor
{
    internal class DefaultLitShaderGUI : ShaderGUI
    {
        public override void OnGUI(UnityEditor.MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            //AmazingAssets
            AmazingAssets.AdvancedDissolve.Editor.AdvancedDissolveMaterialProperties.Init(properties);

            //Curved World
            AmazingAssets.AdvancedDissolve.Editor.AdvancedDissolveMaterialProperties.DrawCurvedWorldHeader(true, UnityEngine.GUIStyle.none, materialEditor, (Material)materialEditor.target);

            if (AmazingAssets.AdvancedDissolve.Editor.AdvancedDissolveMaterialProperties.DrawDefaultOptionsHeader("Default Shader Options"))
            {
                base.OnGUI(materialEditor, properties);
            }

            //AmazingAssets
            AmazingAssets.AdvancedDissolve.Editor.AdvancedDissolveMaterialProperties.DrawDissolveOptions(true, materialEditor, false, false, true, true, false, false);
        }

        public override void ValidateMaterial(Material material)
        {
            base.ValidateMaterial(material);

            AdvancedDissolve.AdvancedDissolveKeywords.Reload(material);
        }
    }
}
