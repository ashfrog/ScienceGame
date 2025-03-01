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
    // Start is called before the first frame update
    void Start()
    {

        button.onClick.AddListener(() =>
        {
            displayUGUI.CurrentMediaPlayer = mediaPlayerOnce;
            //从头开始播放跳转视频
            mediaPlayerOnce.Control.Seek(0);
            mediaPlayerOnce.Play();
        });
    }
    private void OnEnable()
    {
        //播放循环视频
        mediaPlayerLoop.Play();
        button.gameObject.SetActive(true);
        displayUGUI.CurrentMediaPlayer = mediaPlayerLoop;
    }


    //捕获 mediaPlayerOnce 视频播放完事件


    // Update is called once per frame
    void Update()
    {

    }
}
