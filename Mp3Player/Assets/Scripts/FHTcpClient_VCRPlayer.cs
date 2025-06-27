using Newtonsoft.Json;
using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;


public class FHTcpClient_VCRPlayer : MonoBehaviour
{
    //所有电脑设备都是只需要一个Tcp客户端 包括平板  服务端只有一台就是负责转发给所有的那台设备 这里模拟航线这台设备

    public FHClientController tcpClient;
    public DataTypeEnum receiveDataTypeEnum;//标记当前设备是 航线这台设备  接收的数据类型

    public DataTypeEnum sendDataTypeEnum;  //发送的数据类型
    public LitVCR _vcr;
    float setVolumn = 1f;

    void Start()
    {
        Screen.fullScreen = true;  //设置成全屏

        setVolumn = PlayerPrefs.GetFloat("volumn", 0.5f);

        Settings.ini.IPHost.IPNO = Settings.ini.IPHost.IPNO;
        Settings.ini.IPHost.PadIPNO = Settings.ini.IPHost.PadIPNO;
        receiveDataTypeEnum = (DataTypeEnum)Settings.ini.IPHost.IPNO;
        sendDataTypeEnum = (DataTypeEnum)Settings.ini.IPHost.PadIPNO;



        tcpClient.receiveData += (info) =>
        {
            //所有设备都收到相同的数据 通过DataTypeEnum区分
            //Debug.Log("设备:" + (DataTypeEnum)info.DataType + " 指令:" + (OrderTypeEnum)info.OrderType);
            if (info.DataType == (int)receiveDataTypeEnum)
            {
                switch ((OrderTypeEnum)info.OrderType)//处理指令类型
                {
                    case OrderTypeEnum.GetFileList:                  //获取文件列表
                        string filesStr = _vcr.GetFileListStr();
                        filesStr = filesStr.Replace("," + Settings.ini.Graphics.ScreenSaver, "");
                        tcpClient.Send(sendDataTypeEnum, OrderTypeEnum.GetFileList, filesStr);
                        break;
                    case OrderTypeEnum.GetVolumn:                    //获取当前音量
                        float getVolumn = _vcr.GetVolumn();
                        tcpClient.Send(sendDataTypeEnum, OrderTypeEnum.GetVolumn, getVolumn);
                        break;
                    case OrderTypeEnum.GetPlayInfo:                   //获取当前播放进度
                        string playinfo = _vcr.GetPlayInfo();
                        tcpClient.Send(sendDataTypeEnum, OrderTypeEnum.GetPlayInfo, playinfo);
                        break;
                    case OrderTypeEnum.SetVolumn:                    //设置音量
                        setVolumn = JsonConvert.DeserializeObject<float>(Encoding.UTF8.GetString(info.Body));
                        PlayerPrefs.SetFloat("volumn", setVolumn);
                        _vcr.SetVolumn(setVolumn);
                        break;
                    case OrderTypeEnum.PauseMovie:
                        _vcr.OnPauseButton();          //暂停
                        break;
                    case OrderTypeEnum.PlayScreenSaver:
                        _vcr.PlayScreenSaver();
                        break;
                    case OrderTypeEnum.PlayMovie:
                        _vcr.OnPlayButton();           //播放
                        break;
                    case OrderTypeEnum.StopMovie:
                        _vcr.Stop();
                        _vcr.PlayScreenSaver();
                        break;
                    case OrderTypeEnum.SetMovSeek:                   //设置播放进度
                        float setSeek = JsonConvert.DeserializeObject<float>(Encoding.UTF8.GetString(info.Body));
                        _vcr.OnVideoSeekSlider(setSeek);
                        break;
                    case OrderTypeEnum.PlayPrev:                     //播放上一个视频                    
                        _vcr.PlayPrevious();
                        break;
                    case OrderTypeEnum.PlayNext:                     //播放下一个视频                    
                        _vcr.PlayNext();

                        break;
                    case OrderTypeEnum.SetPlayMovie:                 //指定播放某个视频
                        string videoPath = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(info.Body));
                        _vcr.OpenVideoByFileName(videoPath);
                        break;
                    case OrderTypeEnum.GetLoopMode:
                        {
                            string loopmode = _vcr.GetLoopMode();
                            tcpClient.Send(sendDataTypeEnum, OrderTypeEnum.GetLoopMode, loopmode);
                        }
                        break;
                    case OrderTypeEnum.LoopMode:
                        {
                            int loopmode = JsonConvert.DeserializeObject<int>(Encoding.UTF8.GetString(info.Body));
                            _vcr.SetLoopMode((LitVCR.LoopMode)loopmode);
                        }
                        break;
                    case OrderTypeEnum.SetScreenSaver:
                        {
                            string screenSaver = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(info.Body));
                            _vcr.SetScreenSaver(screenSaver);
                        }
                        break;
                    case OrderTypeEnum.GetScreenSaver:
                        {
                            tcpClient.Send(sendDataTypeEnum, OrderTypeEnum.GetScreenSaver, _vcr.GetScreenSaver());
                        }
                        break;
                    case OrderTypeEnum.Browser:

                    case OrderTypeEnum.GetUrls:

                        break;
                }
            }
        };

    }
}
