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
    public List<string> onCmd;
    /// <summary>
    /// Off指令
    /// </summary>
    [SerializeField]
    public List<string> offCmd;
    /// <summary>
    /// 发送的CMD是16进制字符
    /// </summary>
    [SerializeField]
    public bool isHexCmd = false;
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

    /// <summary>
    /// 绑定执行ON/Off
    /// </summary>
    [SerializeField]
    List<GameObject> BindControls;

    // Start is called before the first frame update
    void Start()
    {
        fhClientController = FindObjectOfType<FHClientController>();
        btnOn.onClick.AddListener(() =>
        {
            On();
        });

        btnOff.onClick.AddListener(() =>
        {
            Off();
        });
    }

    public void On()
    {
        if (isHexCmd)
        {
            StartCoroutine(SendCommandsWithInterval(onCmd, true));
        }
        else
        {
            StartCoroutine(SendCommandsWithInterval(onCmd, false));
        }

        //执行绑定的控件
        StartCoroutine(ExcudeBindsWithInterval(true));
    }

    /// <summary>
    /// 执行绑定的控件 相当于把绑定的开关都依次按一遍
    /// 添加时间间隔防止连续发送造成问题
    /// </summary>
    /// <param name="on"></param>
    private IEnumerator ExcudeBindsWithInterval(bool on)
    {
        // 收集所有需要执行的命令，按顺序执行
        List<System.Action> allCommands = new List<System.Action>();

        // 收集 ClientItemsControl 类型的控件命令
        foreach (var bindControlObj in BindControls)
        {
            var itemsControls = bindControlObj.GetComponentsInChildren<ClientItemsControl>();
            if (itemsControls != null)
            {
                foreach (var itemsControl in itemsControls)
                {
                    // 使用本地变量避免闭包问题
                    var control = itemsControl;
                    if (on)
                    {
                        allCommands.Add(() => SendCommandOnly(control, true));
                    }
                    else
                    {
                        allCommands.Add(() => SendCommandOnly(control, false));
                    }
                }
            }
        }

        // 收集 ClientItemControl 类型的控件命令
        foreach (var bindControlObj in BindControls)
        {
            var itemControls = bindControlObj.GetComponentsInChildren<ClientItemControl>();
            if (itemControls != null)
            {
                foreach (var itemControl in itemControls)
                {
                    // 使用本地变量避免闭包问题
                    var control = itemControl;
                    if (on)
                    {
                        allCommands.Add(() => control.On());
                    }
                    else
                    {
                        allCommands.Add(() => control.Off());
                    }
                }
            }
        }

        // 按顺序执行所有命令，并在每个命令之间添加时间间隔
        foreach (var command in allCommands)
        {
            command.Invoke();
            // 添加延迟，确保命令之间有间隔
            yield return new WaitForSeconds(messageInterval);
        }
    }

    /// <summary>
    /// 只发送命令，不触发绑定控件的级联操作，防止循环调用
    /// </summary>
    /// <param name="control">要操作的控件</param>
    /// <param name="on">是否为开启操作</param>
    private void SendCommandOnly(ClientItemsControl control, bool on)
    {
        if (on)
        {
            if (control.isHexCmd)
            {
                control.StartCoroutine(control.SendCommandsWithInterval(control.onCmd, true));
            }
            else
            {
                control.StartCoroutine(control.SendCommandsWithInterval(control.onCmd, false));
            }
        }
        else
        {
            if (control.isHexCmd)
            {
                control.StartCoroutine(control.SendCommandsWithInterval(control.offCmd, true));
            }
            else
            {
                control.StartCoroutine(control.SendCommandsWithInterval(control.offCmd, false));
            }
        }
    }

    public void Off()
    {
        if (isHexCmd)
        {
            StartCoroutine(SendCommandsWithInterval(offCmd, true));
        }
        else
        {
            StartCoroutine(SendCommandsWithInterval(offCmd, false));
        }

        //执行绑定的控件
        StartCoroutine(ExcudeBindsWithInterval(false));
    }

    /// <summary>
    /// 带间隔地发送指令列表，防止粘包
    /// </summary>
    /// <param name="commands">要发送的指令列表</param>
    /// <param name="isHex">是否为16进制指令</param>
    /// <returns></returns>
    public IEnumerator SendCommandsWithInterval(List<string> commands, bool isHex)
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