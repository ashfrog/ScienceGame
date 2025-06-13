#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CardTextGenerator))]
public class CardTextGeneratorEditor : Editor
{
    private CardTextGenerator cardGenerator;
    private string previewText = "";

    void OnEnable()
    {
        cardGenerator = (CardTextGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        // 绘制默认Inspector
        DrawDefaultInspector();

        EditorGUILayout.Space();

        // TextMeshPro特有设置
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("TextMeshPro设置", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("启用自动大小"))
        {
            cardGenerator.enableAutoSizing = true;
        }
        if (GUILayout.Button("禁用自动大小"))
        {
            cardGenerator.enableAutoSizing = false;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("启用换行"))
        {
            cardGenerator.enableWordWrapping = true;
        }
        if (GUILayout.Button("禁用换行"))
        {
            cardGenerator.enableWordWrapping = false;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.LabelField("快速操作", EditorStyles.boldLabel);

        // 文字预览和设置
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("文字预览", EditorStyles.boldLabel);

        previewText = EditorGUILayout.TextArea(previewText, GUILayout.Height(60));

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("设置文字"))
        {
            cardGenerator.SetCardText(previewText);
        }
        if (GUILayout.Button("清空文字"))
        {
            previewText = "";
            cardGenerator.SetCardText("");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 生成按钮
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("生成操作", EditorStyles.boldLabel);

        if (GUILayout.Button("生成当前卡片", GUILayout.Height(30)))
        {
            cardGenerator.GenerateAndSaveCard();
        }

        if (GUILayout.Button("批量生成测试卡片"))
        {
            string[] testTexts = {
                "测试卡片 1\n这是第一张测试卡片",
                "测试卡片 2\n这是第二张测试卡片",
                "测试卡片 3\n这是第三张测试卡片"
            };
            cardGenerator.GenerateMultipleCards(testTexts);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 设置快捷操作
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("快捷设置", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("大字体"))
        {
            cardGenerator.fontSize = 32f;
        }
        if (GUILayout.Button("中字体"))
        {
            cardGenerator.fontSize = 24f;
        }
        if (GUILayout.Button("小字体"))
        {
            cardGenerator.fontSize = 16f;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("左对齐"))
        {
            cardGenerator.textAlignment = TMPro.TextAlignmentOptions.Left;
        }
        if (GUILayout.Button("居中对齐"))
        {
            cardGenerator.textAlignment = TMPro.TextAlignmentOptions.Center;
        }
        if (GUILayout.Button("右对齐"))
        {
            cardGenerator.textAlignment = TMPro.TextAlignmentOptions.Right;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("黑色文字"))
        {
            cardGenerator.textColor = Color.black;
        }
        if (GUILayout.Button("白色文字"))
        {
            cardGenerator.textColor = Color.white;
        }
        if (GUILayout.Button("红色文字"))
        {
            cardGenerator.textColor = Color.red;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        // 保存更改
        if (GUI.changed)
        {
            EditorUtility.SetDirty(cardGenerator);
        }
    }
}

[CustomEditor(typeof(CardUISetup))]
public class CardUISetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        CardUISetup setup = (CardUISetup)target;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("UI创建工具", EditorStyles.boldLabel);

        if (GUILayout.Button("创建卡片UI结构", GUILayout.Height(30)))
        {
            setup.CreateCardUI();
        }

        if (GUILayout.Button("调整文字到下半部分"))
        {
            setup.AdjustTextToBottomHalf();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 帮助信息
        EditorGUILayout.BeginVertical("helpbox");
        EditorGUILayout.LabelField("使用说明:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("1. 点击'创建卡片UI结构'自动创建UI元素");
        EditorGUILayout.LabelField("2. 拖拽背景图片到CardBackgroundSprite字段");
        EditorGUILayout.LabelField("3. 使用CardTextGenerator脚本生成卡片");
        EditorGUILayout.EndVertical();
    }
}
#endif