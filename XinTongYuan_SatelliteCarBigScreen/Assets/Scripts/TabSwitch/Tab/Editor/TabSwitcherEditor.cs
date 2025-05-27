using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(TabSwitcher))]
public class TabSwitcherEditor : Editor
{
    SerializedProperty allTabTypesProp;
    SerializedProperty tabPageGroupsProp;

    void OnEnable()
    {
        allTabTypesProp = serializedObject.FindProperty("allTabTypes");
        tabPageGroupsProp = serializedObject.FindProperty("tabPageGroups");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 1. 先绘制allTabTypes（可动态增删）
        EditorGUILayout.PropertyField(allTabTypesProp, new GUIContent("全部Tab类型名（类似enum，可增删）"), true);

        // 2. 其它字段
        EditorGUILayout.PropertyField(serializedObject.FindProperty("tabButtons"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("currentTabIndex"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("initTabPages"));

        // 3. 自定义绘制tabPageGroups
        EditorGUILayout.LabelField("每个Tab下挂载的页面组", EditorStyles.boldLabel);

        for (int i = 0; i < tabPageGroupsProp.arraySize; i++)
        {
            var groupProp = tabPageGroupsProp.GetArrayElementAtIndex(i);
            var pagesProp = groupProp.FindPropertyRelative("pages");
            var tabTypeProp = groupProp.FindPropertyRelative("tabType");

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Tab Page Group " + (i + 1), EditorStyles.boldLabel);

            // Tab类型下拉
            List<string> tabTypes = new List<string>();
            for (int j = 0; j < allTabTypesProp.arraySize; j++)
            {
                tabTypes.Add(allTabTypesProp.GetArrayElementAtIndex(j).stringValue);
            }

            int selectedIdx = Mathf.Max(0, tabTypes.IndexOf(tabTypeProp.stringValue));
            int newIdx = EditorGUILayout.Popup("Tab类型名", selectedIdx, tabTypes.ToArray());
            if (tabTypes.Count > 0)
                tabTypeProp.stringValue = tabTypes[newIdx];
            else
                tabTypeProp.stringValue = "";

            // 页面数组
            EditorGUILayout.PropertyField(pagesProp, new GUIContent("pages"), true);

            // 删除按钮
            if (GUILayout.Button("删除本组"))
            {
                tabPageGroupsProp.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndVertical();
        }

        // 添加新TabPageGroup
        if (GUILayout.Button("添加新Tab页面组"))
        {
            tabPageGroupsProp.InsertArrayElementAtIndex(tabPageGroupsProp.arraySize);
            var newGroup = tabPageGroupsProp.GetArrayElementAtIndex(tabPageGroupsProp.arraySize - 1);
            newGroup.FindPropertyRelative("tabType").stringValue = (allTabTypesProp.arraySize > 0)
                ? allTabTypesProp.GetArrayElementAtIndex(0).stringValue : "";
            newGroup.FindPropertyRelative("pages").ClearArray();
        }

        serializedObject.ApplyModifiedProperties();
    }
}