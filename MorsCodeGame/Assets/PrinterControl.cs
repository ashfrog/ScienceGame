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

    private void OnEnable()
    {
        curT = 0f;
        cardCamera.depth = 1;
        string PrintVar = PlayerPrefs.GetString("PrintKey");
        cardTextGenerator.SetCardText(PrintVar);

        StartCoroutine(CountDown());
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

    private void Start()
    {
        Settings.ini.Game.ResetTime = Settings.ini.Game.ResetTime;
        waitT = Settings.ini.Game.ResetTime;
    }

    private void OnDisable()
    {
        cardCamera.depth = -1;
    }

    // 双击最大间隔时间
    public float doubleClickThreshold = 0.3f;

    private float lastPressTime = -1f;
    private bool waitingForSecondClick = false;

    float curT = 0;
    float waitT = 8f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (waitingForSecondClick && (Time.time - lastPressTime) < doubleClickThreshold)
            {
                // 双击
                waitingForSecondClick = false;
            }
            else
            {
                // 可能是单击，开始计时
                waitingForSecondClick = true;
                lastPressTime = Time.time;
                OnClick();
            }
        }

        // 超过双击间隔，判定为单击
        if (waitingForSecondClick && (Time.time - lastPressTime) >= doubleClickThreshold)
        {
            waitingForSecondClick = false;
            OnSingleClick();
        }

        curT += Time.deltaTime;
        if (curT > waitT)
        {
            curT = 0f;
            tabSwitcher.SwitchTab(0);
        }
    }

    void OnSingleClick()
    {
        Debug.Log("K键单击事件");
        // 这里写单击执行的逻辑
        tabSwitcher.SwitchTab(0);
    }

    void OnClick()
    {
        Debug.Log("K键双击事件");

        string cardPath = Path.Combine(Settings.ini.Game.SaveCardDirectory, System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg");
        cardTextGenerator.GenerateAndSaveCard(cardPath);
        String printPhotoExePath = Path.Combine(exefolder, "Printer", "PrintPhoto.exe");
        //打印照片
        System.Diagnostics.Process.Start(printPhotoExePath, cardPath);
        tabSwitcher.SwitchTab(0);
        // 这里写双击执行的逻辑
    }
}
