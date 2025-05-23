using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单选 多选 状态按钮 配合ToggleButtonGroup使用
/// </summary>
[RequireComponent(typeof(Button))]
public class ToggleButtonWithImage : MonoBehaviour
{
    public Sprite onSprite;
    [SerializeField]
    Color TextColor = Color.white;
    Color defaltTextColor;

    public bool isOn = false;
    public bool alphaHitTestMinimumThreshold = false;

    private Button button;
    private Image buttonImage;
    private Sprite offSprite;
    private ToggleButtonGroup parentGroup;
    private TMP_Text text;


    private void Start()
    {

        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        if (buttonImage != null)
        {
            if (offSprite == null && buttonImage.sprite != null)
            {
                offSprite = buttonImage.sprite;
            }
        }

        if (buttonImage != null && alphaHitTestMinimumThreshold)
        {
            buttonImage.alphaHitTestMinimumThreshold = 0.1f; // 设置透明图片点击阈值
        }

        text = GetComponentInChildren<TMP_Text>();
        defaltTextColor = text.color;

        button.onClick.AddListener(ToggleState);

        parentGroup = GetComponentInParent<ToggleButtonGroup>();

        UpdateButtonImage();
    }

    private void ToggleState()
    {
        isOn = !isOn;
        UpdateButtonImage();

        if (parentGroup != null)
        {
            parentGroup.OnToggleStateChanged(this);
        }
    }

    public void UpdateButtonImage()
    {
        if (buttonImage != null)
        {
            buttonImage.sprite = isOn ? onSprite : offSprite;
            text.color = isOn ? TextColor : defaltTextColor;
        }
    }
}