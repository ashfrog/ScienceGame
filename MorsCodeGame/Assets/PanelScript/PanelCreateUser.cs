using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelCreateUser : MonoBehaviour
{
    [SerializeField]
    TabSwitcher tabSwitcher;

    public int nextTab;
    [SerializeField]
    PlaylistMediaPlayer playlistMediaPlayer;

    [SerializeField]
    DisplayUGUI displayUGUI;

    [SerializeField]
    List<GameObject> finishEnableObjs;

    [SerializeField]
    bool autoSwitch = true;

    [SerializeField]
    Camera cardCamera;

    bool inited;
    private void Start()
    {
        inited = true;
        playlistMediaPlayer.Events.AddListener(OnPlaylistFinished);
    }

    private void OnEnable()
    {
        displayUGUI.gameObject.SetActive(false);
        tabSwitcher = GetComponentInParent<TabSwitcher>();
        if (inited) //第二次OnEnable才进入
        {
            if (playlistMediaPlayer != null)
            {
                playlistMediaPlayer.JumpToItem(0); // 从第一个开始播放列表
            }
        }

        if (finishEnableObjs != null)
        {
            foreach (var obj in finishEnableObjs)
            {
                obj.SetActive(false);
            }
        }

        if (cardCamera != null)
        {
            cardCamera.depth = -1;
        }
        StartCoroutine(ActivateDisplayUGUIAfterDelay(0.2f));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCodeInput.keyCode))
        {
            if (cardCamera != null)
            {
                cardCamera.depth = -1;
            }
            tabSwitcher.SwitchTab(nextTab); // 直接跳转到下一个场景
        }
    }

    //防止显示上个视频最后一帧
    private IEnumerator ActivateDisplayUGUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        displayUGUI.gameObject.SetActive(true);
    }

    private void OnPlaylistFinished(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        if (mp == playlistMediaPlayer.CurrentPlayer && playlistMediaPlayer.PlaylistIndex == (playlistMediaPlayer.Playlist.Items.Count - 1) && et == MediaPlayerEvent.EventType.FinishedPlaying)
        {
            if (finishEnableObjs != null)
            {
                foreach (var obj in finishEnableObjs)
                {
                    obj.SetActive(true);
                }
            }
            if (cardCamera != null)
            {
                cardCamera.depth = 1;
            }
            if (autoSwitch)
            {
                tabSwitcher.SwitchTab(nextTab); // 播放完列表后跳转到下一个场景
            }
        }
    }
}

