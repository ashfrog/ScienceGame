using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyPanel : MonoBehaviour
{
    [SerializeField]
    DisplayUGUI displayUGUI;
    [SerializeField]
    MediaPlayer mediaPlayerLoop;
    [SerializeField]
    MediaPlayer mediaPlayerOnce;

    [SerializeField]
    Button button;
    [SerializeField]
    TabSwitcher tabSwitcher;
    [SerializeField]
    GameObject Panel;
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            displayUGUI.CurrentMediaPlayer = mediaPlayerOnce;
            //从头开始播放跳转视频
            mediaPlayerOnce.Control.Seek(0);
            mediaPlayerOnce.Play();
            //隐藏button
            Panel.SetActive(false);
        });

        mediaPlayerOnce.Events.AddListener(OnMediaPlayerEvent);

        mediaPlayerLoop.Events.AddListener(OnMediaPlayerEventA);
    }


    private void OnEnable()
    {
        //播放循环视频
        mediaPlayerLoop.Play();

        //Panel.SetActive(true);
        displayUGUI.CurrentMediaPlayer = mediaPlayerLoop;
    }



    private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        if (mp == mediaPlayerOnce && et == MediaPlayerEvent.EventType.FinishedPlaying)
        {
            //跳转到下一个场景
            tabSwitcher.SwitchTab(1);
        }
    }

    private void OnMediaPlayerEventA(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        if (mp == mediaPlayerLoop && et == MediaPlayerEvent.EventType.FinishedPlaying)
        {
            Panel.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
