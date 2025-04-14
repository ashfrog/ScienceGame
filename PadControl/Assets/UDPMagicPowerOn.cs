using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UDPMagicPowerOn : MonoBehaviour
{
    [SerializeField]
    FHClientController fhClientController;

    [SerializeField]
    TMP_InputField tMP_Text_MAC;

    [SerializeField]
    Button btnOn;
    [SerializeField]
    Button btnOff;

    private void Start()
    {
        tMP_Text_MAC.text = PlayerPrefs.GetString("MAC", "D8-43-AE-B6-F7-2C");
        btnOn.onClick.AddListener(() =>
        {
            On(tMP_Text_MAC.text);
        });
        btnOff.onClick.AddListener(() =>
        {
            PowerOff();
        });
    }

    //根据网线mac地址发送魔法包 只用于中控那台电脑开机
    public static void On(string macAddress)
    {
        byte[] sendBytes = new byte[102];
        for (int i = 0; i < 6; i++)
        {
            sendBytes[i] = 0xFF;
        }
        macAddress = macAddress.Replace(":", "").Replace("-", "").Replace(" ", "");
        //byte test = byte.Parse("FF", System.Globalization.NumberStyles.HexNumber);
        try
        {
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    sendBytes[(i + 1) * 6 + j] = byte.Parse(macAddress.Substring(j * 2, 2), System.Globalization.NumberStyles.HexNumber);
                }
            }

            UdpClient udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            udpClient.Send(sendBytes, sendBytes.Length, "255.255.255.255", 7); //端口号7或者9
            udpClient.Close();
            udpClient.Dispose();

            PlayerPrefs.SetString("MAC", macAddress);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }

    }


    public void PowerOff()
    {
        if (fhClientController == null)
        {
            fhClientController = FindObjectOfType<FHClientController>();
        }
        fhClientController.Send(DataTypeEnum.S_MainHost, OrderTypeEnum.PowerOnMacAddress, false);
    }

    /// <summary>
    /// 主机全关
    /// </summary>
    public void PowerOffAll()
    {
        try
        {
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.EnableBroadcast = true;
                byte[] sendBytes = Encoding.UTF8.GetBytes("shutdown");
                udpClient.Send(sendBytes, sendBytes.Length, "255.255.255.255", 7700);
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"主机全关-发送广播失败: {ex.Message}");
        }
    }
}
