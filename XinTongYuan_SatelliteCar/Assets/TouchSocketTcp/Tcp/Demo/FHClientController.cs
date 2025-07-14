using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class FHClientController : MonoBehaviour
{
    public FHTcpClient fhTcpClient;

    private bool exit;

    [SerializeField]
    private GameObject offLineStatue;

    DataTypeEnum dataTypeEnum = DataTypeEnum.LG20001;

    public Action<DTOInfo> receiveData;

    public string ipHost = "127.0.0.1:4849";
    bool mvSliderDown;
    bool EnableRepeatRequest = true;
    [SerializeField]
    Button btnPlay;
    [SerializeField]
    Button btnPause;
    [SerializeField]
    Button btnStop;
    //[SerializeField]
    //Button btnPrev;
    //[SerializeField]
    //Button btnNext;
    /// <summary>
    /// 当前播放视频名称
    /// </summary>
    [SerializeField]
    Text playingFilename;

    /// <summary>
    /// 视频总时长
    /// </summary>
    [SerializeField]
    Text totalTime;

    [SerializeField]
    Text playingTime;

    /// <summary>
    /// 音量Slider
    /// </summary>
    [SerializeField]
    EventSlider volumnSlider;

    /// <summary>
    /// 视频进度Slider
    /// </summary>
    [SerializeField]
    EventSlider movieSlider;

    /// <summary>
    /// 通过脚本绑定播控事件
    /// </summary>
    [SerializeField]
    private bool BindEvent = true;

    public GameObject P1_1;
    public GameObject P1_1_1;

    private void Update()
    {
    }

    // Start is called before the first frame update
    private void Start()
    {
        ipHost = File.ReadAllText(Application.streamingAssetsPath + @"\ipHost.txt");
        fhTcpClient = new FHTcpClient();

        fhTcpClient.InitConfig(ipHost);

        if (BindEvent)
        {
            //btnNext.onClick.AddListener(PlayNext);
            btnPause.onClick.AddListener(Pause);
            btnPlay.onClick.AddListener(Play);
            //btnPrev.onClick.AddListener(PlayPrev);
            btnStop.onClick.AddListener(Stop);


            volumnSlider.PointerUp.AddListener(VolumnSeek);
            movieSlider.PointerUp.AddListener(MovieSeek0);
            movieSlider.PointerDown.AddListener(MovieSliderPointerDown);
        }

        Loom.RunAsync(() =>
        {
            exit = false;
            while (!exit)
            {
                if (!fhTcpClient.IsOnline())
                {
                    fhTcpClient.StartConnect();
                }
                System.Threading.Thread.Sleep(1000);
            }
        });
        fhTcpClient.FHTcpClientReceive = ReceiveData;
        fhTcpClient.Connected += ((client) =>
        {

        });

        if (offLineStatue != null)
        {
            StartCoroutine(OfflineStatueView());
        }
    }

    private IEnumerator OfflineStatueView()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            bool isonline = fhTcpClient != null && fhTcpClient.IsOnline();
            offLineStatue.SetActive(!isonline);
        }
    }

    private void ReceiveData(DTOInfo dTOInfo)
    {
        receiveData?.Invoke(dTOInfo);
        Loom.QueueOnMainThread(() =>
        {
            try
            {

                Debug.Log((OrderTypeEnum)dTOInfo.OrderType + "  " + (DataTypeEnum)dTOInfo.DataType);

                switch ((OrderTypeEnum)dTOInfo.OrderType)
                {
                    case OrderTypeEnum.Str:
                        string panelstr = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(dTOInfo.Body));
                        switch (panelstr)
                        {
                            case "P1_1":
                                P1_1_1.SetActive(false);
                                P1_1.SetActive(true);
                                break;
                            case "MainPage":
                                Manager._ins.tabSwitcher.SwitchTab(TabUIMainType.Panel_level1);
                                break;
                        }
                        break;
                    case OrderTypeEnum.GetPlayInfo:
                        {
                            string playinfo = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(dTOInfo.Body));
                            //PlayInfo|9385.891,30037.33,0,2021重庆宣传片30秒.mp4
                            string head = "PlayInfo|";
                            //提取 9385.891,30037.33,0,2021重庆宣传片30秒.mp4 
                            if (!string.IsNullOrEmpty(playinfo) && playinfo.StartsWith(head))
                            {
                                playinfo = playinfo.Substring(head.Length);
                            }

                            string[] playinfoArray = playinfo.Split(',');
                            if (playinfoArray.Length == 4)
                            {
                                float curtime = float.Parse(playinfoArray[0]);
                                float totaltime = float.Parse(playinfoArray[1]);
                                int.TryParse(playinfoArray[2], out int index);
                                string curName = playinfoArray[3];
                                playingFilename.text = curName;

                                if (!mvSliderDown)
                                {
                                    if (curtime >= 0 && totaltime > 0)
                                    {
                                        movieSlider.value = curtime / totaltime;
                                    }
                                }

                                string totatimestr = time2str(totaltime);
                                totalTime.text = totatimestr;
                                string cursecstr = time2str(curtime);
                                playingTime.text = cursecstr;
                            }

                            //Debug.Log(playinfo);
                        }
                        break;
                    case OrderTypeEnum.GetCurMovieName:
                        string filename = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(dTOInfo.Body));
                        if (playingFilename.text != filename)
                        {
                            playingFilename.text = filename;
                        }
                        break;

                    case OrderTypeEnum.GetVolumn:
                        float volumn = JsonConvert.DeserializeObject<float>(Encoding.UTF8.GetString(dTOInfo.Body));
                        volumnSlider.value = volumn;
                        break;
                }
            }
            catch (Exception ex)
            {

                Debug.LogException(ex);
            }

        });
    }
    private static string time2str(float totaltime)
    {
        TimeSpan ts = new TimeSpan(0, 0, (int)(totaltime * 0.001));
        string sec = $"{ts.Hours.ToString().PadLeft(2, '0')}:{ts.Minutes.ToString().PadLeft(2, '0')}:{ts.Seconds.ToString().PadLeft(2, '0')}";
        return sec;
    }

    private void OnEnable()
    {
        StartCoroutine(InitComponent());
    }
    IEnumerator InitComponent()
    {
        while (fhTcpClient?.IsOnline() != true)
        {
            yield return null;
        }
        if (EnableRepeatRequest)
        {
            InvokeRepeating(nameof(RepeatRequest), 0, 1f);
            GetVolumn();
        }
    }
    private void OnDisable()
    {
        if (EnableRepeatRequest)
        {
            CancelInvoke(nameof(RepeatRequest));
        }
        exit = true;
        if (fhTcpClient != null)
        {
            fhTcpClient.Close();
        }
    }

    /// <summary>
    /// 循环请求进度
    /// </summary>
    void RepeatRequest()
    {
        try
        {
            Send(dataTypeEnum, OrderTypeEnum.GetPlayInfo, "");
        }
        catch (Exception ex)
        {
            Debug.Log("RepeatRequest:" + ex.Message);
        }

    }

    /// <summary>
    /// 播放上一个
    /// </summary>
    public void PlayPrev()
    {
        Send(dataTypeEnum, OrderTypeEnum.PlayPrev, "");
    }
    /// <summary>
    /// 播放下一个
    /// </summary>
    public void PlayNext()
    {
        Send(dataTypeEnum, OrderTypeEnum.PlayNext, "");
    }

    /// <summary>
    /// 停止
    /// </summary>
    public void Stop()
    {
        Send(dataTypeEnum, OrderTypeEnum.StopMovie, "");
    }

    /// <summary>
    /// 播放
    /// </summary>
    public void Play()
    {
        Send(dataTypeEnum, OrderTypeEnum.PlayMovie, "");
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public void Pause()
    {
        Send(dataTypeEnum, OrderTypeEnum.PauseMovie, "");
    }

    /// <summary>
    /// 播放指定视频
    /// </summary>
    /// <param name="moviename">视频文件名</param>
    public void PlayMovieByName(string moviename)
    {
        Send(dataTypeEnum, OrderTypeEnum.SetPlayMovie, moviename);
    }

    /// <summary>
    /// 拖动进度条 鼠标抬起
    /// </summary>
    /// <param name="value"></param>
    public void MovieSeek0()
    {
        Send(dataTypeEnum, OrderTypeEnum.SetMovSeek, movieSlider.value);
        mvSliderDown = false;
        Debug.Log("mvSliderSeek0");
    }

    public void MovieSeek()
    {
        Debug.Log("未知触发:MovieSeek()");
    }
    /// <summary>
    /// 拖动进度条 鼠标按下
    /// </summary>
    public void MovieSliderPointerDown()
    {
        mvSliderDown = true;
    }

    /// <summary>
    /// 设置音量
    /// </summary>
    /// <param name="value"></param>
    public void VolumnSeek()
    {
        Send(dataTypeEnum, OrderTypeEnum.SetVolumn, volumnSlider.value);
        Debug.Log("VolumnSliderSeek");
    }

    /// <summary>
    /// 获取播放列表
    /// </summary>
    public void GetFileList()
    {
        Send(dataTypeEnum, OrderTypeEnum.GetFileList, "获取播放列表");
    }
    public void GetVolumn()
    {
        Send(dataTypeEnum, OrderTypeEnum.GetVolumn, "获取音量");
    }

    /// <summary>
    /// 转发对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dataTypeEnum"></param>
    /// <param name="orderTypeEnum"></param>
    /// <param name="data"></param>
    public void Send<T>(DataTypeEnum dataTypeEnum, OrderTypeEnum orderTypeEnum, T data)
    {
        fhTcpClient?.Send(dataTypeEnum, orderTypeEnum, data);
    }

    public void Send<T>(OrderTypeEnum orderTypeEnum, T data)
    {
        fhTcpClient?.Send(DataTypeEnum.S_MainHostOld, orderTypeEnum, data);
    }

    public void Send(OrderTypeEnum orderTypeEnum)
    {
        fhTcpClient?.Send(DataTypeEnum.S_MainHostOld, orderTypeEnum, "");
    }

    public void SendHex(DataTypeEnum dataTypeEnum, OrderTypeEnum orderTypeEnum, string obj)
    {
        fhTcpClient.SendHexStr(dataTypeEnum, orderTypeEnum, obj);
    }

}