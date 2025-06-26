using System;
using System.Text;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Plugins;
using TouchSocket.Sockets;
using UnityEngine;

public class FHTcpClient
{
    public static FHTcpClient ins;
    private TcpClient m_tcpClient = new TcpClient();

    public Action<ITcpClient> Connected;
    public Action DisConnected;
    public Action<DTOInfo> FHTcpClientReceive;

    private string iplog;

    private bool isconnected;
    private TouchSocketConfig config = new TouchSocketConfig();

    public FHTcpClient()
    {
        ins = this;
        m_tcpClient.Connecting += (client, e) =>
        {
            client.SetDataHandlingAdapter(new MyFixedHeaderCustomDataHandlingAdapter());//适配器
        };
        m_tcpClient.Connected += (client, e) =>
        {
            if (m_tcpClient.Online)
            {
                if (Connected != null)
                {
                    Loom.QueueOnMainThread(() => { Connected.Invoke(client); });
                }
            }
        };//成功连接到服务器
        m_tcpClient.Disconnected += (client, e) =>
        {
            if (DisConnected != null)
            {
                Loom.QueueOnMainThread(() => { DisConnected.Invoke(); });
            }
        };//从服务器断开连接，当连接不成功时不会触发。
        m_tcpClient.Received += (client, byteBlock, requestInfo) =>
        {
            if (FHTcpClientReceive != null)
            {
                Loom.QueueOnMainThread(() =>
                {
                    try
                    {
                        FHTcpClientReceive.Invoke(requestInfo as DTOInfo);
                    }
                    catch (Exception ex)
                    {
                        logmsg(ex.Message);
                    }
                });
            }
        };
    }

    public bool IsOnline()
    {
        return m_tcpClient.Online;
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
                //a.UseReconnection(-1, false, 1000);
            })
            .SetBufferLength(1024 * 10)
            .ConfigureContainer(a =>
            {
                //a.SetSingletonLogger(new LoggerGroup(new EasyLogger(logmsg), new FileLogger()));
            });

        //载入配置
        m_tcpClient.Setup(config);
    }

    public ITcpClient StartConnect()
    {
        ITcpClient tcpClient = null;
        try
        {
            tcpClient = m_tcpClient.Connect();
        }
        catch (Exception ex)
        {
            isconnected = false;
            logmsg($"连接失败: {iplog} {ex.Message}");
        }
        return tcpClient;
    }

    public void Send(string msg)
    {
        m_tcpClient.Send(msg);
    }

    public void Send(byte[] data)
    {
        m_tcpClient.Send(data);
    }

    /// <summary>
    /// 转发T对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dataTypeEnum"></param>
    /// <param name="orderTypeEnum"></param>
    /// <param name="obj"></param>
    public void Send<T>(DataTypeEnum dataTypeEnum, OrderTypeEnum orderTypeEnum, T obj)
    {
        byte[] objBuf = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
        ByteBlock block = PackInfo(dataTypeEnum, orderTypeEnum, objBuf);
        try
        {
            m_tcpClient.Send(block);
        }
        catch (Exception ex)
        {
            logmsg("Send<T> fail:" + ex.Message);
        }
    }

    public void SendStr(DataTypeEnum dataTypeEnum, OrderTypeEnum orderTypeEnum, string obj)
    {
        byte[] objBuf = Encoding.GetEncoding("gb2312").GetBytes(obj); //tcp&udp测试工具用的gb2312
        ByteBlock block = PackInfo(dataTypeEnum, orderTypeEnum, objBuf);
        try
        {
            m_tcpClient.Send(block);
        }
        catch (Exception ex)
        {
            logmsg("Send<T> fail:" + ex.Message);
        }
    }

    /// <summary>
    /// 转发16进制字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dataTypeEnum"></param>
    /// <param name="orderTypeEnum"></param>
    /// <param name="obj"></param>
    public void SendHexStr(DataTypeEnum dataTypeEnum, OrderTypeEnum orderTypeEnum, string obj)
    {
        byte[] objBuf = ConvertUtil.HexStrTobyte(obj);
        ByteBlock block = PackInfo(dataTypeEnum, orderTypeEnum, objBuf);
        try
        {
            m_tcpClient.Send(block);
        }
        catch (Exception ex)
        {
            logmsg(ex.Message);
        }
    }

    public void SendBytes(DataTypeEnum dataTypeEnum, OrderTypeEnum orderTypeEnum, byte[] obj)
    {
        ByteBlock block = PackInfo(dataTypeEnum, orderTypeEnum, obj);
        try
        {
            m_tcpClient.Send(block);
        }
        catch (Exception ex)
        {
            logmsg(ex.Message);
        }
    }

    /// <summary>
    /// 发送ask码
    /// </summary>
    /// <param name="dataTypeEnum"></param>
    /// <param name="orderTypeEnum"></param>
    /// <param name="obj"></param>
    public void SendASKII(DataTypeEnum dataTypeEnum, OrderTypeEnum orderTypeEnum, string obj)
    {
        byte[] objBuf = Encoding.ASCII.GetBytes(obj);
        ByteBlock block = PackInfo(dataTypeEnum, orderTypeEnum, objBuf);
        try
        {
            m_tcpClient.Send(block);
        }
        catch (Exception ex)
        {
            logmsg(ex.Message);
        }
    }

    public static ByteBlock PackInfo(DataTypeEnum dataTypeEnum, OrderTypeEnum orderTypeEnum, byte[] objBuf)
    {
        ByteBlock block = new ByteBlock();
        block.Write((int)objBuf.Length + 12);//12为固定格式报文头长度 == MyFixedHeaderCustomDataHandlingAdapter的HeaderLength=>12
        block.Write((int)dataTypeEnum);//datatype
        block.Write((int)orderTypeEnum);//ordertype
        block.Write(objBuf);//写入数据
        return block;
    }

    public void Close()
    {
        if (m_tcpClient.PluginsManager != null)
        {
            m_tcpClient.PluginsManager.Clear();
        }

        isconnected = false;
        this.m_tcpClient.Close();
    }

    public void logmsg(string msg)
    {
        Debug.Log(msg);
    }
}