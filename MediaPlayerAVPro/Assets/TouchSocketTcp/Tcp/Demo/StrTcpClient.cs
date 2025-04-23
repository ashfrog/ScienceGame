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

public class StrTcpClient : MonoBehaviour
{
    private TcpClient m_tcpClient;

    public Action<String> StrTcpClientReceive;
    public Action<ITcpClient> Connected;
    public Action DisConnected;


    private string iplog;

    private bool isconnected;
    TouchSocketConfig config = new TouchSocketConfig();

    public string ipHost = "127.0.0.1:4850";

    private void OnEnable()
    {
        InitConfig(ipHost);
        StartConnect();
    }

    public StrTcpClient()
    {
        m_tcpClient = new TcpClient();
        m_tcpClient.Connecting += (client, e) =>
        {
        };
        m_tcpClient.Connected += (client, e) =>
        {
            if (Connected != null)
            {
                Connected.Invoke(client);
            }
            Debug.Log($@"{client.IP}:{client.Port}成功连接");
        };//成功连接到服务器
        m_tcpClient.Disconnected += (client, e) =>
        {
            Debug.Log($"断开连接，信息：{e.Message}");
            if (DisConnected != null)
            {
                DisConnected.Invoke();
            }
        };//从服务器断开连接，当连接不成功时不会触发。
        m_tcpClient.Received += TcpClient_Received;
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.F2)))
        {
            Send("abc");
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

    public bool StartConnect()
    {
        if (!isconnected)
        {
            try
            {
                m_tcpClient.Connect();
                isconnected = true;
            }
            catch (Exception ex)
            {
                m_tcpClient.Logger.Info("中控client:" + iplog + ex.Message);
                isconnected = false;
            }
        }
        return isconnected;
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
        this.m_tcpClient.Close();
    }
    private void OnDisable()
    {
        Close();
    }

    private void TcpClient_Received(TcpClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
    {
        Debug.Log($"StrTcp从服务器收到消息：{Encoding.UTF8.GetString(byteBlock.ToArray())}");//utf8解码。//无适配器情况接受字符串
        if (StrTcpClientReceive != null)
        {
            StrTcpClientReceive.Invoke(Encoding.UTF8.GetString(byteBlock.ToArray()));
        }
    }

    public void logmsg(string msg)
    {
    }

}
