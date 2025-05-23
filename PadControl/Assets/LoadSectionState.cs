using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设置情景模式选中状态
/// </summary>
public class LoadSectionState : MonoBehaviour
{
    [SerializeField]
    ToggleButtonGroup toggleButtonGroup;

    [SerializeField]
    UnityEngine.UI.Button[] buttons;

    // Start is called before the first frame update
    void Start()
    {
        if (buttons == null || buttons.Length == 0)
        {
            buttons = GetComponentsInChildren<Button>();
        }
        toggleButtonGroup.SetSelectedByIndex(Settings.ini.Game.SectionIndex);
        // 给每个按钮添加点击事件
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // 闭包问题，需单独声明
            buttons[i].onClick.AddListener(() =>
            {
                Settings.ini.Game.SectionIndex = index;
            });
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
