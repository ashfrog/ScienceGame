using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public class FHClientController : MonoBehaviour
{
    public FHTcpClient fhTcpClient;

    private bool exit;

    [SerializeField]
    private GameObject offLineStatue;

    public Action<DTOInfo> receiveData;

    public string ipHost = "127.0.0.1:4849";

    private void Update()
    {
    }

    // Start is called before the first frame update
    private void Start()
    {
        fhTcpClient = new FHTcpClient();

        fhTcpClient.InitConfig(ipHost);

        exit = false;
        fhTcpClient.FHTcpClientReceive = ReceiveData;
        fhTcpClient.Connected += ((client) =>
        {
            Debug.Log($"FHTcp {client.IP}:{client.Port} 成功连接");
        });
    }

    private static SemaphoreSlim semaphore = new SemaphoreSlim(1); // 限制异步线程同时进行的连接数 推荐1连接最稳定

    private IEnumerator LoopReconnect()
    {
        while (!exit)
        {
            //等待一帧
            yield return new WaitForEndOfFrame();

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

            bool isonline = fhTcpClient != null && fhTcpClient.IsOnline();
            if (offLineStatue != null)
            {
                offLineStatue.SetActive(!isonline);
            }
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
    }

    public void SendStr(DataTypeEnum dataTypeEnum, OrderTypeEnum orderTypeEnum, string obj)
    {
        fhTcpClient.SendStr(dataTypeEnum, orderTypeEnum, obj);
    }
    public void SendHex(DataTypeEnum dataTypeEnum, OrderTypeEnum orderTypeEnum, string obj)
    {
        fhTcpClient.SendHexStr(dataTypeEnum, orderTypeEnum, obj);
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
}
