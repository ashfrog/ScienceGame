using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveDisplay : MonoBehaviour
{
    void Awake()
    {
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
    }
    private void Start()
    {
        //Screen.fullScreen = true;  //���ó�ȫ��
    }
    void Update()
    {
        
    }
}
