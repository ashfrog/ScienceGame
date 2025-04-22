using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

/// <summary>
/// 通用确认对话框管理器 - 单例模式
/// </summary>
public class ConfirmationDialogManager : MonoBehaviour
{
    // 单例实例
    private static ConfirmationDialogManager _instance;
    public static ConfirmationDialogManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 在场景中查找实例
                _instance = Object.FindObjectOfType<ConfirmationDialogManager>();

                // 如果没有找到，则创建一个新的
                if (_instance == null)
                {
                    // 加载预制体
                    GameObject prefab = Resources.Load<GameObject>("ConfirmationDialogManager");
                    if (prefab != null)
                    {
                        // 实例化预制体
                        GameObject obj = Instantiate(prefab);
                        obj.name = "ConfirmationDialogManager";
                        _instance = obj.GetComponent<ConfirmationDialogManager>();

                        // 确保挂载到Canvas上
                        Canvas canvas = Object.FindObjectOfType<Canvas>();
                        if (canvas == null)
                        {
                            // 如果场景中没有Canvas，创建一个
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

                        // 将对话框挂到Canvas下
                        obj.transform.SetParent(canvas.transform, false);

                        DontDestroyOnLoad(obj); // 保持在场景切换时不被销毁
                    }
                    else
                    {
                        Debug.LogError("ConfirmationDialogManager预制体未找到！请确保在Resources文件夹中存在该预制体。");
                    }
                }
            }
            return _instance;
        }
    }

    // UI组件引用
    [SerializeField] public GameObject dialogPanel;
    [SerializeField] public TextMeshProUGUI titleText;
    [SerializeField] public TextMeshProUGUI messageText;
    [SerializeField] public Button confirmButton;
    [SerializeField] public Button cancelButton;
    [SerializeField] public TextMeshProUGUI confirmButtonText;
    [SerializeField] public TextMeshProUGUI cancelButtonText;
    [SerializeField] public Image iconImage;

    // 当前回调
    private Action onConfirm;
    private Action onCancel;

    private void Awake()
    {
        // 确保单例的唯一性
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始状态为隐藏
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }

        // 添加按钮监听器
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
        }
    }

    /// <summary>
    /// 显示确认对话框
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="message">消息内容</param>
    /// <param name="onConfirmAction">确认按钮回调</param>
    /// <param name="onCancelAction">取消按钮回调</param>
    /// <param name="confirmText">确认按钮文本，默认为"确认"</param>
    /// <param name="cancelText">取消按钮文本，默认为"取消"</param>
    /// <param name="icon">对话框图标，可选</param>
    public void ShowDialog(
        string title,
        string message,
        Action onConfirmAction = null,
        Action onCancelAction = null,
        string confirmText = "确认",
        string cancelText = "取消",
        Sprite icon = null)
    {
        // 设置文本
        if (titleText != null) titleText.text = title;
        if (messageText != null) messageText.text = message;
        if (confirmButtonText != null) confirmButtonText.text = confirmText;
        if (cancelButtonText != null) cancelButtonText.text = cancelText;

        // 设置图标
        if (iconImage != null)
        {
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        // 保存回调
        onConfirm = onConfirmAction;
        onCancel = onCancelAction;

        // 显示对话框
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 关闭对话框
    /// </summary>
    public void CloseDialog()
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }

        // 清除回调引用
        onConfirm = null;
        onCancel = null;
    }

    /// <summary>
    /// 确认按钮点击处理
    /// </summary>
    private void OnConfirmClicked()
    {
        // 调用确认回调
        onConfirm?.Invoke();

        // 关闭对话框
        CloseDialog();
    }

    /// <summary>
    /// 取消按钮点击处理
    /// </summary>
    private void OnCancelClicked()
    {
        // 调用取消回调
        onCancel?.Invoke();

        // 关闭对话框
        CloseDialog();
    }
}

/// <summary>
/// 提供静态扩展方法，使调用更简洁
/// </summary>
public static class ConfirmationDialogExtensions
{
    /// <summary>
    /// 显示确认对话框的静态便捷方法
    /// </summary>
    public static void ShowConfirmationDialog(
        string title,
        string message,
        Action onConfirm = null,
        Action onCancel = null,
        string confirmText = "确认",
        string cancelText = "取消",
        Sprite icon = null)
    {
        ConfirmationDialogManager.Instance.ShowDialog(
            title, message, onConfirm, onCancel, confirmText, cancelText, icon);
    }
}
