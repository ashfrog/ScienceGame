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
    private TabSwitcher tabSwitcher;


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

        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.SetMovSeek, $"汽车街景|{index + 1}");
    }
    public void OnCarModel(int index) //汽车模型
    {
        panel_CarModel.SetActive(true);
        panel_Trim.SetActive(false);
        img_car.sprite = cars[index];
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.SetMovSeek, "汽车模型");
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.SetPlayMovie, index);
    }

    public void OnTrimBtn(string name)
    {
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.SetPlayMovieFolder, name);
    }

    public void OnPlay()
    {
        media.Play();
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.PlayMovie, "");
    }
    public void OnPause()
    {
        media.Pause();
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.PauseMovie, "");
    }
    public void IncreaseVolume()  //音量加
    {
        float currentVolume = media.Control.GetVolume();
        float newVolume = Mathf.Clamp(currentVolume + volumeStep, 0f, 1f);
        media.Control.SetVolume(newVolume);
        print(newVolume);
    }

    public void DecreaseVolume()  //音量减
    {
        float currentVolume = media.Control.GetVolume();
        float newVolume = Mathf.Clamp(currentVolume - volumeStep, 0f, 1f);
        media.Control.SetVolume(newVolume);
        print(newVolume);
    }
    public void OnRewind()  //重播暂停
    {
        media.Control.Rewind();
        media.Pause();
    }
    public void OnSliderValueChanged(float value)  //时间刻度进度条
    {
        // Update the text to display the current slider value
        print(value);
        progress.fillAmount = value;


        buttons[0].gameObject.SetActive(value >= 0.01f);
        buttons[1].gameObject.SetActive(value >= 0.1f);
        buttons[2].gameObject.SetActive(value >= 0.48f);
        buttons[3].gameObject.SetActive(value >= 0.58f);
        buttons[4].gameObject.SetActive(value >= 0.88f);
        buttons[5].gameObject.SetActive(value >= 0.88f);
        buttons[6].gameObject.SetActive(value >= 0.98f);
        buttons[7].gameObject.SetActive(value >= 0.98f);
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.WeiXingDot, value);
    }
    public void OnSatellite(int index) //在空姿态卫星按钮
    {
        Destroy(_stickyHeaderTable.headerRow);
        _satelliteDataReader.initialize(index);
    }
    public void OnYear(int index)
    {
        img_year.sprite = years[index];
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.Reload, index);
    }

    //public void OnCarModel()  //汽车模型
    //{
    //    _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.SetMovSeek, "汽车模型");
    //}
    public void OnCarVista()  //汽车街景
    {
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.SetMovSeek, "汽车街景");
    }
    public void OnCarTrim()  //汽车内饰
    {
        _mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.SetMovSeek, "汽车内饰");
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
