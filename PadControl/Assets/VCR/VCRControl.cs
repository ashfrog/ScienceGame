using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TouchSocket.Core.XREF.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class VCRControl : MonoBehaviour, IVCRControl
{
    /// <summary>
    /// 开启循环发送请求
    /// </summary>
    [SerializeField]
    bool EnableRepeatRequest;
    /// <summary>
    /// 发送组类型
    /// </summary>
    [SerializeField]
    DataTypeEnum dataTypeEnum;

    /// <summary>
    /// 接收组类型 需要区分开不同控制设备用 S_Pad50-S_Pad57等区分开 播放器需要配置相同编号50-57
    /// </summary>
    [SerializeField]
    DataTypeEnum itemDataType = DataTypeEnum.S_Pad;

    [SerializeField]
    Button btnPlay;
    [SerializeField]
    Button btnPause;
    [SerializeField]
    Button btnStop;
    [SerializeField]
    Button btnPrev;
    [SerializeField]
    Button btnNext;
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

    Transform btnFileroot;
    Transform playBtnFileRoot;
    ///// <summary>
    ///// 播放传回列表prefab
    ///// </summary>
    //[SerializeField]
    //Button btnFilePrefab;
    /// <summary>
    /// 音量Slider
    /// </summary>
    [SerializeField]
    SliderEvent volumnSlider;

    /// <summary>
    /// 视频进度Slider
    /// </summary>
    [SerializeField]
    SliderEvent movieSlider;

    /// <summary>
    /// 展开列表
    /// </summary>
    [SerializeField]
    Button btn_Expand;

    private Button playButtonPrefab; //列表底部 ScrollRect 的带文件名播放按钮

    /// <summary>
    /// 通过脚本绑定播控事件
    /// </summary>
    [SerializeField]
    private bool BindEvent = true;

    /// <summary>
    /// 拖动视频进度条的时候 进度条不通过接受消息改变
    /// </summary>
    private bool mvSliderDown = false;

    private AudioSource audioSource;

    /// <summary>
    /// 单个网页
    /// </summary>
    public bool singleUrl = true;

    [SerializeField]
    TMPro.TMP_Dropdown tMP_LoopModeDropdown;
    public enum LoopMode
    {
        none,
        one,
        all
    }
    // Start is called before the first frame update
    private void Start()
    {
        //btnFileroot = btnFilePrefab.transform.parent;//分离prefab
        //btnFilePrefab.transform.SetParent(null);

        if (BindEvent)
        {
            btnNext.onClick.AddListener(PlayNext);
            btnPause.onClick.AddListener(Pause);
            btnPlay.onClick.AddListener(Play);
            btnPrev.onClick.AddListener(PlayPrev);
            btnStop.onClick.AddListener(Stop);
            btn_Expand.onClick.AddListener(() =>
            {
                FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.GetUrls, "");
                if (singleUrl)
                {
                    SendShowBrowser();
                }
                else
                {
                    if (btnFileroot != null)
                    {
                        try
                        {
                            var scrollview = btnFileroot.parent.parent.gameObject;
                            scrollview.SetActive(!scrollview.activeSelf);
                        }
                        catch (Exception ex)
                        {
                            Debug.Log("展开:" + ex.Message);
                        }
                    }
                }
            });
            tMP_LoopModeDropdown.onValueChanged.AddListener((index) =>
            {
                switch (index)
                {
                    case 0:
                        FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.LoopMode, LoopMode.none);
                        break;
                    case 1:
                        FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.LoopMode, LoopMode.one);
                        break;
                    case 2:
                        FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.LoopMode, LoopMode.all);
                        break;
                }
            });
            volumnSlider.PointerUp.AddListener(VolumnSeek);
            movieSlider.PointerUp.AddListener(MovieSeek0);
            movieSlider.PointerDown.AddListener(MovieSliderPointerDown);
        }


        playButtonPrefab = transform.parent.GetComponentInChildren<PlayButton>().GetComponent<Button>();//分离ScrollRect  的列表播放按钮项
        playBtnFileRoot = playButtonPrefab.transform.parent;
        playButtonPrefab.transform.SetParent(null);
        var audio = GameObject.Find("Audio Source");
        if (audio != null)
        {
            audioSource = audio.GetComponent<AudioSource>();
        }

        if (FHClientController.ins != null)
        {
            FHClientController.ins.Connected += (client) =>
            {
                if (EnableRepeatRequest)
                {
                    GetFileList();
                    GetVolumn();
                }

            };
        }
    }
    public void PlayClickSound()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    private void OnEnable()
    {
        HideUrlScrollView();

        StartCoroutine(InitComponent());
    }

    bool isonline = false;
    IEnumerator InitComponent()
    {

        while (!isonline)
        {
            isonline = FHClientController.ins != null
                && FHClientController.ins.fhTcpClient != null
                && FHClientController.ins.fhTcpClient.IsOnline();
            yield return null;
        }

        if (FHClientController.ins != null && FHClientController.ins.fhTcpClient != null)
        {
            FHClientController.ins.fhTcpClient.FHTcpClientReceive += FHTcpClientReceive;
        }

        if (EnableRepeatRequest)
        {
            InvokeRepeating(nameof(RepeatRequest), 0, 1f);
            GetFileList();
            GetVolumn();
            GetLoopMode();
        }
    }

    private void HideUrlScrollView()
    {
        try
        {
            if (btnFileroot != null)
            {
                var scrollview = btnFileroot.parent.parent.gameObject;
                scrollview.gameObject.SetActive(false);
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public void SendShowBrowser(string name = "")
    {
        FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.Browser, name);
        PlayClickSound();
    }


    void ExpandFileList()
    {
        GameObject scrollList = btnFileroot.parent.parent.gameObject;
        scrollList.SetActive(!scrollList.activeSelf);
    }

    void FHTcpClientReceive(DTOInfo dTOInfo)
    {
        //Debug.Log(dTOInfo.DataType);
        if (dTOInfo.DataType == (int)itemDataType)
        {
            switch ((OrderTypeEnum)dTOInfo.OrderType)
            {
                //case OrderTypeEnum.GetUrls:
                //    List<UrlItem> urlItems = JsonConvert.DeserializeObject<List<UrlItem>>(Encoding.UTF8.GetString(dTOInfo.Body));
                //    InstantiateUrlItem(urlItems, btnFileroot, btnFilePrefab);

                //    break;

                case OrderTypeEnum.GetFileList:
                    if (EnableRepeatRequest)
                    {
                        string playlistStr = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(dTOInfo.Body));

                        try
                        {
                            List<string> list = playlistStr.Split(",").ToList();

                            InstantiateFileItem(list, playBtnFileRoot, playButtonPrefab);
                        }
                        catch (Exception ex)
                        {
                            Debug.Log(ex.Message);
                        }
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
                case OrderTypeEnum.GetLoopMode:
                    {
                        string loopmode = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(dTOInfo.Body));
                        string head = "Loop|";
                        //提取 9385.891,30037.33,0,2021重庆宣传片30秒.mp4 
                        if (!string.IsNullOrEmpty(loopmode) && loopmode.StartsWith(head))
                        {
                            loopmode = loopmode.Substring(head.Length);
                        }

                        switch (loopmode.ToLower())
                        {
                            case "none":
                                tMP_LoopModeDropdown.SetValueWithoutNotify(0);
                                break;
                            case "one":
                                tMP_LoopModeDropdown.SetValueWithoutNotify(1);
                                break;
                            case "all":
                                tMP_LoopModeDropdown.SetValueWithoutNotify(2);
                                break;
                        }
                    }
                    break;

                case OrderTypeEnum.GetVolumn:
                    float volumn = JsonConvert.DeserializeObject<float>(Encoding.UTF8.GetString(dTOInfo.Body));
                    volumnSlider.value = volumn;
                    break;

            }
        }


    }

    private static string time2str(float totaltime)
    {
        TimeSpan ts = new TimeSpan(0, 0, (int)(totaltime * 0.001));
        string sec = $"{ts.Hours.ToString().PadLeft(2, '0')}:{ts.Minutes.ToString().PadLeft(2, '0')}:{ts.Seconds.ToString().PadLeft(2, '0')}";
        return sec;
    }

    void InstantiateFileItem(List<string> items, Transform rootTransform, Button itemPrefab)
    {
        for (int i = rootTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(rootTransform.GetChild(i).gameObject);
        }
        foreach (string item in items)
        {
            var movienamebtn = Instantiate(itemPrefab, rootTransform);
            movienamebtn.gameObject.transform.localScale = new Vector3(1, 1, 1);
            movienamebtn.onClick.RemoveAllListeners();
            Text btnText = movienamebtn.GetComponentInChildren<Text>();
            btnText.text = item;
            movienamebtn.onClick.AddListener(() =>
            {
                PlayMovieByName(btnText.text);
            });
        }
    }

    void InstantiateUrlItem(List<UrlItem> items, Transform rootTransform, Button itemPrefab)
    {
        for (int i = rootTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(rootTransform.GetChild(i).gameObject);
        }
        foreach (UrlItem item in items)
        {
            var movienamebtn = Instantiate(itemPrefab, rootTransform);
            movienamebtn.gameObject.transform.localScale = new Vector3(1, 1, 1);
            movienamebtn.onClick.RemoveAllListeners();
            Text btnText = movienamebtn.GetComponentInChildren<Text>();
            btnText.text = item.name;
            movienamebtn.onClick.AddListener(() =>
            {
                SendShowBrowser(btnText.text);
            });
        }
    }

    private void OnDisable()
    {
        isonline = false;
        if (EnableRepeatRequest)
        {
            CancelInvoke(nameof(RepeatRequest));
        }
        if (FHClientController.ins != null && FHClientController.ins.fhTcpClient != null)
        {
            FHClientController.ins.fhTcpClient.FHTcpClientReceive -= FHTcpClientReceive;
        }
    }

    /// <summary>
    /// 循环请求进度
    /// </summary>
    void RepeatRequest()
    {
        try
        {
            FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.GetPlayInfo, "");
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
        FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.PlayPrev, "");
        PlayClickSound();
    }
    /// <summary>
    /// 播放下一个
    /// </summary>
    public void PlayNext()
    {
        FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.PlayNext, "");
        PlayClickSound();
    }

    /// <summary>
    /// 停止
    /// </summary>
    public void Stop()
    {
        FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.StopMovie, "");
        PlayClickSound();
    }

    /// <summary>
    /// 播放
    /// </summary>
    public void Play()
    {
        FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.PlayMovie, "");
        PlayClickSound();
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public void Pause()
    {
        FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.PauseMovie, "");
        PlayClickSound();
    }

    /// <summary>
    /// 播放指定视频
    /// </summary>
    /// <param name="moviename">视频文件名</param>
    public void PlayMovieByName(string moviename)
    {
        FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.SetPlayMovie, moviename);
        //FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.SetPlayMovie, moviename.Normalize());
        PlayClickSound();
    }

    /// <summary>
    /// 拖动进度条 鼠标抬起
    /// </summary>
    /// <param name="value"></param>
    public void MovieSeek0()
    {
        FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.SetMovSeek, movieSlider.value);
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
        FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.SetVolumn, volumnSlider.value);
        Debug.Log("VolumnSliderSeek");
    }

    /// <summary>
    /// 获取播放列表
    /// </summary>
    public void GetFileList()
    {
        if (FHClientController.ins != null)
        {
            FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.GetFileList, "获取播放列表");
        }
    }
    public void GetVolumn()
    {
        if (FHClientController.ins != null)
        {
            FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.GetVolumn, "获取音量");
        }
    }

    public void GetLoopMode()
    {
        if (FHClientController.ins != null)
        {
            FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.GetLoopMode, "获取循环模式");
        }
    }
}
