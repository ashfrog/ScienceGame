using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class TabPageGroup
{
    [Tooltip("该Tab对应的所有页面")]
    public GameObject[] pages;

    [Tooltip("Tab类型名（下拉选择）")]
    public string tabType;
}

public class TabSwitcher : MonoBehaviour
{
    public Button[] tabButtons;
    public int currentTabIndex = 0;

    [Header("全部Tab类型名（类似enum，可增删）")]
    public List<string> allTabTypes = new List<string> { "卫星星座发射展示", "卫星星座展示", "卫星在空姿态", "内外卫星星座对比", "全景展示" };

    [Header("每个Tab下挂载的页面组")]
    public List<TabPageGroup> tabPageGroups = new List<TabPageGroup>();

    public bool initTabPages;

    private void Start()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }
        if (initTabPages)
        {
            InitTabPages();
        }
        UpdateTabPages();
    }

    public void SwitchTab(int index)
    {
        currentTabIndex = index;
        UpdateTabPages();
    }

    /// <summary>
    /// 通过Tab类型名切换
    /// </summary>
    public void SwitchTab(string tabTypeName)
    {
        for (int i = 0; i < tabPageGroups.Count; i++)
        {
            if (tabPageGroups[i].tabType == tabTypeName)
            {
                currentTabIndex = i;
                UpdateTabPages();
                return;
            }
        }
        Debug.LogWarning("TabType " + tabTypeName + " not found in tabPageGroups.");
    }

    private void UpdateTabPages()
    {
        for (int i = 0; i < tabPageGroups.Count; i++)
        {
            var group = tabPageGroups[i];
            bool active = i == currentTabIndex;
            if (group.pages != null)
            {
                foreach (var page in group.pages)
                {
                    if (page != null)
                        page.SetActive(active);
                }
            }
        }
    }

    private void InitTabPages()
    {
        foreach (var group in tabPageGroups)
        {
            if (group.pages != null)
            {
                foreach (var page in group.pages)
                {
                    if (page != null)
                        page.SetActive(true);
                }
            }
        }
    }
}