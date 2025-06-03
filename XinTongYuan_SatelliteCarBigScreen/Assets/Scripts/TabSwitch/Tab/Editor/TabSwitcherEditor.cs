using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(TabSwitcher))]
public class TabSwitcherEditor : Editor
{
    SerializedProperty allTabTypesProp;
    SerializedProperty tabPageGroupsProp;
    SerializedProperty tabButtonsProp;
    ReorderableList reorderableList;

    // 拖拽区域相关
    private Rect dragDropArea;
    private const float DRAG_DROP_AREA_HEIGHT = 40f;

    void OnEnable()
    {
        allTabTypesProp = serializedObject.FindProperty("allTabTypes");
        tabPageGroupsProp = serializedObject.FindProperty("tabPageGroups");
        tabButtonsProp = serializedObject.FindProperty("tabButtons");

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
                                "Tab " + (index) + ":");

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

        // 拷贝按钮紧贴在右边
        Rect lastRect = GUILayoutUtility.GetLastRect();
        if (GUI.Button(new Rect(lastRect.xMax - 50, lastRect.y, 50, EditorGUIUtility.singleLineHeight), "拷贝"))
        {
            CopyTabTypesToClipboard();
        }

        EditorGUILayout.Space();

        // 2. Tab按钮数组 - 支持拖入多个按钮自动创建Tab页面组
        DrawTabButtonsField();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("currentTabIndex"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("initTabPages"));

        EditorGUILayout.Space();

        // 3. 拖拽区域 - 支持拖入多个GameObject自动创建Tab页面组
        DrawDragDropArea();

        EditorGUILayout.Space();

        // 4. 使用可重排序列表绘制tabPageGroups
        reorderableList.DoLayoutList();

        EditorGUILayout.Space();

