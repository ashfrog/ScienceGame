using UnityEngine;

/// <summary>
/// 演示如何使用确认对话框的示例脚本
/// </summary>
public class ConfirmationDialogExample : MonoBehaviour
{
    // 可选：自定义图标
    [SerializeField] private Sprite warningIcon;
    [SerializeField] private Sprite deleteIcon;
    [SerializeField] private Sprite infoIcon;

    // 示例方法1：基本用法
    public void ShowBasicConfirmation()
    {
        // 最简单的用法
        ConfirmationDialogExtensions.ShowConfirmationDialog(
            "确认操作", 
            "您确定要执行此操作吗？",
            () => Debug.Log("用户点击了确认"),
            () => Debug.Log("用户点击了取消")
        );
    }

    // 示例方法2：删除确认
    public void ShowDeleteConfirmation()
    {
        // 自定义按钮文本和图标
        ConfirmationDialogExtensions.ShowConfirmationDialog(
            "删除警告", 
            "您确定要删除此项目吗？此操作无法撤销！",
            OnDeleteConfirmed,
            null, // 不需要取消回调
            "删除", // 自定义确认按钮文本
            "返回",  // 自定义取消按钮文本
            deleteIcon // 使用自定义图标
        );
    }

    private void OnDeleteConfirmed()
    {
        // 执行删除逻辑
        Debug.Log("删除操作已确认，正在执行删除...");
        // 实际删除代码...
    }

    // 示例方法3：退出游戏确认
    public void ShowExitConfirmation()
    {
        ConfirmationDialogExtensions.ShowConfirmationDialog(
            "退出游戏", 
            "您确定要退出游戏吗？未保存的进度将会丢失！",
            () => {
                Debug.Log("退出游戏...");
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            },
            () => Debug.Log("取消退出"),
            "确认退出", 
            "继续游戏",
            warningIcon
        );
    }

    // 示例方法4：单按钮确认(仅提示信息)
    public void ShowInfoDialog()
    {
        // 使用相同的确认和取消文本，可以实现单按钮效果
        ConfirmationDialogExtensions.ShowConfirmationDialog(
            "提示信息", 
            "游戏已自动保存。",
            () => Debug.Log("用户已确认信息"),
            () => Debug.Log("用户已确认信息"), // 两个按钮执行相同操作
            "确定", 
            "确定", // 相同文本，视觉上类似单按钮
            infoIcon
        );
    }

    // 示例方法5：在UI按钮上使用
    // 可以直接在Unity编辑器中将此方法拖拽到Button的OnClick事件上
    public void OnDeleteButtonClicked()
    {
        ShowDeleteConfirmation();
    }

    // 示例方法6：使用直接调用单例的方式
    public void ShowAdvancedConfirmation()
    {
        // 直接调用单例实例
        ConfirmationDialogManager.Instance.ShowDialog(
            "高级操作确认",
            "执行此操作将重置所有用户数据。\n您确定要继续吗？",
            () => {
                Debug.Log("执行高级操作...");
                // 复杂操作逻辑...
            },
            () => {
                Debug.Log("取消高级操作");
                // 取消后的逻辑...
            },
            "继续执行",
            "停止",
            warningIcon
        );
    }
}