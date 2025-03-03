using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TouchSocket.Core.ByteManager;
using TouchSocket.Sockets;
using UnityEngine;

public class MouseRoteReceiver : MonoBehaviour
{
    [SerializeField]
    FHTcpService tcpService;

    [SerializeField]
    GameObject obj;

    float deltaScale = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForTcpServiceInitialization());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator WaitForTcpServiceInitialization()
    {
        // 等待tcpService初始化完成
        yield return new WaitForSeconds(1f);

        // 绑定tcpService接收消息事件
        tcpService.fh_tcpservice.Received += this.FHService_Received;
    }

    private void FHService_Received(SocketClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
    {
        Loom.QueueOnMainThread(() =>
        {

            // 处理接收到的消息
            try
            {
                var info = requestInfo as DTOInfo;
                switch ((OrderTypeEnum)info.OrderType)
                {

                    case OrderTypeEnum.Rotate:
                        {
                            string v2 = JsonConvert.DeserializeObject<String>(Encoding.UTF8.GetString(info.Body)); //v2
                                                                                                                   //V2 为逗号分割的字符串，第一个为x轴旋转角度增量，第二个为y轴旋转角度增量
                                                                                                                   //将obj按照x,y增量旋转
                            string[] v2s = v2.Split(',');
                            Vector2 vec2 = new Vector2(float.Parse(v2s[0]), float.Parse(v2s[1]));
                            obj.transform.parent.Rotate(vec2.y * deltaScale, 0, 0);
                            obj.transform.Rotate(0, vec2.x * deltaScale, 0);

                        }

                        break;


                }
            }
            catch (Exception ex)
            {
                Debug.Log("ID:" + client.ID + "  " + ex.Message);
            }
        });
    }
}
