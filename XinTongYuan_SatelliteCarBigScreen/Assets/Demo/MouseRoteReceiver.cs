using Newtonsoft.Json;
using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TouchSocket.Core.ByteManager;
using TouchSocket.Sockets;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using DG.Tweening;
using Lean.Common;
using System.IO;
using TMPro;
using System.Linq;
using ChartAndGraph;

public class MouseRoteReceiver : MonoBehaviour
{
    [SerializeField]
    FHTcpService tcpService;

    [SerializeField]
    LeanPitchYaw leanPitchYaw;

    // 定义平滑系数 (0-1之间，越小越平滑)
    public float smoothFactor = 0.001f;

    // 目标增量旋转值
    private Vector2 targetRotationDelta = Vector2.zero;

    public LitVCR litVCR1;

    public LitVCR litVCR2;

    public Sprite[] sprites_Introduce;   //介绍图组
    public Sprite[] sprites_Introduce2;   //介绍图组2
    public Image img_Introduce;  //图片介绍组件
    public Image img_Introduce2;  //图片介绍组件2
    //public GameObject WeiXingGuangDian;  //卫星光点
    public GameObject[] cars;
    public GameObject oribit; //卫星轨道模型

    public GameObject theEarth; //地球模型

    [SerializeField]
    TMP_Text text_WX;

    [SerializeField]
    SatelliteOrbitRenderer satelliteOrbitRenderer;


    public enum TabUILabel //需要Inspector中TabSwitcher的allTabTypes保持一致
    {
        P1_1, P1_1_1, P1_1_2, P1_1_3, P1_2, P1_2_1, P1_2_2, P1_2_3, panel_level2_1_1, panel_level2_1_2, panel_level2_1_3, panel_level2_1_4, panel_level2_1_5, Panel_卫星展示, Panel_内外星座对比
    }
    public TabSwitcher tabSwitcher_UI;
    public enum TabObjLabel
    {
        地球, 年份卫星数量, 卫星展示, 卫星轨道, 车模, 卫星光点
    }
    public TabSwitcher tabSwitcher_Obj;
    /// <summary>
    /// index对应星座
    /// </summary>
    public enum ConstellationGroup
    {
        伽利略星座,
        国网星座,
        千帆星座,
        北斗星座,
        格洛纳斯星座,
        GPS星座,
        星链星座,
        一网卫星
    }

    public enum ExelSheetGroup
    {
        北斗星座,
        GPS星座,
        国网星座,
        千帆星座,
        星链星座,
        一网卫星,
        伽利略星座,
        格洛纳斯星座,
    }


    public enum EarthGroup
    {
        北斗一号, 北斗二号, 北斗三号, 全球定位系统1, 全球定位系统2, 伽俐略, 千帆, 星链, 一网, 格洛纳斯A, 格洛纳斯B
    }
    [SerializeField]
    private TabSwitcher carTabSwitcher;
    [SerializeField]
    private TabSwitcher wxTabSwitcher;

    [SerializeField]
    private TabSwitcher orbitTabSwitcher;

    private int curCarIndex;

    [SerializeField]
    Camera camObj;


    private float defaultCameraFieldofView;

    string countrys = "CN,US,UK"; //逗号分割的国家缩写

    [SerializeField]
    SatletExelDataReader satletExelDataReader;

    // Start is called before the first frame update
    void Start()
    {
        string filnames1 = litVCR1.ReloadFileList();
        string filnames2 = litVCR2.ReloadFileList();
        defaultCameraFieldofView = camObj.fieldOfView;
        Settings.ini.Game.ShowOrbitWhenPie = Settings.ini.Game.ShowOrbitWhenPie;
        StartCoroutine(WaitForTcpServiceInitialization());
        MainPageLoop();
        //Settings.ini.Game.AutoResetTime = Settings.ini.Game.AutoResetTime;
        autoResetTime = Settings.ini.Game.AutoResetTime;
    }

    public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        switch (et)
        {

            case MediaPlayerEvent.EventType.FinishedPlaying:
                //Panel_LoopVideo.SetActive(true);
                //video_car.SetActive(false);
                //media_Loop.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "天屏汽车循环.mp4");
                break;
        }