        // 添加一些辅助信息
        if (tabPageGroupsProp.arraySize == 0)
        {
            EditorGUILayout.HelpBox("添加Tab页面组的方法：\n" +
                                  "• 点击上方列表的 '+' 按钮手动添加\n" +
                                  "• 直接拖入多个Button到Tab按钮数组中自动创建\n" +
                                  "• 拖拽多个GameObject到上方拖拽区域自动创建", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("可以拖拽左侧的 '≡' 图标来重新排序Tab页面组", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// 绘制拖拽区域，支持拖入多个GameObject自动创建Tab页面组
    /// </summary>
    private void DrawDragDropArea()
    {
        // 创建拖拽区域
        dragDropArea = GUILayoutUtility.GetRect(0, DRAG_DROP_AREA_HEIGHT, GUILayout.ExpandWidth(true));

        // 绘制拖拽区域背景
        GUI.Box(dragDropArea, "");

        // 检查是否有对象被拖拽到这个区域
        Event evt = Event.current;
        bool isDragging = evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform;
        bool isInDragArea = dragDropArea.Contains(evt.mousePosition);

        if (isDragging && isInDragArea)
        {
            // 高亮显示拖拽区域
            GUI.backgroundColor = Color.green;
            GUI.Box(dragDropArea, "");
            GUI.backgroundColor = Color.white;
        }

        // 绘制提示文本
        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Italic
        };

        GUI.Label(dragDropArea, "拖拽多个GameObject到此处自动创建Tab页面组", centeredStyle);

        // 处理拖拽事件
        HandleDragAndDrop(evt, isInDragArea);
    }

    /// <summary>
    /// 处理拖拽和放置事件
    /// </summary>
    private void HandleDragAndDrop(Event evt, bool isInDragArea)
    {
        if (!isInDragArea) return;

        switch (evt.type)
        {
            case EventType.DragUpdated:
                // 检查拖拽的对象是否为GameObject
                bool hasValidObjects = DragAndDrop.objectReferences.Any(obj => obj is GameObject);
                DragAndDrop.visualMode = hasValidObjects ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                evt.Use();
                break;

            case EventType.DragPerform:
                // 接受拖拽操作
                DragAndDrop.AcceptDrag();

                // 获取所有拖拽的GameObject
                var draggedObjects = DragAndDrop.objectReferences
                    .OfType<GameObject>()
                    .Where(go => go != null)
                    .ToList();

                if (draggedObjects.Count > 0)
                {
                    CreateTabPageGroupsFromDraggedObjects(draggedObjects);
                }

                evt.Use();
                break;
        }
    }

    /// <summary>
    /// 从拖拽的对象创建Tab页面组
    /// </summary>
    private void CreateTabPageGroupsFromDraggedObjects(List<GameObject> draggedObjects)
    {
        serializedObject.Update();

        // 为每个拖拽的对象创建一个Tab页面组
        for (int i = 0; i < draggedObjects.Count; i++)
        {
            var obj = draggedObjects[i];

            // 创建新的Tab页面组
            tabPageGroupsProp.InsertArrayElementAtIndex(tabPageGroupsProp.arraySize);
            var newGroup = tabPageGroupsProp.GetArrayElementAtIndex(tabPageGroupsProp.arraySize - 1);

            // 设置Tab类型名（基于对象名称或使用默认名称）
            string tabTypeName = GetOrCreateTabTypeName(obj.name, tabPageGroupsProp.arraySize - 1);
            newGroup.FindPropertyRelative("tabType").stringValue = tabTypeName;

            // 初始化pages数组，将拖拽的对象作为第一个page
            var pagesProp = newGroup.FindPropertyRelative("pages");
            pagesProp.ClearArray();
            pagesProp.InsertArrayElementAtIndex(0);
            pagesProp.GetArrayElementAtIndex(0).objectReferenceValue = obj;

            Debug.Log($"从拖拽对象自动创建Tab页面组 '{tabTypeName}'，添加页面: '{obj.name}'");
        }

        serializedObject.ApplyModifiedProperties();

        // 显示成功消息
        Debug.Log($"成功从 {draggedObjects.Count} 个拖拽对象创建了 {draggedObjects.Count} 个Tab页面组");
    }

    /// <summary>
    /// 获取或创建Tab类型名（基于对象名称）
    /// </summary>
    private string GetOrCreateTabTypeName(string objectName, int index)
    {
        // 首先尝试基于对象名称创建一个合适的Tab类型名
        string baseTypeName = objectName;

        // 清理名称，移除常见的UI前缀/后缀
        if (baseTypeName.EndsWith("Panel", System.StringComparison.OrdinalIgnoreCase))
            baseTypeName = baseTypeName.Substring(0, baseTypeName.Length - 5);
        if (baseTypeName.EndsWith("Page", System.StringComparison.OrdinalIgnoreCase))
            baseTypeName = baseTypeName.Substring(0, baseTypeName.Length - 4);
        if (baseTypeName.StartsWith("Tab", System.StringComparison.OrdinalIgnoreCase))
            baseTypeName = baseTypeName.Substring(3);

        // 如果清理后为空，使用默认名称
        if (string.IsNullOrEmpty(baseTypeName.Trim()))
            baseTypeName = $"Tab{index + 1}";

        // 检查是否已存在相同的类型名，如果存在则添加数字后缀
        string finalTypeName = baseTypeName;
        int counter = 1;
        while (IsTabTypeNameExists(finalTypeName))
        {
            finalTypeName = $"{baseTypeName}{counter}";
            counter++;
        }

        // 将新的类型名添加到allTabTypes列表中
        allTabTypesProp.InsertArrayElementAtIndex(allTabTypesProp.arraySize);
        allTabTypesProp.GetArrayElementAtIndex(allTabTypesProp.arraySize - 1).stringValue = finalTypeName;

        return finalTypeName;
    }

    /// <summary>
    /// 检查Tab类型名是否已存在
    /// </summary>
    private bool IsTabTypeNameExists(string typeName)
    {
        for (int i = 0; i < allTabTypesProp.arraySize; i++)
        {
            if (allTabTypesProp.GetArrayElementAtIndex(i).stringValue == typeName)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 绘制Tab按钮字段，支持拖入时自动创建Tab页面组
    /// </summary>
    private void DrawTabButtonsField()
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(tabButtonsProp, new GUIContent("Tab按钮数组"), true);

        if (EditorGUI.EndChangeCheck())
        {
            // 当Tab按钮数组发生变化时，检查是否需要自动创建Tab页面组
            CheckAndCreateTabPageGroups();
        }
    }

    /// <summary>
    /// 检查并自动创建Tab页面组
    /// </summary>
    private void CheckAndCreateTabPageGroups()
    {
        serializedObject.ApplyModifiedProperties(); // 确保更改生效

        int tabButtonCount = tabButtonsProp.arraySize;
        int tabPageGroupCount = tabPageGroupsProp.arraySize;

        // 如果Tab按钮数量大于页面组数量，自动创建新的页面组
        if (tabButtonCount > tabPageGroupCount)
        {
            for (int i = tabPageGroupCount; i < tabButtonCount; i++)
            {
                // 创建新的Tab页面组
                tabPageGroupsProp.InsertArrayElementAtIndex(tabPageGroupsProp.arraySize);
                var newGroup = tabPageGroupsProp.GetArrayElementAtIndex(tabPageGroupsProp.arraySize - 1);

                // 设置Tab类型名
                string tabTypeName = GetAvailableTabTypeName(i);
                newGroup.FindPropertyRelative("tabType").stringValue = tabTypeName;

                // 初始化pages数组，将对应的Button作为第一个page
                var pagesProp = newGroup.FindPropertyRelative("pages");
                pagesProp.ClearArray();

                // 获取对应的Button GameObject并添加为第一个page
                var buttonProp = tabButtonsProp.GetArrayElementAtIndex(i);
                if (buttonProp.objectReferenceValue != null)
                {
                    var button = buttonProp.objectReferenceValue as UnityEngine.UI.Button;
                    if (button != null)
                    {
                        pagesProp.InsertArrayElementAtIndex(0);
                        pagesProp.GetArrayElementAtIndex(0).objectReferenceValue = button.gameObject;

                        Debug.Log($"自动创建Tab页面组 '{tabTypeName}'，并将按钮 '{button.name}' 添加为第一个页面");
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        // 如果Tab按钮数量减少，可以选择是否删除多余的页面组（这里不自动删除，避免意外丢失数据）
        else if (tabButtonCount < tabPageGroupCount)
        {
            Debug.LogWarning($"Tab按钮数量({tabButtonCount})少于页面组数量({tabPageGroupCount})，请手动调整页面组");
        }
    }

    /// <summary>
    /// 获取可用的Tab类型名
    /// </summary>
    private string GetAvailableTabTypeName(int index)
    {
        // 首先尝试使用现有的Tab类型名
        if (index < allTabTypesProp.arraySize)
        {
            string existingTypeName = allTabTypesProp.GetArrayElementAtIndex(index).stringValue;
            if (!string.IsNullOrEmpty(existingTypeName))
            {
                return existingTypeName;
            }
        }

        // 如果没有现有的类型名，创建一个新的
        string newTypeName = $"Tab{index + 1}";

        // 确保Tab类型名列表有足够的元素
        while (allTabTypesProp.arraySize <= index)
        {
            allTabTypesProp.InsertArrayElementAtIndex(allTabTypesProp.arraySize);
            allTabTypesProp.GetArrayElementAtIndex(allTabTypesProp.arraySize - 1).stringValue = $"Tab{allTabTypesProp.arraySize}";
        }

        // 如果当前位置为空，设置为新的类型名
        if (string.IsNullOrEmpty(allTabTypesProp.GetArrayElementAtIndex(index).stringValue))
        {
            allTabTypesProp.GetArrayElementAtIndex(index).stringValue = newTypeName;
        }

        return allTabTypesProp.GetArrayElementAtIndex(index).stringValue;
    }

    /// <summary>
    /// 拷贝Tab类型名到剪贴板
    /// </summary>
    private void CopyTabTypesToClipboard()
    {
        if (allTabTypesProp.arraySize == 0)
        {
            Debug.LogWarning("没有Tab类型可以拷贝");
            return;
        }

        List<string> tabTypes = new List<string>();
        for (int i = 0; i < allTabTypesProp.arraySize; i++)
        {
            string tabType = allTabTypesProp.GetArrayElementAtIndex(i).stringValue;
            if (!string.IsNullOrEmpty(tabType))
            {
                tabTypes.Add(tabType);
            }
        }

        if (tabTypes.Count == 0)
        {
            Debug.LogWarning("没有有效的Tab类型可以拷贝");
            return;
        }

        string result = string.Join(", ", tabTypes);
        EditorGUIUtility.systemCopyBuffer = result;
        Debug.Log($"已拷贝 {tabTypes.Count} 个Tab类型名: {result}");
    }
}