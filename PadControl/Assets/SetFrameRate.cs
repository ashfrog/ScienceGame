using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetFrameRate : MonoBehaviour
{
    void Start()
    {
        // 检测设备性能并设置相应帧率
        if (SystemInfo.processorCount >= 4 && SystemInfo.systemMemorySize >= 4000)
        {
            // 高端设备设置60fps
            Application.targetFrameRate = 60;
        }
        else
        {
            // 低端设备设置30fps
            Application.targetFrameRate = 30;
        }
    }
}