        Debug.Log("Event: " + et.ToString());
    }

    float curt = 0f;
    float autoResetTime = 10f;
    // Update is called once per frame
    void Update()
    {
        //只有当有旋转增量需要应用时才进行旋转
        if (targetRotationDelta.sqrMagnitude > 0.001f)
        {
            leanPitchYaw.Pitch -= targetRotationDelta.y * smoothFactor;
            leanPitchYaw.Yaw += targetRotationDelta.x * smoothFactor;

            // 逐渐减少剩余的旋转增量
            targetRotationDelta.x *= (1f - smoothFactor);
            targetRotationDelta.y *= (1f - smoothFactor);
        }

        curt += Time.deltaTime;
        if (curt > autoResetTime)
        {
            curt = 0;
            MainPageLoop();
            tcpService.Send(sc, OrderTypeEnum.Str, "MainPage");
        }
    }
    SocketClient sc;
    private IEnumerator WaitForTcpServiceInitialization()
    {
        // 等待tcpService初始化完成
        yield return new WaitForSeconds(1f);

        // 绑定tcpService接收消息事件
        tcpService.fh_tcpservice.Received += this.FHService_Received;
    }
    Vector2 vec2;





    private void FHService_Received(SocketClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
    {
        Loom.QueueOnMainThread(() =>
        {
            sc = client;
            curt = 0;
            // 处理接收到的消息
            try
            {
                var info = requestInfo as DTOInfo;
                if ((OrderTypeEnum)info.OrderType != OrderTypeEnum.GetPlayInfo)
                {
                    Debug.Log((OrderTypeEnum)info.OrderType + "  " + (DataTypeEnum)info.DataType);
                }
                switch ((OrderTypeEnum)info.OrderType)
                {
                    case OrderTypeEnum.Str:
                        {
                            string cmdstr = JsonConvert.DeserializeObject<String>(Encoding.UTF8.GetString(info.Body));
                            Debug.Log(cmdstr);
                            switch (cmdstr)
                            {
                                case "主页面":
                                    MainPageLoop();
                                    break;
                                case "P1_1":
                                    ReturnP1_1();
                                    break;
                                case "P1_2":
                                    ReturnP1_2();
                                    break;
                            }

                        }
                        break;
                    case OrderTypeEnum.PlayGroundMovie:
                        string groundMovIndex = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(info.Body));
                        string groundMovName = $"{groundMovIndex}代俯视.mp4";
                        litVCR2.OpenVideoByFileName(groundMovName);
                        break;
                    case OrderTypeEnum.VolumnUp:
                        litVCR1.VolumnUp();
                        litVCR2.VolumnUp();
                        break;
                    case OrderTypeEnum.VolumnDown:
                        litVCR1.VolumnDown();
                        litVCR2.VolumnDown();
                        break;
                    case OrderTypeEnum.Rotate:
                        {
                            string v2 = JsonConvert.DeserializeObject<String>(Encoding.UTF8.GetString(info.Body)); //v2
                                                                                                                   //V2 为逗号分割的字符串，第一个为x轴旋转角度增量，第二个为y轴旋转角度增量

                            string[] v2s = v2.Split(',');
                            vec2 = new Vector2(float.Parse(v2s[0]), float.Parse(v2s[1]));
                            //Debug.Log(vec2);
                            // 将接收到的旋转角度增量添加到目标增量中
                            targetRotationDelta += vec2;
                        }
                        break;
                    case OrderTypeEnum.TabControl:
                        {
                            string cmdstr = JsonConvert.DeserializeObject<String>(Encoding.UTF8.GetString(info.Body));

                            string[] cmdstrs = cmdstr.Split('|');

                            string cmd = cmdstrs[0];

                            string cmdparam = "";
                            if (cmdstrs.Length > 1)
                            {
                                cmdparam = cmdstrs[1];
                            }

                            switch (cmd)
                            {
                                case "卫星":
                                    print(cmd + " " + cmdparam);
                                    litVCR1.OpenVideoByFileName("卫星产业系统展示.mp4", true, true, () =>
                                    {
                                        ReturnP1_1();
                                    });
                                    break;
                                case "发射展示":  //1-1 卫星发射展示
                                    print(cmd + " " + cmdparam);
                                    tabSwitcher_Obj.Hide();
                                    tabSwitcher_UI.SwitchTab(TabUILabel.P1_1_1);
                                    //MainPageLoop();
                                    SocketClient sc = client;
                                    litVCR1.OpenVideoByFileName("火箭发射全息屏.mp4", true, true, () =>
                                    {
                                        ReturnP1_1();
                                        tcpService.Send(sc, OrderTypeEnum.Str, "P1_1");
                                    });
                                    litVCR2.OpenVideoByFileName("地屏循环地球.mp4");
                                    //mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "地屏循环地球.mp4");
                                    break;
                                case "发射展示返回"://1-1返回 
                                    //MainPageLoop();
                                    Debug.Log(cmdstr);
                                    ReturnP1_1();
                                    break;
                                case "星座展示"://1-2 卫星星座展示
                                    leanPitchYaw.PitchMin = -90f;
                                    leanPitchYaw.PitchMax = 90f;
                                    Debug.Log(cmdstr);
                                    tabSwitcher_UI.SwitchTab(TabUILabel.P1_1_2);
                                    tabSwitcher_Obj.SwitchTab(TabObjLabel.卫星轨道);
                                    orbitTabSwitcher.SwitchTab(0);
                                    satelliteOrbitRenderer.SetBaseSatelliteScale(0.8f);
                                    satelliteOrbitRenderer.SetDisplayGroup("伽利略星座");


                                    litVCR1.OpenVideoByFileName("待机循环粒子背景.mp4");
                                    litVCR2.OpenVideoByFileName("汽车百年进化论地屏.mp4");
                                    break;
                                case "星座展示返回"://1-2 返回 选项界面
                                    leanPitchYaw.Pitch = 10f;
                                    Debug.Log(cmdstr);
                                    StopDoTween();
                                    camObj.fieldOfView = defaultCameraFieldofView;
                                    //Panel_level1_1_2.SetActive(false);
                                    tabSwitcher_Obj.Hide();
                                    oribit.gameObject.SetActive(false);
                                    satelliteOrbitRenderer.SetDisplayMode(DisplayMode.None);

                                    //MainPageLoop();
                                    ReturnP1_1();
                                    break;
                                case "星座对比"://1-3 卫星在空姿态

                                    Debug.Log(cmdstr);
                                    //Panel_level1_1_3.SetActive(true);
                                    tabSwitcher_UI.SwitchTab(TabUILabel.P1_1_3);
                                    tabSwitcher_Obj.SwitchTab(TabObjLabel.卫星光点);
                                    //WeiXingGuangDian.SetActive(true);
                                    //obj = theEarth;
                                    //oribit.gameObject.SetActive(true);
                                    //Panel_LoopVideo.SetActive(false);
                                    //mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "汽车百年进化论地屏.mp4");
                                    ResetZ(4);
                                    litVCR1.OpenVideoByFileName("卫星待机循环动画.mp4");
                                    litVCR2.OpenVideoByFileName("汽车百年进化论地屏.mp4");
                                    break;
                                case "星座对比返回"://1-3-1返回 （1-3卫星在空姿态）
                                    satletExelDataReader.Hide();
                                    leanPitchYaw.Pitch = 10f;
                                    Debug.Log(cmdstr);
                                    StopDoTween();
                                    satelliteOrbitRenderer.SetDisplayMode(DisplayMode.None);
                                    //Panel_level1_1_3.SetActive(true);
                                    //Panel_卫星在空姿态.SetActive(false);
                                    tabSwitcher_UI.SwitchTab(TabUILabel.P1_1_3);
                                    //WeiXingGuangDian.SetActive(true);
                                    //ReturnP1_1();
                                    break;
                                case "在空姿态"://1-3-1 内外星座对比 改成了星座对比
                                    satletExelDataReader.Show();
                                    tabSwitcher_UI.SwitchTab(TabUILabel.Panel_内外星座对比);
                                    Debug.Log(cmdstr);
                                    leanPitchYaw.Pitch = 10f;
                                    leanPitchYaw.Camera.DOFieldOfView(35f, 1f);
                                    wxTabSwitcher.SwitchTab(-1);
                                    if (Settings.ini.Game.ShowOrbitWhenPie)
                                    {
                                        satelliteOrbitRenderer.SetDisplayMode(DisplayMode.OrbitOnly);
                                    }
                                    else
                                    {
                                        satelliteOrbitRenderer.SetDisplayMode(DisplayMode.None);
                                    }
                                    satelliteOrbitRenderer.SetBaseSatelliteScale(0.5f);
                                    //Panel_level1_1_3.SetActive(false);
                                    //Panel_卫星在空姿态.SetActive(true);
                                    //WeiXingGuangDian.SetActive(false);
                                    //obj = theEarth;
                                    theEarth.SetActive(true);
                                    break;
                                case "在空姿态返回":// 1-3返回 1
                                    leanPitchYaw.Pitch = 10f;
                                    Debug.Log(cmdstr);
                                    StopDoTween();
                                    //Panel_level1_1_3.SetActive(false);
                                    tabSwitcher_Obj.Hide();
                                    theEarth.transform.localPosition = new Vector3(0, 0, 0); //重置地球位置
                                    camObj.fieldOfView = defaultCameraFieldofView; //重置相机视角
                                    satelliteOrbitRenderer.SetDisplayMode(DisplayMode.None);
                                    //for (int i = 1; i < WeiXingGuangDian.transform.childCount; i++)
                                    //{
                                    //    WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(false);
                                    //}
                                    ReturnP1_1();
                                    //Panel_LoopVideo.SetActive(true);
                                    //MainPageLoop();
                                    //media_Loop.Play();
                                    //mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "汽车百年进化论地屏.mp4");
                                    break;
                                case "汽车":
                                    Debug.Log(cmdstr);
                                    leanPitchYaw.PitchMin = 10f;
                                    leanPitchYaw.PitchMax = 90f;
                                    leanPitchYaw.Pitch = 10f;
                                    tabSwitcher_Obj.Hide();
                                    //tabSwitcher_UI.SwitchTab(TabUILabel.P汽车百年进化论);
                                    //video_car.SetActive(true);
                                    //media_Car.GetComponent<MediaPlayer>().Rewind(true);
                                    //media_Car.GetComponent<MediaPlayer>().Play();
                                    litVCR1.OpenVideoByFileName("汽车百年进化论全息屏.mp4", false, true, () =>
                                    {
                                        ReturnP1_2();
                                    });
                                    //Panel_LoopVideo.SetActive(false);
                                    //mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "汽车百年进化论地屏.mp4");
                                    litVCR2.OpenVideoByFileName("汽车百年进化论地屏.mp4");
                                    break;
                                case "汽车返回":
                                    Debug.Log(cmdstr);
                                    MainPageLoop();
                                    break;
                                case "汽车模型":
                                    Debug.Log(cmdstr);
                                    //Panel_level1_2_1.SetActive(true);
                                    //Panel_level1_2_2.SetActive(false);
                                    //panel_level1_2_3.SetActive(false);
                                    //panel_TanChuangVideo.SetActive(false);
                                    //camTabSwitcher.SwitchTab((int)CamGroup.全息);
                                    //oribit.gameObject.SetActive(false);
                                    ShowCarMode();
                                    break;
                                case "汽车模型返回":
                                    Debug.Log(cmdstr);
                                    ShowCarMode();
                                    break;
                                case "汽车街景":
                                    Debug.Log(cmdstr);
                                    int.TryParse(cmdparam, out curCarIndex);
                                    ShowCarUI();
                                    ShowCarMode();
                                    ShowCarModel(curCarIndex);
                                    leanPitchYaw.Pitch = 0f;
                                    break;
                                case "汽车街景返回":
                                    Debug.Log(cmdstr);
                                    for (int i = 0; i < cars.Length; i++)
                                    {
                                        cars[i].SetActive(false);
                                    }
                                    //media_Loop.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "天屏屏保.mp4");
                                    litVCR1.OpenVideoByFileName("天屏屏保.mp4");
                                    break;
                                case "汽车内饰":
                                    Debug.Log(cmdstr);
                                    //Panel_level1_2_1.SetActive(false);
                                    //Panel_level1_2_2.SetActive(false);
                                    //panel_level1_2_3.SetActive(true);
                                    break;
                                case "汽车内饰返回":
                                    Debug.Log(cmdstr);
                                    tabSwitcher_UI.SwitchTab(TabUILabel.P1_2);
                                    carTabSwitcher.Hide();
                                    //media_Loop.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "天屏汽车循环.mp4");
                                    litVCR1.OpenVideoByFileName("待机循环粒子背景.mp4");
                                    //mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "火箭发射地屏.mp4");
                                    litVCR2.OpenVideoByFileName("火箭发射地屏.mp4");
                                    break;
                            }
                        }
                        break;
                    case OrderTypeEnum.PlayMovie:
                        Debug.Log((OrderTypeEnum)info.OrderType + "  " + (DataTypeEnum)info.DataType);
                        //media.Play();
                        litVCR1.OnPlayButton();
                        break;
                    case OrderTypeEnum.PauseMovie:
                        Debug.Log((OrderTypeEnum)info.OrderType + "  " + (DataTypeEnum)info.DataType);
                        //media.Pause();
                        litVCR1.OnPauseButton();
                        break;
                    case OrderTypeEnum.LoadUrl:          //星座展示 选星座
                        {
                            Debug.Log((OrderTypeEnum)info.OrderType + "  " + (DataTypeEnum)info.DataType);
                            int index = JsonConvert.DeserializeObject<int>(Encoding.UTF8.GetString(info.Body));
                            tabSwitcher_Obj.SwitchTab(TabObjLabel.卫星轨道);
                            orbitTabSwitcher.SwitchTab(index);
                            DisplayMode displayMode = index != 6 ? DisplayMode.Both : DisplayMode.SatelliteOnly;
                            satelliteOrbitRenderer.SetDisplayGroup(Enum.GetName(typeof(ConstellationGroup), index), displayMode);
                            img_Introduce.sprite = sprites_Introduce[index];
                            img_Introduce2.sprite = sprites_Introduce2[index];
                            ResetZ(index);
                        }

                        break;
                    case OrderTypeEnum.ShowGroup:          //卫星姿态选星座exelsheet
                        {
                            int exelsheetindex = JsonConvert.DeserializeObject<int>(Encoding.UTF8.GetString(info.Body));
                            satelliteOrbitRenderer.SetDisplayGroup(Enum.GetName(typeof(ExelSheetGroup), exelsheetindex), DisplayMode.SatelliteOnly);
                            ResetZ(ExelSheetGroupIndexToConstellationGroupIndex(exelsheetindex));
                            wxTabSwitcher.SwitchTab(-1);
                            theEarth.SetActive(true);
                            litVCR2.OpenVideoByFileName("汽车百年进化论地屏.mp4");
                        }

                        break;
                    case OrderTypeEnum.Reload:            //星座对比
                        {
                            Debug.Log((OrderTypeEnum)info.OrderType + "  " + (DataTypeEnum)info.DataType);
                            int year = JsonConvert.DeserializeObject<int>(Encoding.UTF8.GetString(info.Body));
                            if (Settings.ini.Game.ShowOrbitWhenPie)
                            {
                                satelliteOrbitRenderer.SetDisplayAll(year - 5, year, countrys);
                            }
                            else
                            {
                                satelliteOrbitRenderer.SetDisplayMode(DisplayMode.None);
                            }
                            satletExelDataReader.ShowYear(year);

                            litVCR2.OpenVideoByFileName("汽车百年进化论地屏.mp4");
                            //satelliteOrbitRenderer.SetDisplayMode(DisplayMode.SatelliteOnly);
                            //for (int i = 1; i < WeiXingGuangDian.transform.childCount; i++)
                            //{
                            //    WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(false);
                            //}
                            //if (index1 > 6 && index1 <= 13)
                            //{
                            //    WeiXingGuangDian.transform.GetChild(1).gameObject.SetActive(true);
                            //}
                            //if (index1 > 13 && index1 <= 16)
                            //{
                            //    for (int i = 0; i <= 2; i++)
                            //    {
                            //        WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            //    }
                            //}
                            //if (index1 == 17)
                            //{
                            //    for (int i = 0; i <= 3; i++)
                            //    {
                            //        WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            //    }
                            //}
                            //if (index1 == 18)
                            //{
                            //    for (int i = 0; i <= 4; i++)
                            //    {
                            //        WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            //    }
                            //}
                            //if (index1 == 19)
                            //{
                            //    for (int i = 0; i <= 5; i++)
                            //    {
                            //        WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            //    }
                            //}
                            //if (index1 == 20)
                            //{
                            //    for (int i = 0; i <= 6; i++)
                            //    {
                            //        WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            //    }
                            //}
                            //if (index1 == 21)
                            //{
                            //    for (int i = 0; i <= 7; i++)
                            //    {
                            //        WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            //    }
                            //}
                            //if (index1 == 22)
                            //{
                            //    for (int i = 0; i <= 8; i++)
                            //    {
                            //        WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            //    }
                            //}
                            //if (index1 == 23)
                            //{
                            //    for (int i = 0; i <= 9; i++)
                            //    {
                            //        WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            //    }
                            //}
                        }
                        break;
                    case OrderTypeEnum.DrawOrbit:
                        bool drawOrbit = JsonConvert.DeserializeObject<bool>(Encoding.UTF8.GetString(info.Body));
                        if (drawOrbit)
                        {
                            satelliteOrbitRenderer.SetDisplayMode(DisplayMode.Both);
                            leanPitchYaw.Camera.DOFieldOfView(37f, 1f);
                        }
                        else
                        {
                            satelliteOrbitRenderer.SetDisplayMode(DisplayMode.SatelliteOnly);
                            leanPitchYaw.Camera.DOFieldOfView(30f, 1f);
                        }
                        break;
                    case OrderTypeEnum.CountryFilterChange:
                        //countrys = JsonConvert.DeserializeObject<String>(Encoding.UTF8.GetString(info.Body));
                        //satelliteOrbitRenderer.SetCountryFilter(true, countrys);
                        //satelliteOrbitRenderer.RefreshData();
                        break;
                    case OrderTypeEnum.WeiXingDot:
                        Debug.Log((OrderTypeEnum)info.OrderType + "  " + (DataTypeEnum)info.DataType);
                        {
                            int year = JsonConvert.DeserializeObject<int>(Encoding.UTF8.GetString(info.Body));
                            Debug.Log(year);
                            ResetZ(0);
                            wxTabSwitcher.SwitchTab(-1);
                            theEarth.SetActive(true);

                            satelliteOrbitRenderer.SetBaseSatelliteScale(0.5f);
                            satelliteOrbitRenderer.SetDisplayMode(DisplayMode.SatelliteOnly);
                            satelliteOrbitRenderer.SetDisplayAll(1970, year);
                            litVCR1.OpenVideoByFileName("卫星待机循环动画.mp4");
                            litVCR2.OpenVideoByFileName("汽车百年进化论地屏.mp4");
                            //for (int i = 0; i < WeiXingGuangDian.transform.childCount; i++)
                            //{
                            //    int value = Mathf.CeilToInt(progressValue * 10);
                            //    Transform item = WeiXingGuangDian.transform.GetChild(i);
                            //    if (item != null)
                            //    {
                            //        if (value <= i)
                            //        {
                            //            item.gameObject.SetActive(false);
                            //        }
                            //        else
                            //        {
                            //            item.gameObject.SetActive(true);
                            //        }
                            //    }
                            //}
                        }
                        break;
                    case OrderTypeEnum.SetPlayMovie://汽车模型浏览
                        Debug.Log((OrderTypeEnum)info.OrderType + "  " + (DataTypeEnum)info.DataType);
                        int index2 = JsonConvert.DeserializeObject<int>(Encoding.UTF8.GetString(info.Body));
                        ShowCarModel(index2 + 1);
                        break;
                    case OrderTypeEnum.SetPlayMovieFolder: //弹窗视频 内饰视频
                        tabSwitcher_UI.Hide();
                        Debug.Log((OrderTypeEnum)info.OrderType + "  " + (DataTypeEnum)info.DataType);
                        tabSwitcher_Obj.Hide();
                        string cmd1 = JsonConvert.DeserializeObject<String>(Encoding.UTF8.GetString(info.Body));
                        //media_TanChuang.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, Path.Combine("汽车功能弹窗视频", cmd1 + ".mp4"));
                        litVCR1.OpenVideoByFileName(cmd1 + ".mp4");
                        //mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "汽车百年进化论地屏.mp4");
                        litVCR2.OpenVideoByFileName("汽车百年进化论地屏.mp4");
                        break;
                    case OrderTypeEnum.WeiXingView: //卫星视图 卫星展示
                        satelliteOrbitRenderer.SetDisplayMode(DisplayMode.None);
                        Debug.Log((OrderTypeEnum)info.OrderType + "  " + (DataTypeEnum)info.DataType);
                        string weixingraw = JsonConvert.DeserializeObject<String>(Encoding.UTF8.GetString(info.Body));

                        Debug.Log(weixingraw);
                        string[] weixingraws = weixingraw.Split('|');
                        if (weixingraw.Length >= 2)
                        {
                            tabSwitcher_Obj.SwitchTab(TabObjLabel.卫星展示);
                            tabSwitcher_UI.SwitchTab(TabUILabel.Panel_卫星展示);

                            string name = weixingraws[0]; //卫星名称
                            text_WX.SetText(name);
                            int weixingindex = int.Parse(weixingraws[1]);
                            wxTabSwitcher.Hide();
                            if (name.Contains(EarthGroup.一网.ToString()))
                            {
                                wxTabSwitcher.SwitchTab(EarthGroup.一网);
                            }
                            else if (name.Contains("伽利略"))
                            {
                                wxTabSwitcher.SwitchTab(EarthGroup.伽俐略);
                            }
                            else if (name.Contains("全球定位"))
                            {

                                if (name.Contains("A"))
                                {
                                    wxTabSwitcher.SwitchTab(EarthGroup.全球定位系统1);
                                }
                                else if (name.Contains("B"))
                                {
                                    wxTabSwitcher.SwitchTab(EarthGroup.全球定位系统2);
                                }
                                else //默认
                                {
                                    wxTabSwitcher.SwitchTab(EarthGroup.全球定位系统1);
                                }
                            }
                            else if (name.Contains("北斗一号A"))
                            {
                                wxTabSwitcher.SwitchTab(EarthGroup.北斗一号);
                            }
                            else if (name.Contains("北斗一号C"))
                            {
                                wxTabSwitcher.SwitchTab(EarthGroup.北斗三号);
                            }
                            else if (name.Contains("北斗一号B"))
                            {
                                wxTabSwitcher.SwitchTab(EarthGroup.北斗二号);
                            }
                            else if (name.Contains("北斗"))//默认
                            {
                                wxTabSwitcher.SwitchTab(EarthGroup.北斗一号);
                            }
                            else if (name.Contains(EarthGroup.千帆.ToString()))
                            {
                                wxTabSwitcher.SwitchTab(EarthGroup.千帆);
                            }
                            else if (name.Contains("多媒体卫星"))
                            {
                                wxTabSwitcher.SwitchTab(EarthGroup.千帆);
                            }
                            else if (name.StartsWith("飓风模型"))
                            {
                                wxTabSwitcher.SwitchTab(EarthGroup.格洛纳斯A);
                            }
                            else if (name.StartsWith("飓风"))
                            {
                                wxTabSwitcher.SwitchTab(EarthGroup.格洛纳斯B);
                            }
                            else if (name.All(char.IsDigit))
                            {
                                text_WX.SetText("星链-猎鹰九号");
                                wxTabSwitcher.SwitchTab(EarthGroup.星链);
                            }
                            else
                            {
                                //wxTabSwitcher.SwitchTab(weixingindex % wxTabSwitcher.tabPageGroups.Count);
                                wxTabSwitcher.Hide();
                                text_WX.SetText("无数据");
                            }



                            //动画 leanPitchYaw.Pitch 从0渐变到20f;
                            leanPitchYaw.Pitch = 0f;
                            DOTween.To(() => leanPitchYaw.Pitch, x => leanPitchYaw.Pitch = x, 30f, rotateDuration);
                            // 让 leanPitchYaw.Yaw 在 1 秒内从当前值增加 30f
                            DOTween.To(
                                () => leanPitchYaw.Yaw,
                                x => leanPitchYaw.Yaw = x,
                                leanPitchYaw.Yaw + 90f,
                                rotateDuration
                            );

                            camObj.DOFieldOfView(normalFOV, 0f).OnComplete(() =>
                            {
                                camObj.DOFieldOfView(smallFOV, 1f);
                            });
                            if (theEarth.transform.localPosition.y > -0.01f)
                            {
                                //将地球模型往下移动
                                theEarth.transform.DOMoveY(-4.5f, fovDuration).SetEase(Ease.OutQuad).OnComplete(() =>
                                {
                                    //当前mediaPlayer_2播放的不是该视频才播放
                                    string medianame = "地屏循环地球.mp4";
                                    //if (!mediaPlayer_2.m_VideoPath.Contains(medianame))
                                    //{
                                    //    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, medianame);
                                    //}
                                    litVCR2.OpenVideoByFileName(medianame);
                                    theEarth.transform.localPosition = new Vector3(0, 0, 0);
                                    theEarth.SetActive(false);
                                });
                            }
                        }


                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("ID:" + client.ID + "  " + ex.Message);
                Debug.LogException(ex);
            }
        });
    }


    void ReturnP1_1()
    {
        tabSwitcher_Obj.Hide();
        litVCR1.OpenVideoByFileName("待机循环粒子背景.mp4");
        tabSwitcher_UI.SwitchTab(TabUILabel.P1_1);
    }

    void ReturnP1_2()
    {
        tabSwitcher_Obj.Hide();
        litVCR1.OpenVideoByFileName("待机循环粒子背景.mp4");
        tabSwitcher_UI.SwitchTab(TabUILabel.P1_2);
    }

    private void ShowCarUI()
    {
        switch (curCarIndex)
        {
            case 1:
                tabSwitcher_UI.SwitchTab(TabUILabel.panel_level2_1_1);
                break;
            case 2:
                tabSwitcher_UI.SwitchTab(TabUILabel.panel_level2_1_2);
                break;
            case 3:
                tabSwitcher_UI.SwitchTab(TabUILabel.panel_level2_1_3);
                break;
            case 4:
                tabSwitcher_UI.SwitchTab(TabUILabel.panel_level2_1_4);
                break;
            case 5:
                tabSwitcher_UI.SwitchTab(TabUILabel.panel_level2_1_5);
                break;
        }
    }

    private void MainPageLoop()
    {
        litVCR1.SetLoopMode(LitVCR.LoopMode.one);
        litVCR2.SetLoopMode(LitVCR.LoopMode.one);
        tabSwitcher_Obj.Hide();
        tabSwitcher_UI.Hide();
        litVCR1.OpenVideoByFileName("待机循环动画.mp4");
        litVCR2.OpenVideoByFileName("地屏循环地球.mp4");
        satelliteOrbitRenderer.SetDisplayMode(DisplayMode.None);
    }

    private void ShowCarMode()
    {
        //media_Loop.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "光循环动画.mp4");
        litVCR1.OpenVideoByFileName("光循环动画.mp4");
        tabSwitcher_Obj.SwitchTab(TabObjLabel.车模);
    }

    float rotateDuration = 2f;
    float normalFOV = 60f; // 正常视角
    float smallFOV = 6f; //拉进视角
    float fovDuration = 1f; // 动画持续时间

    private void StopDoTween()
    {
        // 停止所有相关动画
        DOTween.Kill(leanPitchYaw);
        DOTween.Kill(camObj);
        DOTween.Kill(theEarth.transform);

        // 或者停止所有DOTween动画
        // DOTween.KillAll();
    }

    private void ShowCarModel(int index2)
    {
        carTabSwitcher.SwitchTab(curCarIndex.ToString());
        //mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, $"{index2}代俯视.mp4");
        //litVCR2.OpenVideoByFileName($"{index2}代俯视.mp4");
    }

    /// <summary>
    /// 卫星轨道移动位置 展示全 fieldofview
    /// </summary>
    /// <param name="index"></param>
    private void ResetZ(int index)
    {
        float z = 50;
        switch (index)
        {
            case 0:
                z = 40;
                break;
            case 1:
                z = 45f;
                break;
            case 2:
                z = 10f;
                break;
            case 3:
                z = 58f;
                break;
            case 4:
                z = 35f;
                break;
            case 5:
                z = 45f;
                break;
            case 6:
                z = 10f;
                break;
            case 7:
                z = 10f;
                break;
            default:
                break;
        }
        leanPitchYaw.Camera.DOFieldOfView(z, 1f);
    }

    /// <summary>
    /// ExelSheetGroup 的枚举值转换成字符串，然后在 ConstellationGroup 中查找相同名称的枚举，并返回其索引。
    /// </summary>
    /// <param name="exelIndex"></param>
    /// <returns></returns>
    public static int ExelSheetGroupIndexToConstellationGroupIndex(int exelIndex)
    {
        string exelName = Enum.GetName(typeof(ExelSheetGroup), exelIndex);
        Array constellationValues = Enum.GetValues(typeof(ConstellationGroup));
        for (int i = 0; i < constellationValues.Length; i++)
        {
            string constellationName = Enum.GetName(typeof(ConstellationGroup), i);
            if (constellationName == exelName)
            {
                return i;
            }
        }
        // 如果没找到，返回 -1
        return -1;
    }
}