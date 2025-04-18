using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ClientItemsControl : MonoBehaviour
{
    [SerializeField]
    FHClientController fhClientController;
    /// <summary>
    /// 设备IPNO 就是ip的最后一个值 如果重复比如串口服务器某一路就是ip的最后一个值加端口号 
    /// </summary>
    [SerializeField]
    DataTypeEnum deviceIPNO;
    [SerializeField]
    OrderTypeEnum orderType;
    [SerializeField]
    Button btnOn;
    [SerializeField]
    Button btnOff;
    /// <summary>
    /// On指令
    /// </summary>
    [SerializeField]
    List<string> onCmd;
    /// <summary>
    /// Off指令
    /// </summary>
    [SerializeField]
    List<string> offCmd;
    /// <summary>
    /// 发送的CMD是16进制字符
    /// </summary>
    [SerializeField]
    bool isHexCmd = false;
    /// <summary>
    /// 补全CRC16
    /// </summary>
    [SerializeField]
    bool appendCRC16;

    /// <summary>
    /// 消息发送间隔时间(秒)
    /// </summary>
    [SerializeField]
    float messageInterval = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        fhClientController = FindObjectOfType<FHClientController>();
        btnOn.onClick.AddListener(() =>
        {
            if (orderType == OrderTypeEnum.PowerOnMacAddress)
            {
                fhClientController.Send(deviceIPNO, orderType, true);
                return;
            }

            if (isHexCmd)
            {
                StartCoroutine(SendCommandsWithInterval(onCmd, true));
            }
            else
            {
                StartCoroutine(SendCommandsWithInterval(onCmd, false));
            }
        });

        btnOff.onClick.AddListener(() =>
        {
            if (orderType == OrderTypeEnum.PowerOnMacAddress)
            {
                fhClientController.Send(deviceIPNO, orderType, false);
                return;
            }

            if (isHexCmd)
            {
                StartCoroutine(SendCommandsWithInterval(offCmd, true));
            }
            else
            {
                StartCoroutine(SendCommandsWithInterval(offCmd, false));
            }
        });
    }

    /// <summary>
    /// 带间隔地发送指令列表，防止粘包
    /// </summary>
    /// <param name="commands">要发送的指令列表</param>
    /// <param name="isHex">是否为16进制指令</param>
    /// <returns></returns>
    private IEnumerator SendCommandsWithInterval(List<string> commands, bool isHex)
    {
        foreach (var cmd in commands)
        {
            if (isHex)
            {
                fhClientController.SendHex(deviceIPNO, orderType, appendCRC16 ? CRC.GetCRCHexString(cmd) : cmd);
            }
            else
            {
                fhClientController.SendStr(deviceIPNO, orderType, cmd);
            }

            // 添加延迟，防止粘包
            yield return new WaitForSeconds(messageInterval);
        }
    }
}