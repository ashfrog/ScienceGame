using System;
using System.Collections;
using System.Threading;
using TouchSocket.Sockets;
//using UnityEditor.PackageManager;
using UnityEngine;

public class FHClientController : MonoBehaviour
{
    public static FHClientController ins;

    public FHTcpClient fhTcpClient;

    private bool exit;

    [SerializeField]
    private GameObject offLineStatue;

    public Action<DTOInfo> receiveData;

    public string ipHost = "127.0.0.1:4849";

    public const string IPHOST_Key = "IPHOST";

    public Action<ITcpClient> Connected;
    public Action DisConnected;
    private void Awake()
    {
        if (ins == null)
        {
            ins = this;
        }
        if (fhTcpClient == null)
        {
            fhTcpClient = new FHTcpClient();
        }
    }
    private void Update()
    {
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (PlayerPrefs.HasKey(IPHOST_Key))
        {
            ipHost = PlayerPrefs.GetString(IPHOST_Key);
        }
        if (fhTcpClient == null)
        {
            fhTcpClient = new FHTcpClient();
        }

        fhTcpClient.InitConfig(ipHost);

        exit = false;
        fhTcpClient.FHTcpClientReceive += ReceiveData;
        fhTcpClient.Connected += ((client) =>
        {
            Debug.Log($"FHTcp {client.IP}:{client.Port} 成功连接"); //client.Port为服务器端口
            Connected?.Invoke(client);
            if (offLineStatue != null)
            {
                offLineStatue.SetActive(false);
            }
        });
        fhTcpClient.DisConnected += (() =>
        {
            Debug.Log($"FHTcp 断开连接");
            DisConnected?.Invoke();
            if (offLineStatue != null)
            {
                offLineStatue.SetActive(true);
            }
        });
    }

    private static SemaphoreSlim semaphore = new SemaphoreSlim(1); // 限制异步线程同时进行的连接数 推荐1连接最稳定

    public bool userDisconnect;

    private IEnumerator LoopReconnect()
    {
        while (!exit)
        {
            //等待一帧
            yield return new WaitForEndOfFrame();
            if (userDisconnect)
            {
                // 如果用户手动断开连接，则不再尝试连接
                continue;
            }

            if (fhTcpClient != null && !fhTcpClient.IsOnline())
            {
                ThreadPool.QueueUserWorkItem(async state =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        if (!exit) // 检查是否已退出
                        {
                            fhTcpClient.StartConnect();
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
            }

            // 添加1秒的延迟
            yield return new WaitForSeconds(1);
        }
    }

    private void ReceiveData(DTOInfo info)
    {
        receiveData?.Invoke(info);
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
        fhTcpClient.Send(dataTypeEnum, orderTypeEnum, data);
        Debug.Log(dataTypeEnum + " " + orderTypeEnum);
    }

    public void SendStr(DataTypeEnum dataTypeEnum, OrderTypeEnum orderTypeEnum, string obj)
    {
        fhTcpClient.SendStr(dataTypeEnum, orderTypeEnum, obj);
        Debug.Log(dataTypeEnum + " " + orderTypeEnum + " " + obj);
    }
    public void SendHex(DataTypeEnum dataTypeEnum, OrderTypeEnum orderTypeEnum, string obj)
    {
        fhTcpClient.SendHexStr(dataTypeEnum, orderTypeEnum, obj);
        Debug.Log(dataTypeEnum + " " + orderTypeEnum + " " + obj);
    }
    private void OnEnable()
    {
        exit = false;
        StartCoroutine(LoopReconnect());
    }

    private void OnDisable()
    {
        exit = true;
        if (fhTcpClient != null)
        {
            fhTcpClient.Close();
        }
    }

    public void DisConnect()
    {
        if (fhTcpClient != null)
        {
            userDisconnect = true;
            fhTcpClient.Close();
        }
    }

    public void Connect(string ipHost)
    {
        this.ipHost = ipHost;
        if (fhTcpClient != null)
        {
            PlayerPrefs.SetString(IPHOST_Key, ipHost);
            fhTcpClient.InitConfig(ipHost);

            userDisconnect = false;
        }
    }
}
