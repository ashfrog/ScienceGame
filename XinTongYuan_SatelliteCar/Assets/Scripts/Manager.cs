using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    //public GameObject panel_level1, panel_level1_1, panel_level1_1_1, panel_level1_1_2, panel_level1_1_3, panel_level1_1_3_1, panel_level1_2, panel_CarYear, panel_CarModel, panel_Trim;
    public GameObject panel_CarYear, panel_CarModel, panel_Trim;
    public static Manager _ins;
    public MediaPlayer media;
    float volumeStep = 0.1f;
    public Slider slider;
    public Image progress, img_year;
    public SatelliteDataReader _satelliteDataReader;
    public StickyHeaderTable _stickyHeaderTable;
    public Button[] buttons;
    public Sprite[] years;
    public MouseTouchInputManager _mouseTouchInputManager;
    public Image img_car;
    public Sprite[] cars;
    public GameObject[] trims; //内饰

    [SerializeField]
    public TabSwitcher tabSwitcher;



    private void Awake()
    {
        _ins = this;
    }
    void Start()
    {
        slider.onValueChanged.AddListener(OnSliderValueChanged);

        progress.fillAmount = 0.1f;
        buttons[0].gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnCarYear(int index) //汽车年份
    {
        panel_Trim.SetActive(true);
        for (int i = 0; i < trims.Length; i++)
        {
            trims[i].SetActive(false);
        }
        trims[index].SetActive(true);
        panel_CarYear.SetActive(true);

        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.TabControl, $"汽车街景|{index + 1}");
    }
    public void OnCarModel(int index) //汽车模型
    {
        panel_CarModel.SetActive(true);
        panel_Trim.SetActive(false);
        img_car.sprite = cars[index];
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.TabControl, "汽车模型");
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.PlayGroundMovie, index.ToString());
        //_mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.SetPlayMovie, index);
    }

    public void OnTrimBtn(string name)
    {
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.SetPlayMovieFolder, name);
    }

    public void OnPlay()
    {
        //media.Play();
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.PlayMovie, "");
    }
    public void OnPause()
    {
        //media.Pause();
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.PauseMovie, "");
    }

    public void IncreaseVolume()  //音量加
    {
        //float currentVolume = media.Control.GetVolume();
        //float newVolume = Mathf.Clamp(currentVolume + volumeStep, 0f, 1f);
        //media.Control.SetVolume(newVolume);
        //print(newVolume);
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.VolumnUp, "");
    }

    public void DecreaseVolume()  //音量减
    {
        //float currentVolume = media.Control.GetVolume();
        //float newVolume = Mathf.Clamp(currentVolume - volumeStep, 0f, 1f);
        //media.Control.SetVolume(newVolume);
        //print(newVolume);
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.VolumnDown, "");
    }
    public void OnRewind()  //重播暂停
    {
        //media.Control.Rewind();
        //media.Pause();
    }


    public static readonly float[] fillAmounts = new float[] { 0.08f, 0.17f, 0.29f, 0.38f, 0.5f, 0.58f, 0.7f, 0.8f, 0.9f, 1f };
    public void OnSliderValueChanged(float value)  //时间刻度进度条
    {
        // value 换算到 [1980,2025] 5年一个节点 上
        float between = 2025 - 1980;
        double mapped = 1980 + value * between;
        // 四舍五入到最近的5年节点
        int year = (int)(Math.Round((mapped - 1980) / 5.0) * 5 + 1980);
        print(year);

        // 直接通过索引访问fillAmounts数组
        int index = (year - 1980) / 5;
        progress.fillAmount = fillAmounts[index];
        buttons[0].gameObject.SetActive(year >= 1978); //GPS
        buttons[1].gameObject.SetActive(year >= 1982); //格洛纳斯
        buttons[2].gameObject.SetActive(year >= 2000); //北斗
        buttons[3].gameObject.SetActive(year >= 2005); //伽利略
        buttons[4].gameObject.SetActive(year >= 2019); //一网
        buttons[5].gameObject.SetActive(year >= 2019); //星链
        buttons[6].gameObject.SetActive(year >= 2024); //千帆
        buttons[7].gameObject.SetActive(year >= 2024); //国网

        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.WeiXingDot, year);
    }
    public void OnSatellite(int index) //在空姿态卫星按钮
    {
        Destroy(_stickyHeaderTable.headerRow);
        _satelliteDataReader.initialize(index);
    }
    public void OnYear(int year)
    {
        int index = (year - 1978) / 2; // 计算索引
        img_year.sprite = years[index];
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.Reload, year);
    }

    //public void OnCarModel()  //汽车模型
    //{
    //    _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.SetMovSeek, "汽车模型");
    //}
    public void OnCarVista()  //汽车街景
    {
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.TabControl, "汽车街景");
    }
    public void OnCarTrim()  //汽车内饰
    {
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.TabControl, "汽车内饰");
    }

    public void OnStr(string str)
    {
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.Str, str);
    }
    public void OnJieShao(int index)  //发送更改星座介绍命令
    {
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.LoadUrl, index);
    }
    public void OnChangeUI(RectTransform rt) //改变可旋转区域的UI
    {
        _mouseTouchInputManager.touchArea = rt;
    }
}
public enum TabUIMainType
{
    Panel_level1, Panel_level1_1, Panel_level1_1_1, Panel_level1_1_2, Panel_level1_1_3, Panel_level1_1_3_1, Panel_level1_2
}