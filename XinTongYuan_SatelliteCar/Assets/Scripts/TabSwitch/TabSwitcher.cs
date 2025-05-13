using UnityEngine;
using UnityEngine.UI;

public class TabSwitcher : MonoBehaviour
{
    // 定义一个Button数组来存储所有的Tab按钮
    public Button[] tabButtons;

    // 当前选中的Tab索引
    public int currentTabIndex = 0;
    // 定义一个GameObject数组来存储所有的Tab页面
    public GameObject[] tabPages;

    /// <summary>
    /// 是否在开始时初始化Tab页面Enable状态 让Action事件在Enable中注册
    /// </summary>
    public bool initTabPages;



    private void Start()
    {
        // 为每个Tab按钮添加点击事件
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i; // 缓存索引
            tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }
        if (initTabPages)
        {
            // Enable所有Tab页面
            InitTabPages();
        }
        // 初始化Tab页面显示状态
        UpdateTabPages();
    }

    // 当Tab按钮被点击时调用
    public void SwitchTab(int index)
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

    private void InitTabPages()
    {
        for (int i = 0; i < tabPages.Length; i++)
        {
            tabPages[i].SetActive(true);
        }
    }
}