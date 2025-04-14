using System.Collections;
using System.Collections.Generic;
using TMPro;
using TouchSocket.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class FHDeviceConfig : MonoBehaviour
{
    [SerializeField]
    FHClientController fhClientController;

    [SerializeField]
    TMP_InputField tMP_Text_IP;

    [SerializeField]
    TMP_InputField tMP_Text_Port;

    [SerializeField]
    Button btn_Connect;
    [SerializeField]
    Button btn_DisConnect;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey(FHClientController.IPHOST_Key))
        {
            string ipHost = PlayerPrefs.GetString(FHClientController.IPHOST_Key);
            string[] ipHostArray = ipHost.Split(':');
            if (ipHostArray.Length == 2)
            {
                tMP_Text_IP.text = ipHostArray[0];
                tMP_Text_Port.text = ipHostArray[1];
            }
        }
        btn_Connect.onClick.AddListener(() =>
        {
            fhClientController.Connect(tMP_Text_IP.text.Trim() + ":" + tMP_Text_Port.text.Trim());
        });
        btn_DisConnect.onClick.AddListener(() =>
        {
            fhClientController.DisConnect();
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
