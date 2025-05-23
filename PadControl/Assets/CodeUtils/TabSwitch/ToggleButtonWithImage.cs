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

        button.onClick.AddListener(ToggleClick);

        parentGroup = GetComponentInParent<ToggleButtonGroup>();

        UpdateButtonImage();
    }

    private void ToggleClick()
    {
        // 检查父组是否允许多选
        if (parentGroup != null && !parentGroup.allowMultipleSelection)
        {
            // 单选模式：如果已经选中，则不执行任何操作
            if (isOn)
            {
                return;
            }
            // 如果未选中，则选中当前按钮
            isOn = true;
        }
        else
        {
            // 多选模式：正常切换状态
            isOn = !isOn;
        }

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