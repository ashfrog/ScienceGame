using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMess : MonoBehaviour
{
    public GameObject Panel1_1;
    public Btn btn;

    private void OnEnable()
    {
        if (Panel1_1.activeInHierarchy)
        {
            btn.mess = "发射展示返回";
        }
        else
        {
            btn.mess = "汽车内饰返回";
        }
    }
}
