using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelRank : MonoBehaviour
{
    [SerializeField]
    Button BtnReturn;
    [SerializeField]
    Button BtnPrint;
    [SerializeField]
    TabSwitcher tabSwitcher;
    [SerializeField]
    MediaPlayer mediaPlayerOnce;
    // Start is called before the first frame update
    void Start()
    {
        if (tabSwitcher == null)
        {
            tabSwitcher = GetComponentInParent<TabSwitcher>();
        }
        //mediaPlayerOnce.Events.AddListener(OnMediaPlayerEvent);
        BtnReturn.onClick.AddListener(() =>
        {
            //BtnReturn.gameObject.SetActive(false);
            //BtnPrint.gameObject.SetActive(false);
            tabSwitcher.SwitchTab(0);
        });
        BtnPrint.onClick.AddListener(() =>
        {
            BtnReturn.gameObject.SetActive(false);
            BtnPrint.gameObject.SetActive(false);
        });
    }
    private void OnEnable()
    {
        BtnReturn.gameObject.SetActive(true);
        BtnPrint.gameObject.SetActive(true);
    }
    //private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    //{
    //    if (mp == mediaPlayerOnce && et == MediaPlayerEvent.EventType.FinishedPlaying)
    //    {
    //        //跳转到下一个场景
    //        tabSwitcher.SwitchTab(0);
    //    }
    //}
    // Update is called once per frame
    void Update()
    {

    }
}
