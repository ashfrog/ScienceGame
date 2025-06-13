using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUISetup : MonoBehaviour
{
    [Header("自动创建UI设置")]
    public bool autoCreateUI = true;
    public Sprite cardBackgroundSprite;

    [Header("UI尺寸设置")]
    public Vector2 cardSize = new Vector2(400, 600);
    public Vector2 textAreaSize = new Vector2(350, 200);
    public Vector2 textAreaOffset = new Vector2(0, -150); // 相对于卡片中心的偏移

    void Start()
    {
        if (autoCreateUI)
        {
            CreateCardUI();
        }
    }

    /// <summary>
    /// 自动创建卡片UI结构
    /// </summary>
    public void CreateCardUI()
    {
        // 创建Canvas
        GameObject canvasGO = GameObject.Find("Card Canvas");
        if (canvasGO == null)
        {
            canvasGO = new GameObject("Card Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // 创建卡片背景
        GameObject cardGO = new GameObject("Card Background");
        cardGO.transform.SetParent(canvasGO.transform, false);

        RectTransform cardRect = cardGO.AddComponent<RectTransform>();
        cardRect.sizeDelta = cardSize;
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);

        Image cardImage = cardGO.AddComponent<Image>();
        if (cardBackgroundSprite != null)
        {
            cardImage.sprite = cardBackgroundSprite;
        }
        else
        {
            cardImage.color = new Color(0.9f, 0.9f, 0.8f, 1f); // 默认卡片颜色
        }

        // 创建文字区域
        GameObject textAreaGO = new GameObject("Text Area");
        textAreaGO.transform.SetParent(cardGO.transform, false);

        RectTransform textAreaRect = textAreaGO.AddComponent<RectTransform>();
        textAreaRect.sizeDelta = textAreaSize;
        textAreaRect.anchorMin = new Vector2(0.5f, 0.5f);
        textAreaRect.anchorMax = new Vector2(0.5f, 0.5f);
        textAreaRect.pivot = new Vector2(0.5f, 0.5f);
        textAreaRect.anchoredPosition = textAreaOffset;

        // 添加TextMeshPro-Text UI组件
        TextMeshProUGUI textComponent = textAreaGO.AddComponent<TextMeshProUGUI>();
        textComponent.text = "在此添加自定义文字";
        textComponent.fontSize = 24f;
        textComponent.color = Color.black;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.enableWordWrapping = true;




        // 配置CardTextGenerator组件
        CardTextGenerator generator = GetComponent<CardTextGenerator>();
        if (generator == null)
        {
            generator = gameObject.AddComponent<CardTextGenerator>();
        }

        generator.cardBackground = cardRect;
        generator.cardImage = cardImage;
        generator.customText = textComponent;

        // 设置TextMeshPro的默认字体（如果有的话）

        textComponent.font = generator.textFont;
        Debug.Log("卡片UI已自动创建完成！");
    }

    /// <summary>
    /// 调整文字区域位置到卡片下半部分
    /// </summary>
    [ContextMenu("调整文字到下半部分")]
    public void AdjustTextToBottomHalf()
    {
        CardTextGenerator generator = GetComponent<CardTextGenerator>();
        if (generator != null && generator.customText != null)
        {
            RectTransform textRect = generator.customText.GetComponent<RectTransform>();

            // 将文字区域移动到卡片的下半部分
            float cardHeight = generator.cardBackground.rect.height;
            textRect.anchoredPosition = new Vector2(0, -cardHeight * 0.25f);

            Debug.Log("文字区域已调整到卡片下半部分");
        }
    }
}