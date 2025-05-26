using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientItemsControl : MonoBehaviour
{
    [SerializeField]
    public FHClientController fhClientController;
    /// <summary>
    /// 设备IPNO 就是ip的最后一个值 如果重复比如串口服务器某一路就是ip的最后一个值加端口号 
    /// </summary>
    [SerializeField]
    public DataTypeEnum deviceIPNO;
    [SerializeField]
    public OrderTypeEnum orderType = OrderTypeEnum.Str;
    [SerializeField]
    Button btnOn;

    [SerializeField]
    Button[] btnOnBinds;

    [SerializeField]
    Button btnOff;

    [SerializeField]
    Button[] btnOffBinds;

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
    public bool appendCRC16;

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

    [SerializeField]
    bool showConfirmDialog;

    /// <summary>
    /// 展项控制中排除这个控件 （统一开关中排除该开关）
    /// </summary>
    [SerializeField]
    bool ignoreBindInGroup;

    /// <summary>
    /// 延迟入队列 作用为控制指令执行顺序
    /// </summary>
    [SerializeField]
    private float enqueDelay;

    // Start is called before the first frame update
    void Start()
    {
        fhClientController = FindObjectOfType<FHClientController>();
        TMP_Text controlText = GetComponentInChildren<TMP_Text>();
        if (btnOn != null)
        {
            btnOn.onClick.AddListener(() =>
            {
                if (showConfirmDialog)
                {
                    ConfirmationDialogExtensions.ShowConfirmationDialog(
                        "提示",
                        $"开启{controlText.text}",
                        () => On(), // 确认回调
                        null       // 取消回调（可选）
                    );
                }
                else
                {
                    On();
                }
            });
        }

        if (btnOff != null)
        {
            btnOff.onClick.AddListener(() =>
            {
                if (showConfirmDialog)
                {
                    ConfirmationDialogExtensions.ShowConfirmationDialog(
                        "提示",
                        $"关闭{controlText.text}",
                        () => Off(), // 确认回调
                        null        // 取消回调（可选）
                    );
                }
                else
                {
                    Off();
                }
            });
        }

        if (btnOnBinds != null && btnOnBinds.Length > 0)
        {
            foreach (Button btnOnBind in btnOnBinds)
            {
                btnOnBind.onClick.AddListener(() =>
                {
                    On();
                });
            }
        }

        if (btnOffBinds != null && btnOffBinds.Length > 0)
        {
            foreach (Button btnOffBind in btnOffBinds)
            {
                btnOffBind.onClick.AddListener(() =>
                {
                    Off();
                });
            }
        }
    }

    public void On()
    {
        // 执行绑定的控件
        StartCoroutine(ExecuteBindsWithInterval(true));
    }

    public void Off()
    {
        // 执行绑定的控件
        StartCoroutine(ExecuteBindsWithInterval(false));
    }

    /// <summary>
    /// 用于处理DeviceIPNO等不同的指令
    /// </summary>
    /// <param name="on">是否为开启操作</param>
    private IEnumerator ExecuteBindsWithInterval(bool on)
    {
        if (enqueDelay > 0)
        {
            yield return new WaitForSeconds(enqueDelay);
        }

        if (on)
        {
            // 将所有指令添加到全局队列
            foreach (var cmd in onCmd)
            {
                AddCommandToQueue(cmd);
            }
        }
        else
        {
            // 将所有指令添加到全局队列
            foreach (var cmd in offCmd)
            {
                AddCommandToQueue(cmd);
            }
        }
        // 收集所有控件的所有指令，添加到全局队列
        List<CommandQueueManager.CommandData> commandsToEnqueue = new List<CommandQueueManager.CommandData>();

        // 收集 ClientItemsControl 类型的控件的指令
        foreach (var bindControlObj in BindControls)
        {
            var itemsControls = bindControlObj.GetComponentsInChildren<ClientItemsControl>(true);
            if (itemsControls != null)
            {
                foreach (var itemsControl in itemsControls)
                {
                    // 使用本地变量避免闭包问题
                    var control = itemsControl;

                    if (control.fhClientController == null)
                    {
                        control.fhClientController = fhClientController;
                    }

                    // 获取该控件需要发送的指令列表
                    List<string> cmdList = on ? control.onCmd : control.offCmd;

                    // 创建命令数据并添加到待入队列表
                    foreach (var cmd in cmdList)
                    {
                        CommandQueueManager.CommandData cmdData = new CommandQueueManager.CommandData
                        {
                            controller = control.fhClientController,
                            deviceID = control.deviceIPNO,
                            orderType = control.orderType,
                            command = cmd,
                            isHex = control.isHexCmd,
                            appendCRC16 = control.appendCRC16,
                            messageInterval = control.messageInterval,
                            ignoreBindInGroup = control.ignoreBindInGroup
                        };
                        if (!control.ignoreBindInGroup)
                        {
                            commandsToEnqueue.Add(cmdData);
                        }
                        else
                        {
                            Debug.Log("展项控制排除开关：" + cmd);
                        }
                    }
                }
            }
        }

        // 批量添加所有命令到队列管理器
        CommandQueueManager.Instance.EnqueueCommands(commandsToEnqueue);

        yield return null;
    }

    /// <summary>
    /// 添加单个指令到全局队列
    /// </summary>
    public void AddCommandToQueue(string cmd)
    {
        CommandQueueManager.CommandData cmdData = new CommandQueueManager.CommandData
        {
            controller = fhClientController,
            deviceID = deviceIPNO,
            orderType = orderType,
            command = cmd,
            isHex = isHexCmd,
            appendCRC16 = appendCRC16,
            messageInterval = messageInterval
        };

        CommandQueueManager.Instance.EnqueueCommand(cmdData);
    }
}