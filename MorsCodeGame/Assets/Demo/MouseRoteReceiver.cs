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
    float rotationSpeed = 5f; // 控制旋转平滑度的速度参数

    [SerializeField]
    float deltaScale = 0.1f;

    // 目标旋转值
    private Quaternion targetParentRotation;
    private Quaternion targetObjectRotation;

    // Start is called before the first frame update
    void Start()
    {
        // 初始化目标旋转为当前旋转
        targetParentRotation = obj.transform.parent.rotation;
        targetObjectRotation = obj.transform.rotation;

        StartCoroutine(WaitForTcpServiceInitialization());
    }

    // Update is called once per frame
    void Update()
    {
        // 在Update中平滑地旋转到目标旋转值
        obj.transform.parent.rotation = Quaternion.Slerp(obj.transform.parent.rotation,
                                                       targetParentRotation,
                                                       rotationSpeed * Time.deltaTime);

        obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation,
                                                targetObjectRotation,
                                                rotationSpeed * Time.deltaTime);
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

                            // 更新目标旋转值，而不是直接旋转物体
                            // 为父对象创建旋转
                            Quaternion parentDeltaRot = Quaternion.Euler(vec2.y * deltaScale, 0, 0);
                            targetParentRotation = parentDeltaRot * targetParentRotation;

                            // 为对象自身创建旋转
                            Quaternion objectDeltaRot = Quaternion.Euler(0, -vec2.x * deltaScale, 0);
                            targetObjectRotation = objectDeltaRot * targetObjectRotation;
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