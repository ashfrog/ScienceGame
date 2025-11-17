using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleStyle : MonoBehaviour
{
    Toggle toggle;
    // Start is called before the first frame update
    void Start()
    {
        toggle = GetComponent<Toggle>();

        if (toggle.isOn)
        {
            GetComponentInChildren<Image>().enabled = false;
        }

        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    void OnToggleValueChanged(bool selected)
    {
        // 设置为选中样式
        GetComponentInChildren<Image>().enabled = !selected; // 选中时背景色为绿色
    }
}
