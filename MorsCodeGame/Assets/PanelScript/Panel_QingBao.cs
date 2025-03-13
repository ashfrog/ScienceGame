using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel_QingBao : MonoBehaviour
{

    [SerializeField]
    TabSwitcher tabSwitcher;
    public Button[] buttons;

    public int nextTab = 3;
    void Start()
    {
        if (tabSwitcher == null)
        {
            tabSwitcher = GetComponentInParent<TabSwitcher>();
        }
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            button.onClick.AddListener(() =>
            {
                tabSwitcher.SwitchTab(nextTab);
            });
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
