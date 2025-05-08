using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Btn : MonoBehaviour
{
    public GameObject uiPanelShow;
    public GameObject uiPanelHide;
    protected Button btn;

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
        if (uiPanelShow!=null)
        {
            uiPanelShow.SetActive(true);
        }
        if (uiPanelHide != null)
        {
            uiPanelHide.SetActive(false);
        }
        
    }
}