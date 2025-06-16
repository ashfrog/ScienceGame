using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class CardTextGenerator : MonoBehaviour
{
    [Header("卡片设置")]
    public RectTransform cardBackground; // 卡片背景
    public Image cardImage; // 卡片背景图片
    public TextMeshProUGUI customText; // TextMeshPro-Text UI组件

    [Header("文字设置")]
    public string textContent = "自定义文字内容";
    public TMP_FontAsset textFont; // 使用TMP_FontAsset替代Font
    public Color textColor = Color.black;
    public float fontSize = 24f; // 使用float类型
    public TextAlignmentOptions textAlignment = TextAlignmentOptions.Center; // 使用TMP的对齐选项

    [Header("高级文字设置")]
    public bool enableWordWrapping = true;
    public bool enableAutoSizing = false;
    public float autoSizeMin = 12f;
    public float autoSizeMax = 48f;
    public float characterSpacing = 0f;
    public float lineSpacing = 0f;

    [Header("保存设置")]
    public string saveDirectory = "Assets/GeneratedCards/";
    public string fileName = "card";
    public string fileExtension = ".png";

    [Header("渲染设置")]
    public Camera renderCamera;
    public int renderWidth = 512;
    public int renderHeight = 768;

    void Start()
    {
        Settings.ini.Game.SaveCardDirectory = Settings.ini.Game.SaveCardDirectory;
        Settings.ini.Game.FontSize = Settings.ini.Game.FontSize;
        fontSize = Settings.ini.Game.FontSize;
        saveDirectory = Settings.ini.Game.SaveCardDirectory;
        // 确保保存目录存在
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        // 如果没有指定渲染相机，创建一个
        if (renderCamera == null)
        {
            SetupRenderCamera();
        }
    }

    /// <summary>
    /// 设置卡片文字内容
    /// </summary>
    /// <param name="text">要显示的文字</param>
    public void SetCardText(string text)
    {
        textContent = text;
        UpdateTextDisplay();
    }

    /// <summary>
    /// 更新文字显示
    /// </summary>
    void UpdateTextDisplay()
    {
        if (customText != null)
        {
            customText.text = textContent;
            customText.color = textColor;
            customText.fontSize = fontSize;
            customText.alignment = textAlignment;

            // 设置TMP特有属性
            if (textFont != null)
            {
                customText.font = textFont;
            }

            customText.enableWordWrapping = enableWordWrapping;
            customText.enableAutoSizing = enableAutoSizing;
            customText.fontSizeMin = autoSizeMin;
            customText.fontSizeMax = autoSizeMax;
            customText.characterSpacing = characterSpacing;
            customText.lineSpacing = lineSpacing;
        }
    }

    /// <summary>
    /// 生成并保存卡片图片
    /// </summary>
    public void GenerateAndSaveCard(string fileName = "card.jpg")
    {
        UpdateTextDisplay();

        // 创建渲染纹理
        RenderTexture renderTexture = new RenderTexture(renderWidth, renderHeight, 24);
        renderCamera.targetTexture = renderTexture;

        // 渲染到纹理
        renderCamera.Render();

        // 读取渲染结果
        RenderTexture.active = renderTexture;
        Texture2D screenshot = new Texture2D(renderWidth, renderHeight, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, renderWidth, renderHeight), 0, 0);
        screenshot.Apply();

        // 清理渲染纹理
        renderCamera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(renderTexture);

        // 保存图片
        byte[] data = screenshot.EncodeToPNG();
        string fullPath = Path.Combine(saveDirectory, fileName);
        File.WriteAllBytes(fullPath, data);

        // 清理内存
        DestroyImmediate(screenshot);

        Debug.Log("卡片已保存到: " + fullPath);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    /// <summary>
    /// 设置渲染相机
    /// </summary>
    void SetupRenderCamera()
    {
        GameObject cameraGO = new GameObject("Card Render Camera");
        renderCamera = cameraGO.AddComponent<Camera>();
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = Color.clear;
        renderCamera.cullingMask = LayerMask.GetMask("UI");
        renderCamera.orthographic = true;

        // 定位相机以包含整个卡片
        if (cardBackground != null)
        {
            Vector3 cardPosition = cardBackground.position;
            renderCamera.transform.position = new Vector3(cardPosition.x, cardPosition.y, cardPosition.z - 10);

            // 调整正交大小以适应卡片
            float cardHeight = cardBackground.rect.height;
            renderCamera.orthographicSize = cardHeight / 2f;
        }
    }

    /// <summary>
    /// 批量生成卡片（用于生成多张不同文字的卡片）
    /// </summary>
    /// <param name="textList">文字列表</param>
    public void GenerateMultipleCards(string[] textList)
    {
        for (int i = 0; i < textList.Length; i++)
        {
            SetCardText(textList[i]);
            fileName = "card_" + (i + 1).ToString("D3");
            GenerateAndSaveCard();
        }
    }

    /// <summary>
    /// 设置卡片背景图片
    /// </summary>
    /// <param name="backgroundSprite">背景精灵</param>
    public void SetCardBackground(Sprite backgroundSprite)
    {
        if (cardImage != null)
        {
            cardImage.sprite = backgroundSprite;
        }
    }

    // Inspector中的按钮，用于测试
    [ContextMenu("生成卡片")]
    public void GenerateCardFromInspector()
    {
        GenerateAndSaveCard();
    }

    [ContextMenu("测试批量生成")]
    public void TestBatchGenerate()
    {
        string[] testTexts = {
            "第一张卡片",
            "第二张卡片",
            "第三张卡片"
        };
        GenerateMultipleCards(testTexts);
    }
}