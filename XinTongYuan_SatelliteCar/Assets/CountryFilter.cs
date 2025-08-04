using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountryFilter : MonoBehaviour
{
    public FHClientController clientController;
    public Action<string> CountryChanged;

    // 国家名字和简称的映射
    private Dictionary<string, string> countryAbbr = new Dictionary<string, string>
    {
        { "中国", "CN" },
        { "欧洲", "I-ESA" },
        { "俄罗斯", "RU" },
        { "美国", "US" },
        { "英国", "UK" }
    };

    // 国家颜色配置
    private Dictionary<string, string[]> countryColors = new Dictionary<string, string[]>
    {
        { "US", new string[] { "#007FFF", "#3399FF", "#0066CC", "#66B3FF", "#1A4DE6" } },
        { "CN", new string[] { "#FF0000", "#FF4D4D", "#CC0000", "#FF8080", "#E61A1A" } },
        { "RU", new string[] { "#00FFFF", "#4DFFCC", "#00CCCC", "#80FFE6", "#1AE6E6" } },
        { "I-ESA", new string[] { "#FFAAFF", "#FFBBFF", "#FFCCFF", "#FFDDFF", "#FFEEFF" } },
        { "UK", new string[] { "#800080", "#9966CC", "#6B46C1", "#A855F7", "#8B5CF6" } }
    };

    // 这里假设你的 Toggle 名字分别是：Toggle_中国、Toggle_欧洲、Toggle_俄罗斯、Toggle_美国、Toggle_英国
    public Toggle toggleChina;
    public Toggle toggleEurope;
    public Toggle toggleRussia;
    public Toggle toggleUSA;
    public Toggle toggleUK;

    public Toggle toggleDrawOrbit;

    private void OnEnable()
    {
        toggleDrawOrbit.isOn = false;
        UpdateCountryString();
    }

    void Start()
    {
        // 设置每个Toggle的文本颜色
        SetToggleTextColor(toggleChina, "CN");
        SetToggleTextColor(toggleEurope, "I-ESA");
        SetToggleTextColor(toggleRussia, "RU");
        SetToggleTextColor(toggleUSA, "US");
        SetToggleTextColor(toggleUK, "UK");

        // 添加监听器
        toggleChina.onValueChanged.AddListener(delegate { UpdateCountryString(); });
        toggleEurope.onValueChanged.AddListener(delegate { UpdateCountryString(); });
        toggleRussia.onValueChanged.AddListener(delegate { UpdateCountryString(); });
        toggleUSA.onValueChanged.AddListener(delegate { UpdateCountryString(); });
        toggleUK.onValueChanged.AddListener(delegate { UpdateCountryString(); });
        toggleDrawOrbit.onValueChanged.AddListener(delegate
        {
            // 发送指令到服务器
            clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.DrawOrbit, toggleDrawOrbit.isOn);
            Debug.Log("轨道显示状态: " + (toggleDrawOrbit.isOn ? "开启" : "关闭"));
        });

        // 初始化显示
        UpdateCountryString();
    }

    /// <summary>
    /// 设置Toggle的文本颜色
    /// </summary>
    /// <param name="toggle">Toggle组件</param>
    /// <param name="countryKey">国家Key</param>
    private void SetToggleTextColor(Toggle toggle, string countryKey)
    {
        if (toggle == null) return;

        // 获取Toggle的Label组件（通常是子对象）
        Text labelText = toggle.GetComponentInChildren<Text>();
        if (labelText == null) return;

        // 检查是否有对应的颜色配置
        if (countryColors.ContainsKey(countryKey) && countryColors[countryKey].Length > 0)
        {
            string hexColor = countryColors[countryKey][0]; // 取第一个颜色
            Color color;
            if (ColorUtility.TryParseHtmlString(hexColor, out color))
            {
                labelText.color = color;
                Debug.Log($"设置 {countryKey} 颜色为: {hexColor}");
            }
            else
            {
                Debug.LogWarning($"无法解析颜色: {hexColor}");
            }
        }
        else
        {
            Debug.LogWarning($"未找到国家 {countryKey} 的颜色配置");
        }
    }

    void UpdateCountryString()
    {
        List<string> selectedAbbr = new List<string>();
        if (toggleChina.isOn) selectedAbbr.Add(countryAbbr["中国"]);
        if (toggleEurope.isOn) selectedAbbr.Add(countryAbbr["欧洲"]);
        if (toggleRussia.isOn) selectedAbbr.Add(countryAbbr["俄罗斯"]);
        if (toggleUSA.isOn) selectedAbbr.Add(countryAbbr["美国"]);
        if (toggleUK.isOn) selectedAbbr.Add(countryAbbr["英国"]);

        string result = string.Join(",", selectedAbbr);
        CountryChanged?.Invoke(result);
        clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.CountryFilterChange, result);
        Debug.Log("当前勾选国家缩写: " + result); // 输出到控制台
    }
}