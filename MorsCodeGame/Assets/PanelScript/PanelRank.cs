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
    [SerializeField]
    GameObject Panel;
    [SerializeField]
    DisplayUGUI displayUGUI;
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
            Panel.gameObject.SetActive(false);
            //从头开始播放跳转视频
            //mediaPlayerOnce.Control.Seek(0);
            //mediaPlayerOnce.Play();
            //displayUGUI.gameObject.SetActive(true);
            tabSwitcher.SwitchTab(0);

        });
        BtnPrint.onClick.AddListener(() =>
        {


        });
    }
    private void OnEnable()
    {
        Panel.gameObject.SetActive(true);
        displayUGUI.gameObject.SetActive(false);
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
