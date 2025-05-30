using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class MainController : MonoBehaviour
{
    [SerializeField]
    private LitVCR litVCR;

    // Start is called before the first frame update
    private void Start()
    {
        litVCR.ReloadFileList();
        if (string.IsNullOrEmpty(Settings.ini.Graphics.ScreenSaver))
        {
            Settings.ini.Graphics.ScreenSaver = "屏保.jpg";
        }
        bool showScreenSaver = litVCR.PlayScreenSaver();
        if (!showScreenSaver)
        {
            litVCR.StartPlay();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            Process.Start(LitVCR.persistentDataPath);
            Application.Quit();
        }
    }
}