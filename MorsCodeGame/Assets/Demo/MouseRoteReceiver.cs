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

    [SerializeField, Range(0.1f, 1f)]
    float deltaScale = 0.2f;

    [SerializeField]
    float minVerticalAngle = -90f; // 垂直旋转的最小角度（向下）

    [SerializeField]
    float maxVerticalAngle = 90f; // 垂直旋转的最大角度（向上）

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

            // 应用水平旋转（绕Y轴）
            obj.transform.Rotate(0, -xAmount * deltaScale, 0);

            // 处理垂直旋转（绕X轴），并保持在限制范围内
            if (obj.transform.parent != null)
            {
                // 获取当前垂直角度（绕X轴的旋转）
                float currentXRotation = obj.transform.parent.eulerAngles.x;

                // 将角度转换到 -180 到 180 度范围，便于比较
                if (currentXRotation > 180)
                {
                    currentXRotation -= 360;
                }

                // 计算新的旋转角度
                float newXRotation = currentXRotation + yAmount * deltaScale;

                // 限制在指定范围内
                newXRotation = Mathf.Clamp(newXRotation, minVerticalAngle, maxVerticalAngle);

                // 应用新的受限旋转
                obj.transform.parent.localRotation = Quaternion.Euler(newXRotation,
                    obj.transform.parent.localEulerAngles.y,
                    obj.transform.parent.localEulerAngles.z);
            }

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