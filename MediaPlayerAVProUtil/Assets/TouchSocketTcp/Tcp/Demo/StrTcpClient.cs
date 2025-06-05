using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using TouchSocket.Sockets;
using TouchSocket.Core.Config;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Plugins;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using System.Threading;

public class StrTcpClient : MonoBehaviour
{

    enum PanelType
    {
        Media, Time
    }
    [SerializeField]
    TabSwitcher tabSwitcher_time;
    private TcpClient m_tcpClient;

    public Action<String> StrTcpClientReceive;
    public Action<ITcpClient> Connected;
    public Action DisConnected;


    private string iplog;

    private bool isconnected;
    TouchSocketConfig config = new TouchSocketConfig();

    public string ipHost = "127.0.0.1:4850";
    [SerializeField]
    FHTcpClient_VCRPlayer fHTcpClient_VCRPlayer;

    bool exited = false;

    private static SemaphoreSlim semaphore_2 = new SemaphoreSlim(1); // 限制异步线程同时进行的连接数 推荐1连接最稳定
    private void OnEnable()
    {
        Settings.ini.IPHost.MediaTime = Settings.ini.IPHost.MediaTime;
        this.ipHost = Settings.ini.IPHost.DoorIPHost;
        if (string.IsNullOrEmpty(ipHost))
        {
            Debug.Log("DoorIPHost未配置开门联动IPHost");
            return;
        }

        InitConfig(ipHost);
        StartCoroutine(StartConnect());
    }

    public StrTcpClient()
    {
        m_tcpClient = new TcpClient();
        m_tcpClient.Connecting += (client, e) =>
        {
        };
        m_tcpClient.Connected += (client, e) =>
        {
            isconnected = true;
            if (Connected != null)
            {
                Connected.Invoke(client);
            }
            Debug.Log($@"{client.IP}:{client.Port}成功连接");
        };//成功连接到服务器
        m_tcpClient.Disconnected += (client, e) =>
        {
            isconnected = false;
            Debug.Log($"断开连接，信息：{e.Message}");
            if (DisConnected != null)
            {
                DisConnected.Invoke();
            }
        };//从服务器断开连接，当连接不成功时不会触发。
        m_tcpClient.Received += TcpClient_Received;
    }

    float curt = 0;
    float wt = 30;

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.F2)))
        {
            Send("abc");
        }
        curt += Time.deltaTime;
        if (curt > wt)
        {
            curt = 0;
            if (tabSwitcher_time != null)
            {
                tabSwitcher_time.SwitchTab(PanelType.Time);
            }
        }
    }

    public void InitConfig(string IpPort)
    {
        iplog = IpPort;
        string ip_port_conf = IpPort;
        string IP_Port = ip_port_conf;

        config.SetRemoteIPHost(new IPHost(IP_Port))
            .UsePlugin()
            .ConfigurePlugins(a =>
            {
                //a.UseReconnection(-1, false, 1000); //重连插件unity不支持 会阻塞主线程
            })
            .SetBufferLength(1024 * 10)
            .ConfigureContainer(a =>
            {
                a.SetSingletonLogger(new LoggerGroup(new EasyLogger(logmsg), new FileLogger()));
            });

        //载入配置
        m_tcpClient.Setup(config);
    }

    IEnumerator StartConnect()
    {
        while (!exited)
        {
            yield return new WaitForSeconds(1);

            if (m_tcpClient != null && !m_tcpClient.Online)
            {
                ThreadPool.QueueUserWorkItem(async state =>
                {
                    await semaphore_2.WaitAsync();
                    try
                    {
                        if (!exited) // 检查是否已退出
                        {
                            m_tcpClient.Connect();
                        }
                    }
                    finally
                    {
                        semaphore_2.Release();
                    }
                });
            }
        }
    }
    public void Send(string msg)
    {
        m_tcpClient.Send(msg);
    }
    public void Send(byte[] data)
    {
        m_tcpClient.Send(data);
    }
    public void Close()
    {
        isconnected = false;
        exited = true;
        this.m_tcpClient.Close();
    }
    private void OnDisable()
    {
        Close();
    }

    private void TcpClient_Received(TcpClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
    {

        //Debug.Log($"StrTcp从服务器收到消息：{Encoding.UTF8.GetString(byteBlock.ToArray())}");//utf8解码。//无适配器情况接受字符串
        string hexString = BitConverter.ToString(byteBlock.ToArray()).Replace("-", "");
        Debug.Log("StrTcp 收到Hex消息:" + hexString);
        Loom.QueueOnMainThread(() =>
        {
            if ("AAAAAAAAA1".Equals(hexString))
            {
                Debug.Log("开门联动");
                if (tabSwitcher_time != null)
                {
                    tabSwitcher_time.SwitchTab(PanelType.Media);
                }
                fHTcpClient_VCRPlayer._vcr.PlayNext();
                curt = 0;
            }
            if (StrTcpClientReceive != null)
            {
                StrTcpClientReceive.Invoke(Encoding.UTF8.GetString(byteBlock.ToArray()));
            }
        });
    }

    public void logmsg(string msg)
    {
    }

}
