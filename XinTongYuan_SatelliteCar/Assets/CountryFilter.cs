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
    }

    void Start()
    {
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
