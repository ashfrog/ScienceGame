using UnityEngine;

using System.IO.Ports;

using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;

/// <summary>
/// 串口管理示例，演示在 Unity 中通过独立线程或协程来发送、接收 16 进制数据。
/// 可在 Inspector 中配置端口参数，并在其他脚本中调用 SendHexData() 实现发送。
/// </summary>
public class SerialPortManager : MonoBehaviour
{

    [Header("串口配置")]
    public string portName = "COM3";
    public int baudRate = 9600;

    public Parity parity = Parity.None;
    public int dataBits = 8;
    public StopBits stopBits = StopBits.One;
    [SerializeField]
    TMP_Text tMP_Text_Log;

    // 接收数据线程
    private Thread _receiveThread;

    // 串口
    private SerialPort _serialPort;


    // 用于缓存接收的字节数据（线程安全队列）
    private Queue<byte[]> _receivedDataQueue = new Queue<byte[]>();
    private readonly object _queueLock = new object();

    // 判断线程是否应该停止
    private bool _shouldStopThread = false;

    void Awake()
    {
        if (String.IsNullOrEmpty(Settings.ini.Game.SerialPort))
        {
            Settings.ini.Game.SerialPort = portName;
        }
        else
        {
            portName = Settings.ini.Game.SerialPort;
        }

        // 可根据需要使用单例模式
        DontDestroyOnLoad(this.gameObject);
        OpenSerialPort();
    }

    void OnDestroy()
    {
        CloseSerialPort();
    }

    /// <summary>
    /// 打开串口并启动接收线程。
    /// </summary>
    private void OpenSerialPort()
    {

        if (_serialPort != null && _serialPort.IsOpen)
        {
            CloseSerialPort();
        }

        try
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.Open();

            _shouldStopThread = false;
            _receiveThread = new Thread(ReceiveDataThread);
            _receiveThread.Start();

            Debug.Log($"[SerialPortManager] Serial port opened on {portName}");
            tMP_Text_Log.text += $"[SerialPortManager] Serial port opened on {portName}\n";
        }
        catch (Exception e)
        {
            Debug.LogError($"[SerialPortManager] Failed to open serial port: {e.Message}");
            tMP_Text_Log.text += $"[SerialPortManager] Failed to open serial port: {e.Message}\n";
        }

    }

    /// <summary>
    /// 关闭串口并停止接收线程。
    /// </summary>
    private void CloseSerialPort()
    {

        _shouldStopThread = true;

        if (_receiveThread != null && _receiveThread.IsAlive)
        {
            _receiveThread.Join(1000);
        }

        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
            _serialPort.Dispose();
            _serialPort = null;

            Debug.Log("[SerialPortManager] Serial port closed.");
        }

    }

    /// <summary>
    /// 接收数据的线程函数，持续读取串口并存储到队列中。
    /// </summary>
    private void ReceiveDataThread()
    {

        while (!_shouldStopThread)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen && _serialPort.BytesToRead > 0)
                {
                    int bytesToRead = _serialPort.BytesToRead;
                    byte[] buffer = new byte[bytesToRead];
                    _serialPort.Read(buffer, 0, bytesToRead);

                    // 将读到的数据入队，等待主线程处理
                    lock (_queueLock)
                    {
                        _receivedDataQueue.Enqueue(buffer);
                    }
                }
            }
            catch (TimeoutException)
            {
                // 可忽略超时异常
            }
            catch (Exception e)
            {
                Debug.LogError($"[SerialPortManager] Receive thread error: {e.Message}");
                tMP_Text_Log.text += $"[SerialPortManager] Receive thread error: {e.Message}\n";
                break;
            }

            // 减少 CPU 占用
            Thread.Sleep(10);
        }

    }

    /// <summary>
    /// Update 中轮询接收队列并处理数据。
    /// </summary>
    private void Update()
    {
        ProcessReceivedData();
    }

    /// <summary>
    /// 处理接收队列中的所有数据，例如转换为 16 进制并输出到控制台。
    /// 如需业务逻辑处理，可在此处解析数据包等操作。
    /// </summary>
    private void ProcessReceivedData()
    {
        lock (_queueLock)
        {
            while (_receivedDataQueue.Count > 0)
            {
                byte[] data = _receivedDataQueue.Dequeue();
                string hexString = BitConverter.ToString(data).Replace("-", " ");
                Debug.Log($"[SerialPortManager] Received (Hex): {hexString}");
            }
        }
    }

    /// <summary>
    /// 发送以 16 进制字符串标识的数据。
    /// 例如 SendHexData("A5 5A 01 02")。
    /// </summary>
    /// <param name="hexString">以空格或无空格分隔的 16 进制字符串</param>
    public void SendHexData(string hexString)
    {

        if (_serialPort == null || !_serialPort.IsOpen)
        {
            Debug.LogError("[SerialPortManager] Serial port not open.");
            tMP_Text_Log.text += $"[SerialPortManager] Serial port not open.\n";
            return;
        }

        try
        {
            // 将输入字符串按空格分隔
            string[] hexValues = hexString.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] sendBytes = new byte[hexValues.Length];

            for (int i = 0; i < hexValues.Length; i++)
            {
                sendBytes[i] = Convert.ToByte(hexValues[i], 16);
            }

            _serialPort.Write(sendBytes, 0, sendBytes.Length);
            Debug.Log($"[SerialPortManager] Sent (Hex): {hexString}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SerialPortManager] Send failed: {e.Message}");
            tMP_Text_Log.text += $"[SerialPortManager] Send failed: {e.Message}\n";
        }

    }

}