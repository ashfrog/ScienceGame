using UnityEngine;
using UnityEngine.UI;

public class TabSwitcher : MonoBehaviour
{
    // 定义一个Button数组来存储所有的Tab按钮
    public Button[] tabButtons;

    // 定义一个GameObject数组来存储所有的Tab页面
    public GameObject[] tabPages;

    // 当前选中的Tab索引
    private int currentTabIndex = 0;

    private void Start()
    {
        // 为每个Tab按钮添加点击事件
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i; // 缓存索引
            tabButtons[i].onClick.AddListener(() => OnTabButtonClicked(index));
        }

        // 初始化Tab页面显示状态
        UpdateTabPages();
    }

    // 当Tab按钮被点击时调用
    private void OnTabButtonClicked(int index)
    {
        currentTabIndex = index;
        UpdateTabPages();
    }

    // 更新Tab页面的显示状态
    private void UpdateTabPages()
    {
        for (int i = 0; i < tabPages.Length; i++)
        {
            tabPages[i].SetActive(i == currentTabIndex);
        }
    }
}