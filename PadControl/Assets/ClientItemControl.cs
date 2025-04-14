using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientItemControl : MonoBehaviour
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
    string onCmd;
    /// <summary>
    /// Off指令
    /// </summary>
    [SerializeField]
    string offCmd;
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
                fhClientController.SendHex(deviceIPNO, orderType, appendCRC16 ? CRC.GetCRCHexString(onCmd) : onCmd);
            }
            else
            {
                fhClientController.SendStr(deviceIPNO, orderType, onCmd);
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
                fhClientController.SendHex(deviceIPNO, orderType, appendCRC16 ? CRC.GetCRCHexString(offCmd) : offCmd);
            }
            else
            {
                fhClientController.SendStr(deviceIPNO, orderType, offCmd);
            }
        });
    }
}
