using Newtonsoft.Json;
using System;
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

    private void FHService_Received(SocketClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
    {
        try
        {
            var info = requestInfo as DTOInfo;
            switch ((OrderTypeEnum)info.OrderType)
            {
                case OrderTypeEnum.GetFileList:
                    {
                        //litVCR.ReloadFileList();
                        //Send(client, OrderTypeEnum.GetFileList, litVCR.GetFileListStr());
                        break;
                    }
                case OrderTypeEnum.LoadUrl:
                    {
                        string urlStr = JsonConvert.DeserializeObject<String>(Encoding.UTF8.GetString(info.Body));
                        //canvasWebViewPrefab.WebView.LoadUrl(urlStr);
                    }

                    break;

                case OrderTypeEnum.GoBack:
                    {
                        //canvasWebViewPrefab.WebView.GoBack();
                    }
                    break;

                case OrderTypeEnum.GoForward:
                    {
                        //canvasWebViewPrefab.WebView.GoForward();
                    }
                    break;

                case OrderTypeEnum.Reload:
                    {
                        //canvasWebViewPrefab.WebView.Reload();
                    }
                    break;

                case OrderTypeEnum.EnableBrowser:
                    {
                        //Loom.QueueOnMainThread(() =>
                        //{
                        //    bool browserEnable = JsonConvert.DeserializeObject<bool>(Encoding.UTF8.GetString(info.Body));
                        //    RawImage[] rawImages = canvasWebViewPrefab.transform.GetComponentsInChildren<RawImage>(true);
                        //    foreach (var rawImg in rawImages)
                        //    {
                        //        rawImg.gameObject.SetActive(browserEnable);
                        //    }
                        //});
                    }
                    break;

                case OrderTypeEnum.GetEnableBrowser:
                    {
                        //Loom.QueueOnMainThread(() =>
                        //{
                        //    RawImage[] rawImages = canvasWebViewPrefab.transform.GetComponentsInChildren<RawImage>(true);
                        //    bool enableBrowser = false;
                        //    foreach (var rawImg in rawImages)
                        //    {
                        //        enableBrowser = rawImg.gameObject.activeSelf;
                        //    }
                        //    Send(client, OrderTypeEnum.GetEnableBrowser, enableBrowser);
                        //});
                    }
                    break;

                case OrderTypeEnum.GetPlayInfo:
                    //{
                    //    string dto = $"playinfo|{litVCR.GetPlayInfo()}";
                    //    Send(client, OrderTypeEnum.GetPlayInfo, dto);
                    //}
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.Log("ID:" + client.ID + "  " + ex.Message);
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