using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;

[CustomEditor(typeof(TabSwitcher))]
public class TabSwitcherEditor : Editor
{
    SerializedProperty allTabTypesProp;
    SerializedProperty tabPageGroupsProp;
    ReorderableList reorderableList;

    void OnEnable()
    {
        allTabTypesProp = serializedObject.FindProperty("allTabTypes");
        tabPageGroupsProp = serializedObject.FindProperty("tabPageGroups");

        // 创建可重排序列表
        reorderableList = new ReorderableList(serializedObject, tabPageGroupsProp, true, true, true, true);

        // 设置列表项高度
        reorderableList.elementHeight = EditorGUIUtility.singleLineHeight * 8; // 根据内容调整高度

        // 绘制列表头部
        reorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Tab页面组列表 (可拖拽排序)");
        };

        // 绘制每个列表项
        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = tabPageGroupsProp.GetArrayElementAtIndex(index);
            var pagesProp = element.FindPropertyRelative("pages");
            var tabTypeProp = element.FindPropertyRelative("tabType");

            rect.y += 2;

            // Tab类型下拉选择
            List<string> tabTypes = new List<string>();
            for (int j = 0; j < allTabTypesProp.arraySize; j++)
            {
                tabTypes.Add(allTabTypesProp.GetArrayElementAtIndex(j).stringValue);
            }

            EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight),
                                "Tab " + (index + 1) + ":");

            if (tabTypes.Count > 0)
            {
                int selectedIdx = Mathf.Max(0, tabTypes.IndexOf(tabTypeProp.stringValue));
                int newIdx = EditorGUI.Popup(new Rect(rect.x + 60, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight),
                                           selectedIdx, tabTypes.ToArray());
                tabTypeProp.stringValue = tabTypes[newIdx];
            }
            else
            {
                EditorGUI.LabelField(new Rect(rect.x + 60, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight),
                                   "请先添加Tab类型");
                tabTypeProp.stringValue = "";
            }

            rect.y += EditorGUIUtility.singleLineHeight + 2;

            // Pages数组 - 使用更紧凑的显示
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight * 5),
                                  pagesProp, new GUIContent("Pages"), true);
        };

        // 添加新元素时的回调
        reorderableList.onAddCallback = (ReorderableList list) =>
        {
            tabPageGroupsProp.InsertArrayElementAtIndex(tabPageGroupsProp.arraySize);
            var newGroup = tabPageGroupsProp.GetArrayElementAtIndex(tabPageGroupsProp.arraySize - 1);
            newGroup.FindPropertyRelative("tabType").stringValue = (allTabTypesProp.arraySize > 0)
                ? allTabTypesProp.GetArrayElementAtIndex(0).stringValue : "";
            newGroup.FindPropertyRelative("pages").ClearArray();
        };

        // 动态计算每个元素的高度
        reorderableList.elementHeightCallback = (int index) =>
        {
            var element = tabPageGroupsProp.GetArrayElementAtIndex(index);
            var pagesProp = element.FindPropertyRelative("pages");

            float height = EditorGUIUtility.singleLineHeight + 4; // Tab类型选择行

            // 计算Pages数组的高度
            if (pagesProp.isExpanded)
            {
                height += EditorGUIUtility.singleLineHeight; // "Pages"标签行
                height += EditorGUIUtility.singleLineHeight; // Size字段行
                height += pagesProp.arraySize * EditorGUIUtility.singleLineHeight; // 每个页面元素
                height += 10; // 额外间距
            }
            else
            {
                height += EditorGUIUtility.singleLineHeight + 4; // 折叠状态的Pages行
            }

            return height;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 1. 先绘制allTabTypes（可动态增删）
        EditorGUILayout.PropertyField(allTabTypesProp, new GUIContent("自定义Tab类型名"), true);

        EditorGUILayout.Space();

        // 2. 其它字段
        EditorGUILayout.PropertyField(serializedObject.FindProperty("tabButtons"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("currentTabIndex"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("initTabPages"));

        EditorGUILayout.Space();

        // 3. 使用可重排序列表绘制tabPageGroups
        reorderableList.DoLayoutList();

        EditorGUILayout.Space();

        // 添加一些辅助信息
        if (tabPageGroupsProp.arraySize == 0)
        {
            EditorGUILayout.HelpBox("点击上方列表的 '+' 按钮添加Tab页面组", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("可以拖拽左侧的 '≡' 图标来重新排序Tab页面组", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }
}