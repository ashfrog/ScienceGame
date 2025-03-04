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

    [SerializeField, Range(0.1f, 10f)]
    float rotationSpeed = 5f; // 控制旋转的平滑度

    float deltaScale = 0.2f;

    // 目标增量旋转值
    private Vector2 targetRotationDelta = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForTcpServiceInitialization());
    }

    // Update is called once per frame
    void Update()
    {
        // 只有当有旋转增量需要应用时才进行旋转
        if (targetRotationDelta.sqrMagnitude > 0.001f)
        {
            // 计算本帧需要应用的旋转量
            float xAmount = Mathf.Lerp(0, targetRotationDelta.x, Time.deltaTime * rotationSpeed);
            float yAmount = Mathf.Lerp(0, targetRotationDelta.y, Time.deltaTime * rotationSpeed);

            // 直接应用增量旋转
            if (obj.transform.parent != null)
            {
                obj.transform.parent.Rotate(yAmount * deltaScale, 0, 0);
            }
            obj.transform.Rotate(0, -xAmount * deltaScale, 0);

            // 减少剩余的旋转增量
            targetRotationDelta.x -= xAmount;
            targetRotationDelta.y -= yAmount;

            // 如果旋转增量很小，则认为已完成
            if (Mathf.Abs(targetRotationDelta.x) < 0.001f && Mathf.Abs(targetRotationDelta.y) < 0.001f)
            {
                targetRotationDelta = Vector2.zero;
            }
        }
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

                            string[] v2s = v2.Split(',');
                            Vector2 vec2 = new Vector2(float.Parse(v2s[0]), float.Parse(v2s[1]));

                            // 将接收到的旋转角度增量添加到目标增量中
                            targetRotationDelta += vec2;
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