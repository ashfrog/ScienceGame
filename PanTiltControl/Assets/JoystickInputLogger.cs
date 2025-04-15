using UnityEngine;
using System.Text;
using System.Collections.Generic;
using UnityEngine.UI;

public class JoystickInputLogger : MonoBehaviour
{
    // 假设常见控制器的按键数量可能为 20
    private const int buttonCount = 20;
    void Update()
    {
        // 旧版输入系统读取摇杆轴
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Debug.Log($"PXN-2113 Pro Joystick - Horizontal: {horizontal}, Vertical: {vertical}");

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