using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelCreateUser : MonoBehaviour
{


    [SerializeField]
    Button button;
    [SerializeField]
    TabSwitcher tabSwitcher;
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            tabSwitcher.SwitchTab(2);
        });


    }



    // Update is called once per frame
    void Update()
    {

    }
}
