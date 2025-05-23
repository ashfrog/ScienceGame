using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 为ToggleButtonWithImage添加呼吸灯效果的独立脚本
/// 保留PNG原始Alpha通道，在其上叠加呼吸灯效果
/// 直接挂载到需要呼吸效果的ToggleButtonWithImage物体上
/// </summary>
[RequireComponent(typeof(ToggleButtonWithImage))]
public class ToggleBreathingEffect : MonoBehaviour
{
    [Header("呼吸灯效果设置")]
    [SerializeField] private float breathSpeed = 0.5f;        // 呼吸速度
    [SerializeField] private float minIntensity = 0.3f;      // 最小强度
    [SerializeField] private float maxIntensity = 0.8f;      // 最大强度
    [SerializeField] private Color breathColor = new Color(1f, 1f, 1f, 1f); // 呼吸灯颜色
    [SerializeField] private AnimationCurve breathCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 呼吸曲线

    private ToggleButtonWithImage toggleButton;
    private Image buttonImage;
    private GameObject breathingOverlay;
    private Image overlayImage;
    private Sprite originalSprite;
    private bool isBreathing = false;

    private void Start()
    {
        toggleButton = GetComponent<ToggleButtonWithImage>();
        buttonImage = GetComponent<Image>();

        if (buttonImage != null)
        {
            originalSprite = buttonImage.sprite;
        }

        CreateBreathingOverlay();
    }

    private void CreateBreathingOverlay()
    {
        // 创建呼吸灯遮罩GameObject
        breathingOverlay = new GameObject("BreathingOverlay");
        breathingOverlay.transform.SetParent(transform, false);

        // 添加Image组件
        overlayImage = breathingOverlay.AddComponent<Image>();

        // 使用相同的sprite来保持PNG的Alpha通道形状
        if (buttonImage != null && buttonImage.sprite != null)
        {
            overlayImage.sprite = buttonImage.sprite;
            overlayImage.type = buttonImage.type;
            overlayImage.preserveAspect = buttonImage.preserveAspect;
        }

        // 设置初始颜色
        overlayImage.color = new Color(breathColor.r, breathColor.g, breathColor.b, 0);

        // 设置RectTransform以完全对齐按钮
        RectTransform overlayRect = breathingOverlay.GetComponent<RectTransform>();
        RectTransform buttonRect = GetComponent<RectTransform>();

        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        overlayRect.anchoredPosition = Vector2.zero;

        // 设置层级，确保在按钮图片之上但在文字之下
        SetOverlayOrder();

        // 初始状态下隐藏遮罩
        breathingOverlay.SetActive(false);
    }

    private void Update()
    {
        if (overlayImage == null || toggleButton == null) return;

        // 检查sprite是否发生变化，同步更新遮罩sprite
        if (buttonImage != null && overlayImage.sprite != buttonImage.sprite)
        {
            overlayImage.sprite = buttonImage.sprite;
            // 重新设置层级以防文字被遮挡
            SetOverlayOrder();
        }

        // 检查是否需要开始或停止呼吸效果
        bool shouldBreathe = toggleButton.isOn;

        if (shouldBreathe && !isBreathing)
        {
            StartBreathing();
        }
        else if (!shouldBreathe && isBreathing)
        {
            StopBreathing();
        }

        // 执行呼吸动画
        if (isBreathing)
        {
            UpdateBreathingIntensity();
        }
    }

    private void StartBreathing()
    {
        isBreathing = true;
        breathingOverlay.SetActive(true);
    }

    private void StopBreathing()
    {
        isBreathing = false;
        breathingOverlay.SetActive(false);
    }

    private void UpdateBreathingIntensity()
    {
        float time = Time.time * breathSpeed;
        float curveValue = breathCurve.Evaluate((Mathf.Sin(time) + 1f) * 0.5f);
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, curveValue);

        Color color = breathColor;
        color.a = intensity;
        overlayImage.color = color;
    }

    private void SetOverlayOrder()
    {
        // 找到文字组件，确保呼吸灯在图片之上、文字之下
        Transform textTransform = null;

        // 查找TMP_Text组件
        var tmpText = GetComponentInChildren<TMPro.TMP_Text>();
        if (tmpText != null)
        {
            textTransform = tmpText.transform;
        }

        if (textTransform != null)
        {
            // 将呼吸灯放在文字前面（层级更低）
            int textSiblingIndex = textTransform.GetSiblingIndex();
            breathingOverlay.transform.SetSiblingIndex(Mathf.Max(0, textSiblingIndex - 1));
        }
        else
        {
            // 如果没有文字，放在最前面
            breathingOverlay.transform.SetAsFirstSibling();
        }
    }

    private void OnDestroy()
    {
        // 清理创建的GameObject
        if (breathingOverlay != null)
        {
            DestroyImmediate(breathingOverlay);
        }
    }
}