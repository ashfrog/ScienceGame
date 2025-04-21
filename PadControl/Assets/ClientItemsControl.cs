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
        // 将所有指令添加到全局队列
        foreach (var cmd in onCmd)
        {
            AddCommandToQueue(cmd, isHexCmd, appendCRC16);
        }

        //执行绑定的控件
        StartCoroutine(ExcudeBindsWithInterval(true));
    }

    /// <summary>
    /// 全局队列，用于存储所有需要发送的指令
    /// </summary>
    private static Queue<CommandData> globalCommandQueue = new Queue<CommandData>();

    /// <summary>
    /// 指示是否正在处理全局队列
    /// </summary>
    private static bool isProcessingQueue = false;

    /// <summary>
    /// 指令数据结构
    /// </summary>
    private struct CommandData
    {
        public FHClientController controller;
        public DataTypeEnum deviceID;
        public OrderTypeEnum orderType;
        public string command;
        public bool isHex;
        public bool appendCRC16;
    }

    /// <summary>
    /// 执行绑定的控件 相当于把绑定的开关都依次按一遍
    /// 将所有指令统一添加到全局队列中顺序执行
    /// </summary>
    /// <param name="on"></param>
    private IEnumerator ExcudeBindsWithInterval(bool on)
    {
        // 收集所有控件的所有指令，添加到全局队列

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

                    // 将所有指令添加到全局队列
                    foreach (var cmd in cmdList)
                    {
                        CommandData cmdData = new CommandData
                        {
                            controller = control.fhClientController,
                            deviceID = control.deviceIPNO,
                            orderType = control.orderType,
                            command = cmd,
                            isHex = control.isHexCmd,
                            appendCRC16 = control.appendCRC16
                        };

                        globalCommandQueue.Enqueue(cmdData);
                    }
                }
            }
        }

        yield return null;
        // 启动队列处理（如果尚未启动）
        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessCommandQueue());
        }
    }

    /// <summary>
    /// 处理全局指令队列
    /// </summary>
    private IEnumerator ProcessCommandQueue()
    {
        isProcessingQueue = true;

        while (globalCommandQueue.Count > 0)
        {
            // 获取队列中的下一个指令
            CommandData cmdData = globalCommandQueue.Dequeue();

            // 发送指令
            if (cmdData.isHex)
            {
                string finalCmd = cmdData.appendCRC16 ? CRC.GetCRCHexString(cmdData.command) : cmdData.command;
                cmdData.controller.SendHex(cmdData.deviceID, cmdData.orderType, finalCmd);
            }
            else
            {
                cmdData.controller.SendStr(cmdData.deviceID, cmdData.orderType, cmdData.command);
            }

            // 添加延迟，确保指令之间有间隔
            yield return new WaitForSeconds(messageInterval);
        }

        isProcessingQueue = false;
    }

    /// <summary>
    /// 添加单个指令到全局队列
    /// </summary>
    public void AddCommandToQueue(string cmd, bool isHex, bool appendCRC16 = false)
    {
        CommandData cmdData = new CommandData
        {
            controller = fhClientController,
            deviceID = deviceIPNO,
            orderType = orderType,
            command = cmd,
            isHex = isHex,
            appendCRC16 = appendCRC16
        };

        globalCommandQueue.Enqueue(cmdData);

        // 启动队列处理（如果尚未启动）
        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessCommandQueue());
        }
    }

    public void Off()
    {
        // 将所有指令添加到全局队列
        foreach (var cmd in offCmd)
        {
            AddCommandToQueue(cmd, isHexCmd, appendCRC16);
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