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

    [SerializeField]
    GameObject obj;

    [SerializeField, Range(0.1f, 10f)]
    float rotationSpeed = 5f; // 控制旋转的平滑度

    [SerializeField, Range(0.1f, 1f)]
    float deltaScale = 0.2f;

    [SerializeField]
    float minVerticalAngle = -90f; // 垂直旋转的最小角度（向下）

    [SerializeField]
    float maxVerticalAngle = 90f; // 垂直旋转的最大角度（向上）

    // 目标增量旋转值
    private Vector2 targetRotationDelta = Vector2.zero;

    public GameObject Panel_level1_1_1, Panel_level1_1_2, Panel_level1_1_3, Panel_level1_2, Panel_level1_2_1, Panel_level1_2_2, panel_level1_2_3, Panel_卫星在空姿态, Panel_LoopVideo, video_car, panel_TanChuangVideo;
    public MediaPlayer media, media_Loop, media_Car, media_TanChuang, media_Quanxi, mediaPlayer_2;
    public Sprite[] sprites_Introduce;   //介绍图组
    public Image img_Introduce;  //图片介绍组件
    public GameObject WeiXingGuangDian;  //卫星光点
    public GameObject[] cars;
    public GameObject theEarth; //地球模型
    public GameObject[] moons; //卫星轨道组
    public enum CamGroup
    {
        normal = 0,
        holo = 1
    }
    [SerializeField]
    private TabSwitcher camTabSwitcher;

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
    private TabSwitcher earthTabSwitcher;

    private int curCarIndex;




    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForTcpServiceInitialization());
        media_Car.Events.AddListener(OnVideoEvent);

        Settings.ini.Game.ZSpeed = Settings.ini.Game.ZSpeed;
    }

    public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        switch (et)
        {

            case MediaPlayerEvent.EventType.FinishedPlaying:
                Panel_LoopVideo.SetActive(true);
                video_car.SetActive(false);
                media_Loop.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "天屏汽车循环.mp4");
                break;
        }

        Debug.Log("Event: " + et.ToString());
    }
    // 定义平滑系数 (0-1之间，越小越平滑)
    float smoothFactor = 0.01f;
    // Update is called once per frame
    void Update()
    {
        //只有当有旋转增量需要应用时才进行旋转
        if (targetRotationDelta.sqrMagnitude > 0.001f)
        {
            leanPitchYaw.Pitch -= targetRotationDelta.y * deltaScale * smoothFactor;
            leanPitchYaw.Yaw += targetRotationDelta.x * deltaScale * smoothFactor;

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
                switch ((OrderTypeEnum)info.OrderType)
                {
                    case OrderTypeEnum.Rotate:
                        {
                            string v2 = JsonConvert.DeserializeObject<String>(Encoding.UTF8.GetString(info.Body)); //v2
                                                                                                                   //V2 为逗号分割的字符串，第一个为x轴旋转角度增量，第二个为y轴旋转角度增量

                            string[] v2s = v2.Split(',');
                            vec2 = new Vector2(float.Parse(v2s[0]), float.Parse(v2s[1]));
                            Debug.Log(vec2);
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
                                case "发射展示":
                                    PlayVideo("发射展示.mp4");
                                    Panel_LoopVideo.SetActive(false);
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "地屏循环地球.mp4");
                                    break;
                                case "发射展示返回":
                                    Panel_level1_1_1.SetActive(false);
                                    media.Control.Rewind();
                                    media.Pause();
                                    Panel_LoopVideo.SetActive(true);
                                    media_Loop.Play();
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "待机循环地屏.mp4");
                                    break;
                                case "星座展示":
                                    for (int i = 1; i < moons.Length; i++)
                                    {
                                        moons[i].SetActive(false);
                                    }
                                    Panel_level1_1_2.SetActive(true);
                                    obj = theEarth;
                                    obj.transform.parent.gameObject.SetActive(true);
                                    Panel_LoopVideo.SetActive(false);
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "循环地屏2.mp4");
                                    break;
                                case "星座展示返回":
                                    Panel_level1_1_2.SetActive(false);
                                    obj.transform.parent.gameObject.SetActive(false);
                                    for (int i = 0; i < moons.Length; i++)
                                    {
                                        moons[i].SetActive(true);
                                    }
                                    Panel_LoopVideo.SetActive(true);
                                    media_Loop.Play();
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "待机循环地屏.mp4");
                                    break;
                                case "星座对比":
                                    Panel_level1_1_3.SetActive(true);
                                    WeiXingGuangDian.SetActive(true);
                                    obj = theEarth;
                                    obj.transform.parent.gameObject.SetActive(true);
                                    Panel_LoopVideo.SetActive(false);
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "汽车百年进化论地屏.mp4");
                                    ResetZ(4);



                                    break;
                                case "星座对比返回":
                                    Panel_level1_1_3.SetActive(true);
                                    Panel_卫星在空姿态.SetActive(false);
                                    WeiXingGuangDian.SetActive(true);
                                    break;
                                case "在空姿态":
                                    Panel_level1_1_3.SetActive(false);
                                    Panel_卫星在空姿态.SetActive(true);
                                    WeiXingGuangDian.SetActive(false);
                                    obj = theEarth;
                                    break;
                                case "在空姿态返回":
                                    Panel_level1_1_3.SetActive(false);
                                    WeiXingGuangDian.SetActive(false);
                                    obj.transform.parent.gameObject.SetActive(false);
                                    for (int i = 1; i < WeiXingGuangDian.transform.childCount; i++)
                                    {
                                        WeiXingGuangDian.transform.GetChild(i).gameObject.SetActive(false);
                                    }
                                    Panel_LoopVideo.SetActive(true);
                                    media_Loop.Play();
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "待机循环地屏.mp4");
                                    break;
                                case "汽车":
                                    video_car.SetActive(true);
                                    media_Car.GetComponent<MediaPlayer>().Rewind(true);
                                    media_Car.GetComponent<MediaPlayer>().Play();
                                    Panel_LoopVideo.SetActive(false);
                                    mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "汽车百年进化论地屏.mp4");

                                    break;
                                case "汽车返回":
                                    Panel_LoopVideo.SetActive(true);
                                    video_car.SetActive(false);
                                    media_Loop.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "待机循环动画.mp4");
                                    break;
                                case "汽车模型":
                                    Panel_level1_2_1.SetActive(true);
                                    Panel_level1_2_2.SetActive(false);
                                    panel_level1_2_3.SetActive(false);
                                    panel_TanChuangVideo.SetActive(false);
                                    camTabSwitcher.SwitchTab((int)CamGroup.normal);
                                    break;
                                case "汽车模型返回":
                                    Panel_level1_2_1.SetActive(false);
                                    Panel_level1_2_2.SetActive(true);
                                    for (int i = 0; i < cars.Length; i++)
                                    {
                                        cars[i].SetActive(false);
                                    }
                                    panel_TanChuangVideo.SetActive(true);
                                    media_TanChuang.Rewind(true);
                                    media_TanChuang.Play();
                                    camTabSwitcher.SwitchTab((int)CamGroup.holo);
                                    media_Quanxi.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, $"汽车街景{curCarIndex}.mp4");
                                    break;
                                case "汽车街景":
                                    camTabSwitcher.SwitchTab((int)CamGroup.holo);
                                    Panel_level1_2_1.SetActive(false);
                                    Panel_level1_2_2.SetActive(true);
                                    panel_level1_2_3.SetActive(false);
                                    video_car.SetActive(false);
                                    Panel_LoopVideo.SetActive(false);
                                    panel_TanChuangVideo.SetActive(true);
                                    //media_TanChuang.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, cmd + cmdparam + ".mp4");
                                    int.TryParse(cmdparam, out int generation);
                                    if (generation > 0)
                                    {
                                        media_Quanxi.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, cmd + cmdparam + ".mp4");
                                        curCarIndex = generation;
                                    }

                                    break;
                                case "汽车街景返回":
                                    Panel_level1_2_2.SetActive(false);
                                    for (int i = 0; i < cars.Length; i++)
                                    {
                                        cars[i].SetActive(false);
                                    }
                                    panel_TanChuangVideo.SetActive(false);
                                    Panel_LoopVideo.SetActive(true);
                                    media_Loop.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "天屏屏保.mp4");
                                    camTabSwitcher.SwitchTab((int)CamGroup.normal);
                                    break;
                                case "汽车内饰":
                                    Panel_level1_2_1.SetActive(false);
                                    Panel_level1_2_2.SetActive(false);
                                    panel_level1_2_3.SetActive(true);
                                    break;
                                case "汽车内饰返回":
                                    Panel_level1_2_2.SetActive(false);
                                    for (int i = 0; i < cars.Length; i++)
                                    {
                                        cars[i].SetActive(false);
                                    }
                                    panel_TanChuangVideo.SetActive(false);
                                    Panel_LoopVideo.SetActive(true);
                                    media_Loop.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "天屏汽车循环.mp4");
                                    camTabSwitcher.SwitchTab((int)CamGroup.normal);
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
                        for (int i = 0; i < cars.Length; i++)
                        {
                            cars[i].SetActive(false);
                        }
                        obj = cars[index2].transform.GetChild(0).gameObject;
                        cars[index2].SetActive(true);

                        mediaPlayer_2.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, $"{index2 + 1}代俯视.mp4");
                        break;
                    case OrderTypeEnum.SetPlayMovieFolder:
                        string cmd1 = JsonConvert.DeserializeObject<String>(Encoding.UTF8.GetString(info.Body));
                        media_TanChuang.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, cmd1 + ".mp4");
                        camTabSwitcher.SwitchTab((int)CamGroup.normal);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("ID:" + client.ID + "  " + ex.Message);
            }
        });
    }
    /// <summary>
    /// 卫星轨道移动位置 展示全
    /// </summary>
    /// <param name="index"></param>
    private void ResetZ(int index)
    {
        float z = 60;
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
                z = 60f;
                break;
            case 5:
                z = 70f;
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
        leanPitchYaw.Camera.DOFieldOfView(z, 2f);
    }

    private void PlayVideo(string str)
    {
        Panel_level1_1_1.SetActive(true);
        media.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, str);
        media.Play();
    }
}