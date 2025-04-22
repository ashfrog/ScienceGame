using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 编辑器工具类，用于自动创建确认对话框预制体
/// </summary>
#if UNITY_EDITOR
public class ConfirmationDialogCreator
{
    [MenuItem("Tools/UI/Create Confirmation Dialog")]
    public static void CreateConfirmationDialog()
    {
        // 确保有Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            // 添加EventSystem
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        // 创建对话框父物体
        GameObject dialogManager = new GameObject("ConfirmationDialogManager");
        ConfirmationDialogManager manager = dialogManager.AddComponent<ConfirmationDialogManager>();
        dialogManager.transform.SetParent(canvas.transform, false);

        // 设置RectTransform组件，确保能在UI上正确显示
        RectTransform managerRect = dialogManager.AddComponent<RectTransform>();
        managerRect.anchorMin = new Vector2(0, 0);
        managerRect.anchorMax = new Vector2(1, 1);
        managerRect.offsetMin = Vector2.zero;
        managerRect.offsetMax = Vector2.zero;

        // 创建面板
        GameObject panel = CreatePanel(dialogManager.transform);
        manager.dialogPanel = panel;

        // 确保面板可见
        CanvasGroup canvasGroup = panel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // 创建标题
        TextMeshProUGUI titleText = CreateText(panel.transform, "Title", "警告", 24);
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Top;
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.8f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = new Vector2(20, 0);
        titleRect.offsetMax = new Vector2(-20, -10);
        manager.titleText = titleText;

        // 创建消息文本
        TextMeshProUGUI messageText = CreateText(panel.transform, "Message", "您确定要执行此操作吗？此操作无法撤销。", 18);
        messageText.alignment = TextAlignmentOptions.TopLeft;
        RectTransform messageRect = messageText.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 0.3f);
        messageRect.anchorMax = new Vector2(1, 0.8f);
        messageRect.offsetMin = new Vector2(20, 0);
        messageRect.offsetMax = new Vector2(-20, -10);
        manager.messageText = messageText;

        // 创建图标
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(panel.transform, false);
        Image iconImage = iconObj.AddComponent<Image>();
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.8f);
        iconRect.anchorMax = new Vector2(0.2f, 1);
        iconRect.offsetMin = new Vector2(10, 10);
        iconRect.offsetMax = new Vector2(-10, -10);
        manager.iconImage = iconImage;

        // 创建按钮容器
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(panel.transform, false);
        HorizontalLayoutGroup buttonLayout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.spacing = 20;
        buttonLayout.childAlignment = TextAnchor.MiddleCenter;
        buttonLayout.childControlWidth = false;
        buttonLayout.childControlHeight = false;
        buttonLayout.childForceExpandWidth = false;
        buttonLayout.childForceExpandHeight = false;

        RectTransform buttonContainerRect = buttonContainer.GetComponent<RectTransform>();
        buttonContainerRect.anchorMin = new Vector2(0, 0);
        buttonContainerRect.anchorMax = new Vector2(1, 0.3f);
        buttonContainerRect.offsetMin = new Vector2(0, 10);
        buttonContainerRect.offsetMax = new Vector2(0, -10);

        // 创建确认按钮
        Button confirmButton = CreateButton(buttonContainer.transform, "ConfirmButton", "确认", Color.green);
        RectTransform confirmRect = confirmButton.GetComponent<RectTransform>();
        confirmRect.sizeDelta = new Vector2(120, 50);
        TextMeshProUGUI confirmText = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
        manager.confirmButton = confirmButton;
        manager.confirmButtonText = confirmText;

        // 创建取消按钮
        Button cancelButton = CreateButton(buttonContainer.transform, "CancelButton", "取消", Color.gray);
        RectTransform cancelRect = cancelButton.GetComponent<RectTransform>();
        cancelRect.sizeDelta = new Vector2(120, 50);
        TextMeshProUGUI cancelText = cancelButton.GetComponentInChildren<TextMeshProUGUI>();
        manager.cancelButton = cancelButton;
        manager.cancelButtonText = cancelText;

        // 创建Resources文件夹（如果不存在）
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        // 保存预制体到Resources文件夹
        PrefabUtility.SaveAsPrefabAsset(dialogManager, "Assets/Resources/ConfirmationDialogManager.prefab");

        // 提示创建成功
        Debug.Log("确认对话框预制体已成功创建并保存至Resources文件夹！");

        // 删除场景中的临时对象
        Object.DestroyImmediate(dialogManager);
    }

    private static GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new GameObject("DialogPanel");
        panel.transform.SetParent(parent, false);

        // 添加RectTransform
        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(400, 250);
        rectTransform.anchoredPosition = Vector2.zero;

        // 添加图像组件和背景
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        return panel;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;

        return tmp;
    }

    private static Button CreateButton(Transform parent, string name, string text, Color color)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        Image image = buttonObj.AddComponent<Image>();
        image.color = color;

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;

        // 设置按钮颜色过渡
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = new Color(color.r * 1.2f, color.g * 1.2f, color.b * 1.2f);
        colors.pressedColor = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f);
        colors.selectedColor = color;
        button.colors = colors;

        // 创建文本
        TextMeshProUGUI tmp = CreateText(buttonObj.transform, "Text", text, 18);
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = tmp.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }
}
#endif