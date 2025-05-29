using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 切换到讲解模式时，诞始TabSwitcher显示实验柜Tab页面
/// </summary>
public class TabShowBySectionIndex : MonoBehaviour
{
    private TabSwitcher tabSwitcher;
    [SerializeField]
    Button[] switchModeBtns;

    private void Start()
    {
        if (switchModeBtns != null)
        {
            foreach (var btn in switchModeBtns)
            {
                btn.onClick.AddListener(OnSwitchModeBtnClicked);
            }
        }
    }

    private void OnSwitchModeBtnClicked()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(SwitchDelay());
        }
    }

    IEnumerator SwitchDelay()
    {
        yield return new WaitForSeconds(0.1f);
        Switch();
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        Switch();
    }

    private void Switch()
    {
        if (tabSwitcher == null)
        {
            tabSwitcher = GetComponentInChildren<TabSwitcher>();
        }
        if (tabSwitcher != null)
        {
            if (Settings.ini.Game.SectionIndex == 0)
            { // 更新Tab页面显示状态
                tabSwitcher.SwitchTab(2);
            }
            else
            {
                // 更新Tab页面显示状态
                tabSwitcher.SwitchTab(1);
            }
        }
        else
        {
            Debug.LogWarning("TabSwitcher component not found in children.");
        }
    }
}
