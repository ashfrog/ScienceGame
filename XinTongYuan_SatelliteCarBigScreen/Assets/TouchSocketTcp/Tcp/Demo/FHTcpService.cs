using Newtonsoft.Json;
using RenderHeads.Media.AVProVideo.Demos;
using System;
using System.Net.Sockets;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Sockets;
using UnityEngine;

public class FHTcpService : MonoBehaviour
{
    public TcpService fh_tcpservice;

    private int FHPort = 4849;
    public LitVCR _vcr;

    private void OnEnable()
    {
        //FHPort = Settings.ini.IPHost.FHServerPort;
        fh_tcpservice = new TcpService();//用于转发对象服务器
        StartFHTcpService(fh_tcpservice);
    }

    private void OnDisable()
    {
        if (fh_tcpservice != null)
        {
            fh_tcpservice.Stop();
        }
    }

    /// <summary>
    /// 转发对象TcpServer
    /// </summary>
    /// <param name="tcpService"></param>
    private void StartFHTcpService(TcpService tcpService)
    {
        tcpService.Connecting += (client, e) =>
        {
            e.ID = $"{client.IP}:{client.Port}";
        };//有客户端正在连接
        tcpService.Connected += this.FH_service_Connected;//有客户端连接
        tcpService.Disconnected += this.FH_service_Disconnected; ;//有客户端断开连接
        tcpService.Received += this.FHService_Received;


        tcpService.Setup(new TouchSocketConfig().SetListenIPHosts(new IPHost[] { new IPHost(FHPort) })
            .SetMaxCount(10000)
            .SetDataHandlingAdapter(() => { return new MyFixedHeaderCustomDataHandlingAdapter(); })
            .SetThreadCount(10)
            .SetClearInterval(-1)
            .ConfigureContainer(a =>
            {
                a.SetSingletonLogger(new LoggerGroup(new EasyLogger(Debug.Log), new FileLogger()));
            }))
            .Start();//启动
        Debug.Log($"FH服务器{FHPort}成功启动");
    }

    private void FHService_Received(SocketClient tcpClient, ByteBlock byteBlock, IRequestInfo requestInfo)
    {
        try
        {
            Loom.QueueOnMainThread(() =>
            {
                var info = requestInfo as DTOInfo;
                switch ((OrderTypeEnum)info.OrderType)
                {
                    case OrderTypeEnum.GetFileList:                  //获取文件列表
                        string filesStr = _vcr.GetFileListStr();
                        filesStr = filesStr.Replace("," + Settings.ini.Graphics.ScreenSaver, "");
                        Send(tcpClient, OrderTypeEnum.GetFileList, filesStr);
                        break;
                    case OrderTypeEnum.GetVolumn:                    //获取当前音量
                        float getVolumn = _vcr.GetVolumn();
                        Send(tcpClient, OrderTypeEnum.GetVolumn, getVolumn);
                        break;
                    case OrderTypeEnum.GetPlayInfo:                   //获取当前播放进度
                        string playinfo = _vcr.GetPlayInfo();
                        Send(tcpClient, OrderTypeEnum.GetPlayInfo, playinfo);
                        break;
                    case OrderTypeEnum.SetVolumn:                    //设置音量
                        var setVolumn = JsonConvert.DeserializeObject<float>(Encoding.UTF8.GetString(info.Body));
                        PlayerPrefs.SetFloat("volumn", setVolumn);
                        _vcr.SetVolumn(setVolumn);
                        break;
                    case OrderTypeEnum.PauseMovie:
                        _vcr.OnPauseButton();          //暂停
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
                }
            });
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void Send<T>(SocketClient client, OrderTypeEnum orderTypeEnum, T obj)
    {
        byte[] objBuf = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
        ByteBlock block = FHTcpClient.PackInfo(DataTypeEnum.S_Pad, orderTypeEnum, objBuf);
        try
        {
            client.Send(block);
        }
        catch (Exception ex)
        {
            Debug.Log("Send<T> fail:" + ex.Message);
        }
    }

    private void FH_service_Connected(SocketClient client, TouchSocketEventArgs e)
    {
        Loom.QueueOnMainThread(() =>
        {
            Debug.Log(DateTime.Now.ToString() + " " + client.IP + "接入FH TCP");
        });
    }

    private void FH_service_Disconnected(SocketClient client, ClientDisconnectedEventArgs e)
    {
        Loom.QueueOnMainThread(() =>
        {
            Debug.Log(DateTime.Now.ToString() + " " + client.IP + "断开FH TCP");
        });
    }
}