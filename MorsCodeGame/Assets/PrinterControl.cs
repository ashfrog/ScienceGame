using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class PrinterControl : MonoBehaviour
{
    [SerializeField]
    Camera cardCamera;
    [SerializeField]
    CardTextGenerator cardTextGenerator;

    [SerializeField]
    TabSwitcher tabSwitcher;

    [SerializeField]
    TMP_Text SecondText;

#if UNITY_EDITOR
    public static string exefolder = System.Environment.CurrentDirectory;
#else
    public static string exefolder = System.AppDomain.CurrentDomain.BaseDirectory;
#endif

    Coroutine countdownCoroutine; // 新增：用于保存协程句柄

    private void OnEnable()
    {
        cardCamera.depth = 1;
        string PrintVar = PlayerPrefs.GetString("PrintKey");
        cardTextGenerator.SetCardText(PrintVar);
        SecondText.gameObject.SetActive(true);
        // 保证每次 OnEnable 都重置 waitT
        waitT = Settings.ini.Game.ResetTime;

        // 停止旧协程，避免重叠
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        countdownCoroutine = StartCoroutine(CountDown());
    }

    IEnumerator CountDown()
    {
        SecondText.SetText("");
        SecondText.ForceMeshUpdate();
        //SecondText.gameObject.SetActive(false);
        for (int i = (int)waitT; i > 0; i--)
        {
            SecondText.SetText($@"<b><bounce>{i}</bounce></b> ");
            yield return new WaitForSeconds(1);
            SecondText.gameObject.SetActive(true);
        }

        SecondText.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        cardCamera.depth = -1;
        // 停止倒计时协程
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
    }

    // 双击最大间隔时间
    public float doubleClickThreshold = 0.3f;

    float waitT = 8f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            OnClick();
        }

    }

    void OnClick()
    {
        Debug.Log("K键双击事件");
        SecondText.gameObject.SetActive(false);
        string cardPath = Path.Combine(Settings.ini.Game.SaveCardDirectory, System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg");
        cardTextGenerator.GenerateAndSaveCard(cardPath);
        String printPhotoExePath = Path.Combine(exefolder, "Printer", "PrintPhoto.exe");
        //打印照片
        System.Diagnostics.Process.Start(printPhotoExePath, cardPath);
        tabSwitcher.SwitchTab(0);
        // 这里写双击执行的逻辑
    }
}
