using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Btn : MonoBehaviour
{
    public GameObject uiPanelShow;
    public List<GameObject> showList = new List<GameObject>();
    public GameObject uiPanelHide;
    protected Button btn;
    public bool isSendMess = false;//为真是需要发消息的组件
    public string mess; //要发送的消息

    void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        SwitchPanels();
        OnButtonClickOverride();
    }

    protected virtual void OnButtonClickOverride()
    {
        // This is where derived classes can add their own behavior
    }

    void SwitchPanels()
    {
        if (uiPanelShow != null)
        {
            uiPanelShow.SetActive(true);
        }
        if (showList != null)
        {
            foreach (GameObject go in showList)
            {
                go.SetActive(true);
            }
        }
        if (uiPanelHide != null)
        {
            uiPanelHide.SetActive(false);
        }
        if (isSendMess)
        {
            Manager._ins._mouseTouchInputManager.clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.TabControl, mess);
        }

    }
}