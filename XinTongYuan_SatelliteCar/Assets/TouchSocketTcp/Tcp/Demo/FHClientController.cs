using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Text;
using TouchSocket.Sockets;
using UnityEngine;

public class FHClientController : MonoBehaviour
{
    public FHTcpClient fhTcpClient;

    private bool exit;

    [SerializeField]
    private GameObject offLineStatue;

    public Action<DTOInfo> receiveData;

    public string ipHost = "127.0.0.1:4849";

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

    private void ReceiveData(DTOInfo info)
    {
        receiveData?.Invoke(info);
        Loom.QueueOnMainThread(() =>
        {
            try
            {

                Debug.Log((OrderTypeEnum)info.OrderType + "  " + (DataTypeEnum)info.DataType);
                string cmdstr = JsonConvert.DeserializeObject<String>(Encoding.UTF8.GetString(info.Body));
                switch (cmdstr)
                {
                    case "P1_1":
                        Debug.Log(cmdstr);
                        P1_1_1.SetActive(false);
                        P1_1.SetActive(true);
                        break;
                }
            }
            catch (Exception ex)
            {

                Debug.LogException(ex);
            }

        });
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

    private void OnDisable()
    {
        exit = true;
        if (fhTcpClient != null)
        {
            fhTcpClient.Close();
        }

    }
}