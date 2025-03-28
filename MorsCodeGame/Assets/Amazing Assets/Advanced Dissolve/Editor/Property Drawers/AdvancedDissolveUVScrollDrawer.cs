// Advanced Dissolve <https://u3d.as/16cX>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using UnityEngine;
using UnityEditor;


namespace AmazingAssets.AdvancedDissolve.Editor
{
    class AdvancedDissolveUVScrollDrawer : MaterialPropertyDrawer
    {
        static GUIContent text = new GUIContent("Scroll");


        public override void OnGUI(Rect position, MaterialProperty prop, string label, UnityEditor.MaterialEditor editor)
        {
            position.y -= 2;

            float width = UnityEditor.EditorGUIUtility.labelWidth - EditorGUI.indentLevel * 15;


            EditorGUI.PrefixLabel(new Rect(position.x, position.y, width, 16f), text);


            Vector2 vector = prop.vectorValue;

            EditorGUI.BeginChangeCheck();
            vector = EditorGUI.Vector2Field(new Rect(position.x + width, position.y, position.width - width, 16f), GUIContent.none, vector);
            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = vector;
            }
        }
    }
}
