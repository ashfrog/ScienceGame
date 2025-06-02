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

    //public GameObject Panel_level1_1_1, Panel_level1_1_2, Panel_level1_1_3, Panel_level1_2, Panel_level1_2_1, Panel_level1_2_2, panel_level1_2_3, Panel_卫星在空姿态, Panel_LoopVideo, video_car, panel_TanChuangVideo;
    public MediaPlayer media, media_Loop, media_Car, media_TanChuang;

    /// <summary>
    /// 地坪视频播放器
    /// </summary>
    public MediaPlayer mediaPlayer_2;
    public Sprite[] sprites_Introduce;   //介绍图组
    public Image img_Introduce;  //图片介绍组件
    public GameObject WeiXingGuangDian;  //卫星光点
    public GameObject[] cars;
    public GameObject oribit; //卫星轨道模型
    public GameObject[] moons; //卫星光点组
    public GameObject theEarth; //地球模型


    public enum TabUILabel //需要Inspector中TabSwitcher的allTabTypes保持一致
    {
        P1_1,
        P1_1_1,
        P1_1_2,
        P1_1_3,
        P1_2,
        P1_2_1,
        P1_2_2,
        P1_2_3,
        P循环屏保,
        P汽车百年进化论,
        P弹窗视频,
        卫星在空姿态,
    }
    public TabSwitcher tabSwitcher_UI;
    public enum TabObjLabel
    {
        地球,
        年份卫星数量,
        卫星展示,
        卫星轨道,
        车模
    }
    public TabSwitcher tabSwitcher_Obj;


    public enum EarthGroup
    {
        北斗卫星A,
        北斗卫星B,
        北斗卫星C,
        GPS卫星A,
        GPS卫星B,
        伽利略卫星,
        千帆,
        星链,
        一网
    }
    [SerializeField]
    private TabSwitcher carTabSwitcher;
    [SerializeField]
    private TabSwitcher wxTabSwitcher;

    private int curCarIndex;

    [SerializeField]
    Camera camObj;


    private float defaultCameraFieldofView;


    // Start is called before the first frame update
    void Start()
    {
        defaultCameraFieldofView = camObj.fieldOfView;
        tabSwitcher_UI.SwitchTab(TabUILabel.P循环屏保.ToString());
        StartCoroutine(WaitForTcpServiceInitialization());
        media_Car.Events.AddListener(OnVideoEvent);

        Settings.ini.Game.ZSpeed = Settings.ini.Game.ZSpeed;
    }

    public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        switch (et)
        {

            case MediaPlayerEvent.EventType.FinishedPlaying:
                //Panel_LoopVideo.SetActive(true);
                //video_car.SetActive(false);
                tabSwitcher_UI.SwitchTab(TabUILabel.P循环屏保);
                media_Loop.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "天屏汽车循环.mp4");
                break;
        }

        Debug.Log("Event: " + et.ToString());
    }

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
    }

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
            // 处理接收到的消息
            try
            {
                var info = requestInfo as DTOInfo;
                //Debug.Log((OrderTypeEnum)info.OrderType + "  " + (DataTypeEnum)info.DataType);
                switch ((OrderTypeEnum)info.OrderType)
                {
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
                    case OrderTypeEnum.SetMovSeek:
                        {
                            string cmdstr = JsonConvert.DeserializeObject<String>(Encoding.UTF8.GetString(info.Body));

                            string[] cmdstrs = cmdstr.Split('|');

                            string cmd = cmdstrs[0];

                            string cmdparam = "";
                            if (cmdstrs.Length > 1)
                            {
                                cmdparam = cmdstrs[1];
                            }
                            print(cmd + " " + cmdparam);
                            switch (cmd)
                            {
                                case "卫星":

                                    break;
                                case "发射展示":  //1-1 卫星发射展示
                                    tabSwitcher_Obj.Hide();
                                    tabSwitcher_UI.SwitchTab(TabUILabel.P1_1_1);
                                    PlayVideo("发射展示.mp4");
                                    //Panel_LoopVideo.SetActive(false);
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "地屏循环地球.mp4");
                                    break;
                                case "发射展示返回"://1-1返回 
                                    tabSwitcher_UI.SwitchTab(TabUILabel.P循环屏保);
                                    tabSwitcher_Obj.Hide();
                                    //Panel_level1_1_1.SetActive(false);
                                    media.Control.Rewind();
                                    media.Pause();
                                    //Panel_LoopVideo.SetActive(true);
                                    media_Loop.Play();
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "汽车百年进化论地屏.mp4");
                                    break;
                                case "星座展示"://1-2 卫星星座展示
                                    tabSwitcher_UI.SwitchTab(TabUILabel.P1_1_2);
                                    tabSwitcher_Obj.SwitchTab(TabObjLabel.地球);
                                    for (int i = 1; i < moons.Length; i++)
                                    {
                                        moons[i].SetActive(false);
                                    }
                                    //tabSwitcher_Obj.SwitchTab(TabObjLabel.地球);
                                    //Panel_level1_1_2.SetActive(true);
                                    //obj = theEarth;
                                    //oribit.gameObject.SetActive(true);
                                    //Panel_LoopVideo.SetActive(false);
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "循环地屏2.mp4");
                                    break;
                                case "星座展示返回"://1-2 返回 选项界面
                                    StopDoTween();
                                    //Panel_level1_1_2.SetActive(false);
                                    tabSwitcher_UI.SwitchTab(TabUILabel.P循环屏保);
                                    tabSwitcher_Obj.Hide();
                                    oribit.gameObject.SetActive(false);
                                    for (int i = 0; i < moons.Length; i++)
                                    {
                                        moons[i].SetActive(true);
                                    }
                                    //Panel_LoopVideo.SetActive(true);
                                    media_Loop.Play();
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "汽车百年进化论地屏.mp4");
                                    break;
                                case "星座对比"://1-3 卫星在空姿态
                                    //Panel_level1_1_3.SetActive(true);
                                    tabSwitcher_UI.SwitchTab(TabUILabel.卫星在空姿态);
                                    tabSwitcher_Obj.SwitchTab(TabObjLabel.卫星展示);
                                    WeiXingGuangDian.SetActive(true);
                                    //obj = theEarth;
                                    oribit.gameObject.SetActive(true);
                                    //Panel_LoopVideo.SetActive(false);
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "汽车百年进化论地屏.mp4");
                                    ResetZ(4);



                                    break;
                                case "星座对比返回"://1-3-1返回 （1-3卫星在空姿态）
                                    StopDoTween();
                                    //Panel_level1_1_3.SetActive(true);
                                    //Panel_卫星在空姿态.SetActive(false);
                                    tabSwitcher_Obj.Hide();
                                    WeiXingGuangDian.SetActive(true);
                                    break;
                                case "在空姿态"://1-3-1 内外星座对比
                                    //Panel_level1_1_3.SetActive(false);
                                    //Panel_卫星在空姿态.SetActive(true);
                                    WeiXingGuangDian.SetActive(false);
                                    //obj = theEarth;
                                    break;
                                case "在空姿态返回":// 1-3返回 1
                                    StopDoTween();
                                    //Panel_level1_1_3.SetActive(false);
                                    tabSwitcher_UI.SwitchTab(TabUILabel.P循环屏保);
                                    tabSwitcher_Obj.Hide();
                                    theEarth.transform.localPosition = new Vector3(0, 0, 0); //重置地球位置
                                    camObj.fieldOfView = defaultCameraFieldofView; //重置相机视角
                                    for (int i = 1; i < WeiXingGuangDian.transform.childCount; i++)
                                    {
                                        WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(false);
                                    }
                                    //Panel_LoopVideo.SetActive(true);
                                    media_Loop.Play();
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "汽车百年进化论地屏.mp4");
                                    break;
                                case "汽车":
                                    tabSwitcher_Obj.Hide();
                                    tabSwitcher_UI.SwitchTab(TabUILabel.P汽车百年进化论);
                                    //video_car.SetActive(true);
                                    media_Car.GetComponent<MediaPlayer>().Rewind(true);
                                    media_Car.GetComponent<MediaPlayer>().Play();
                                    //Panel_LoopVideo.SetActive(false);
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "汽车百年进化论地屏.mp4");

                                    break;
                                case "汽车返回":
                                    //Panel_LoopVideo.SetActive(true);
                                    //video_car.SetActive(false);
                                    media_Loop.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "待机循环动画.mp4");
                                    break;
                                case "汽车模型":
                                    //Panel_level1_2_1.SetActive(true);
                                    //Panel_level1_2_2.SetActive(false);
                                    //panel_level1_2_3.SetActive(false);
                                    //panel_TanChuangVideo.SetActive(false);
                                    //camTabSwitcher.SwitchTab((int)CamGroup.全息);
                                    //oribit.gameObject.SetActive(false);
                                    tabSwitcher_UI.Hide();
                                    break;
                                case "汽车模型返回":
                                    break;
                                case "汽车街景":
                                    int.TryParse(cmdparam, out curCarIndex);
                                    tabSwitcher_Obj.SwitchTab(TabObjLabel.车模);
                                    tabSwitcher_UI.Hide();
                                    ShowCarModel(curCarIndex);
                                    break;
                                case "汽车街景返回":
                                    tabSwitcher_UI.SwitchTab(TabUILabel.P循环屏保);
                                    for (int i = 0; i < cars.Length; i++)
                                    {
                                        cars[i].SetActive(false);
                                    }
                                    media_Loop.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "天屏屏保.mp4");
                                    break;
                                case "汽车内饰":
                                    //Panel_level1_2_1.SetActive(false);
                                    //Panel_level1_2_2.SetActive(false);
                                    //panel_level1_2_3.SetActive(true);
                                    break;
                                case "汽车内饰返回":
                                    carTabSwitcher.Hide();
                                    tabSwitcher_UI.SwitchTab(TabUILabel.P循环屏保);
                                    media_Loop.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "天屏汽车循环.mp4");
                                    Debug.Log("播放 火箭发射地屏");
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "火箭发射地屏.mp4");
                                    break;
                            }
                        }
                        break;
                    case OrderTypeEnum.PlayMovie:
                        media.Play();
                        break;
                    case OrderTypeEnum.PauseMovie:
                        media.Pause();
                        break;
                    case OrderTypeEnum.LoadUrl:          //星座展示 选星座
                        int index = JsonConvert.DeserializeObject<int>(Encoding.UTF8.GetString(info.Body));
                        img_Introduce.sprite = sprites_Introduce[index];
                        for (int i = 0; i < moons.Length; i++)
                        {
                            moons[i].SetActive(false);
                        }
                        moons[index].SetActive(true);
                        ResetZ(index);
                        break;
                    case OrderTypeEnum.Reload:            //星座对比
                        int index1 = JsonConvert.DeserializeObject<int>(Encoding.UTF8.GetString(info.Body));
                        for (int i = 1; i < WeiXingGuangDian.transform.childCount; i++)
                        {
                            WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(false);
                        }
                        if (index1 > 6 && index1 <= 13)
                        {
                            WeiXingGuangDian.transform.GetChild(1).gameObject.SetActive(true);
                        }
                        if (index1 > 13 && index1 <= 16)
                        {
                            for (int i = 0; i <= 2; i++)
                            {
                                WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            }
                        }
                        if (index1 == 17)
                        {
                            for (int i = 0; i <= 3; i++)
                            {
                                WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            }
                        }
                        if (index1 == 18)
                        {
                            for (int i = 0; i <= 4; i++)
                            {
                                WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            }
                        }
                        if (index1 == 19)
                        {
                            for (int i = 0; i <= 5; i++)
                            {
                                WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            }
                        }
                        if (index1 == 20)
                        {
                            for (int i = 0; i <= 6; i++)
                            {
                                WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            }
                        }
                        if (index1 == 21)
                        {
                            for (int i = 0; i <= 7; i++)
                            {
                                WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            }
                        }
                        if (index1 == 22)
                        {
                            for (int i = 0; i <= 8; i++)
                            {
                                WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            }
                        }
                        if (index1 == 23)
                        {
                            for (int i = 0; i <= 9; i++)
                            {
                                WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(true);
                            }
                        }

                        break;
                    case OrderTypeEnum.WeiXingDot:
                        {
                            float progressValue = JsonConvert.DeserializeObject<float>(Encoding.UTF8.GetString(info.Body));
                            Debug.Log(progressValue);

                            for (int i = 0; i < WeiXingGuangDian.transform.childCount; i++)
                            {
                                int value = Mathf.CeilToInt(progressValue * 10);
                                Transform item = WeiXingGuangDian.transform.GetChild(i);
                                if (item != null)
                                {
                                    if (value <= i)
                                    {
                                        item.gameObject.SetActive(false);
                                    }
                                    else
                                    {
                                        item.gameObject.SetActive(true);
                                    }
                                }
                            }
                        }
                        break;
                    case OrderTypeEnum.SetPlayMovie://汽车模型浏览
                        int index2 = JsonConvert.DeserializeObject<int>(Encoding.UTF8.GetString(info.Body));
                        ShowCarModel(index2);
                        break;
                    case OrderTypeEnum.SetPlayMovieFolder: //弹窗视频 内饰视频
                        tabSwitcher_UI.SwitchTab(TabUILabel.P弹窗视频);
                        string cmd1 = JsonConvert.DeserializeObject<String>(Encoding.UTF8.GetString(info.Body));
                        media_TanChuang.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, cmd1 + ".mp4");
                        mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "汽车百年进化论地屏.mp4");
                        break;
                    case OrderTypeEnum.WeiXingView: //卫星视图 卫星展示
                        string weixingraw = JsonConvert.DeserializeObject<String>(Encoding.UTF8.GetString(info.Body));
                        Debug.Log(weixingraw);
                        string[] weixingraws = weixingraw.Split('|');
                        if (weixingraw.Length >= 2)
                        {
                            string name = weixingraws[0]; //卫星名称
                            int weixingindex = int.Parse(weixingraws[1]);
                            Debug.Log(name + " " + weixingindex);
                            tabSwitcher_Obj.SwitchTab(TabObjLabel.卫星展示);
                            wxTabSwitcher.SwitchTab(weixingindex % wxTabSwitcher.tabPageGroups.Count);

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
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "地屏循环地球.mp4");
                                });
                            }
                        }


                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("ID:" + client.ID + "  " + ex.Message);
            }
        });
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
        mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, $"{index2 + 1}代俯视.mp4");
    }

    /// <summary>
    /// 卫星轨道移动位置 展示全
    /// </summary>
    /// <param name="index"></param>
    private void ResetZ(int index)
    {
        float z = 50;
        switch (index)
        {
            case 0:
                z = 60;
                break;
            case 1:
                z = 60f;
                break;
            case 2:
                z = 60f;
                break;
            case 3:
                z = 60f;
                break;
            case 4:
                z = 70f;
                break;
            case 5:
                z = 60f;
                break;
            case 6:
                z = 60f;
                break;
            case 7:
                z = 60f;
                break;
            default:
                break;
        }
        leanPitchYaw.Camera.DOFieldOfView(z, 0.5f);
    }

    private void PlayVideo(string str)
    {
        tabSwitcher_UI.SwitchTab(TabUILabel.P1_1_1);
        media.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, str);
        media.Play();
    }
}