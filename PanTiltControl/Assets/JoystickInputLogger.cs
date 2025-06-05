using UnityEngine;
using System.Text;
using System.Collections.Generic;
using UnityEngine.UI;
//using System.Diagnostics.Eventing.Reader;

public class JoystickInputLogger : MonoBehaviour
{
    // 假设常见控制器的按键数量可能为 20
    private const int buttonCount = 5;

    private float lastx;
    private float lasty;

    float curt = 0f;
    float waitTime = 0.2f;

    [SerializeField]
    SerialPortManager sp;

    float minaxis = 0.2f;

    bool PressedKey0 = false;
    private void Start()
    {
        Settings.ini.Game.enableY = Settings.ini.Game.enableY;
    }
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            Debug.Log("0");
            PressedKey0 = true;
        }
        if (Input.GetKeyUp(KeyCode.Joystick1Button0))
        {
            Debug.Log("0");
            PressedKey0 = false;
        }
        // 旧版输入系统读取按键
        // 这里以Joystick1Button0、Joystick1Button1等为例，可根据设备实际按键情况调整
        // 输出所有按键消息
        for (int i = 0; i < buttonCount; i++)
        {
            var key = $"Joystick1Button0{i}";
            if (Input.GetKeyDown(KeyCode.Joystick1Button0 + i))
            {
                Debug.Log($"PXN-2113 Pro Joystick - {key} Pressed");
            }
            if (Input.GetKeyUp(KeyCode.Joystick1Button0 + i))
            {
                Debug.Log($"PXN-2113 Pro Joystick - {key} Released");
            }
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button1))
        {
            Debug.Log("1");
        }
        if (Input.GetKeyDown(KeyCode.Joystick1Button2))
        {
            Debug.Log("2");
            if (PressedKey0)
            {
                //设置预设点
                sp.SendHexData("FF 01 00 03 00 02 06");
            }
            else
            {
                //调用预设点
                sp.SendHexData("FF 01 00 07 00 02 0A");
            }
        }
        if (Input.GetKeyDown(KeyCode.Joystick1Button3))
        {
            Debug.Log("3");
            if (PressedKey0)
            {
                //设置预设点
                sp.SendHexData("FF 01 00 03 00 03 07");
            }
            else
            {
                //调用预设点
                sp.SendHexData("FF 01 00 07 00 03 0B");
            }
        }
        if (Input.GetKeyDown(KeyCode.Joystick1Button4))
        {
            Debug.Log("4");
            if (PressedKey0)
            {
                //设置预设点
                sp.SendHexData("FF 01 00 03 00 04 08");
            }
            else
            {
                //调用预设点
                sp.SendHexData("FF 01 00 07 00 04 0C");
            }
        }
        if (Input.GetKeyDown(KeyCode.Joystick1Button5))
        {
            Debug.Log("5");
            if (PressedKey0)
            {
                //设置预设点
                sp.SendHexData("FF 01 00 03 00 05 09");
            }
            else
            {
                //调用预设点
                sp.SendHexData("FF 01 00 07 00 05 0D");
            }
        }

        curt += Time.deltaTime;
        if (curt >= waitTime)
        {
            curt = 0;
            // 旧版输入系统读取摇杆轴
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            if (horizontal != lastx || vertical != lasty)
            {   // 这里可以添加代码来处理摇杆输入，例如更新UI或执行动作
                Debug.Log($"Joystick - Horizontal: {horizontal}, Vertical: {vertical}");

                if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
                {
                    if (horizontal > minaxis)
                    {
                        //向右
                        sp.SendHexData("FF 01 00 04 20 00 25");
                    }
                    else if (horizontal < -minaxis)
                    {
                        //向左
                        sp.SendHexData("FF 01 00 02 20 00 23");
                    }
                }
                else
                {
                    if (Settings.ini.Game.enableY)
                    {
                        if (vertical > minaxis)
                        {
                            //向上
                            sp.SendHexData("FF 01 00 08 00 20 29");
                        }
                        else if (vertical < -minaxis)
                        {
                            //向下
                            sp.SendHexData("FF 01 00 10 00 20 31");
                        }
                    }
                }

                if (Mathf.Abs(horizontal) < 0.2f && Mathf.Abs(vertical) < 0.2f)
                {
                    //停止
                    sp.SendHexData("FF 01 00 00 00 00 01");
                }

            }
            lastx = horizontal;
            lasty = vertical;
        }



    }
}