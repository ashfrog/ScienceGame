using UnityEngine;
using System.Text;
using System.Collections.Generic;
using UnityEngine.UI;

public class JoystickInputLogger : MonoBehaviour
{
    // 假设常见控制器的按键数量可能为 20
    private const int buttonCount = 20;

    private float lastx;
    private float lasty;
    void Update()
    {
        // 旧版输入系统读取摇杆轴
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        if (horizontal != lastx || vertical != lasty)
        {   // 这里可以添加代码来处理摇杆输入，例如更新UI或执行动作
            Debug.Log($"Joystick - Horizontal: {horizontal}, Vertical: {vertical}");
        }
        lastx = horizontal;
        lasty = vertical;


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
    }
}