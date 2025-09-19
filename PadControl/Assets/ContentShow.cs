using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using TouchSocket.Core.XREF.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class ContentShow : MonoBehaviour
{
    [SerializeField]
    DataTypeEnum dataTypeEnum;

    [SerializeField]
    string key;

    [SerializeField]
    TMP_Text text;

    [SerializeField]
    ScrollRect scrollRect;
    // Start is called before the first frame update
    private void Start()
    {

    }


    private void OnEnable()
    {
        StartCoroutine(InitComponent());
    }
    private void OnDisable()
    {
        isonline = false;

        if (FHClientController.ins != null && FHClientController.ins.fhTcpClient != null)
        {
            FHClientController.ins.fhTcpClient.FHTcpClientReceive -= FHTcpClientReceive;
        }
    }

    bool isonline = false;
    IEnumerator InitComponent()
    {

        while (!isonline)
        {
            isonline = FHClientController.ins != null
                && FHClientController.ins.fhTcpClient != null
                && FHClientController.ins.fhTcpClient.IsOnline();
            yield return null;
        }

        if (FHClientController.ins != null && FHClientController.ins.fhTcpClient != null)
        {
            FHClientController.ins.fhTcpClient.FHTcpClientReceive += FHTcpClientReceive;
        }

        GetContent();
    }



    void FHTcpClientReceive(DTOInfo dTOInfo)
    {
        //Debug.Log(dTOInfo.DataType);
        if (dTOInfo.DataType == (int)dataTypeEnum)
        {
            switch ((OrderTypeEnum)dTOInfo.OrderType)
            {
                case OrderTypeEnum.Content:
                    string content = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(dTOInfo.Body));

                    text.text = content;

                    StartCoroutine(ResetScrollToTopNextFrame());
                    break;
            }
        }
    }

    private IEnumerator ResetScrollToTopNextFrame()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content as RectTransform); // 强制重建布局
        yield return null;
        scrollRect.verticalNormalizedPosition = 1f; // 回到顶部
    }


    public void GetContent()
    {
        if (FHClientController.ins != null)
        {
            FHClientController.ins.Send(dataTypeEnum, OrderTypeEnum.GetContent, key);
        }
    }
}
