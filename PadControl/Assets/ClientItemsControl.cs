using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Start()
    {
        fhClientController = FindObjectOfType<FHClientController>();
        btnOn.onClick.AddListener(() =>
        {
            if (showConfirmDialog)
            {
                ConfirmationDialogExtensions.ShowConfirmationDialog(
                    "提示",
                    "开启操作确认",
                    () => On(), // 确认回调
                    null       // 取消回调（可选）
                );
            }
            else
            {
                On();
            }
        });

        btnOff.onClick.AddListener(() =>
        {
            if (showConfirmDialog)
            {
                ConfirmationDialogExtensions.ShowConfirmationDialog(
                    "注意",
                    "关闭操作确认",
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

    public void On()
    {
        // 将所有指令添加到全局队列
        foreach (var cmd in onCmd)
        {
            AddCommandToQueue(cmd);
        }

        // 执行绑定的控件
        StartCoroutine(ExecuteBindsWithInterval(true));
    }

    public void Off()
    {
        // 将所有指令添加到全局队列
        foreach (var cmd in offCmd)
        {
            AddCommandToQueue(cmd);
        }

        // 执行绑定的控件
        StartCoroutine(ExecuteBindsWithInterval(false));
    }

    /// <summary>
    /// 用于处理DeviceIPNO等不同的指令
    /// </summary>
    /// <param name="on">是否为开启操作</param>
    private IEnumerator ExecuteBindsWithInterval(bool on)
    {
        // 收集所有控件的所有指令，添加到全局队列
        List<CommandQueueManager.CommandData> commandsToEnqueue = new List<CommandQueueManager.CommandData>();

        // 收集 ClientItemsControl 类型的控件的指令
        foreach (var bindControlObj in BindControls)
        {
            var itemsControls = bindControlObj.GetComponentsInChildren<ClientItemsControl>();
            if (itemsControls != null)
            {
                foreach (var itemsControl in itemsControls)
                {
                    // 使用本地变量避免闭包问题
                    var control = itemsControl;

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
                            messageInterval = control.messageInterval
                        };

                        commandsToEnqueue.Add(cmdData);
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