using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class DisplayGameTimer : MonoBehaviour
{
    [Header("UI设置")]
    public TMP_Text timerText;  // 显示计时的UI文本

    private DateTime startTime;
    private bool isRunning = false;

    public void StartGameTimer()
    {
        StartTimer();
    }

    void Update()
    {
        if (isRunning)
        {
            UpdateTimeDisplay();
        }
    }

    public void StartTimer()
    {
        startTime = DateTime.Now;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    private void UpdateTimeDisplay()
    {
        TimeSpan currentTime = DateTime.Now - startTime;
        timerText.text = string.Format("{0:mm\\:ss\\.fff}", currentTime);
    }

    // 获取当前耗时（可选，供其他脚本调用）
    public TimeSpan GetCurrentTime()
    {
        if (isRunning)
        {
            return DateTime.Now - startTime;
        }
        return TimeSpan.Zero;
    }
}