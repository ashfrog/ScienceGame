using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BtnClickSound : MonoBehaviour
{
    public AudioClip clickSound; // 点击音效
    private AudioSource audioSource;

    void Start()
    {
        // 获取 AudioSource 组件
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // 设置音效
        audioSource.clip = clickSound;

        // 遍历场景中所有按钮
        Button[] buttons = FindObjectsOfType<Button>(true);
        foreach (Button button in buttons)
        {
            // 为按钮添加点击事件
            button.onClick.AddListener(() => PlayClickSound());
        }
    }

    void PlayClickSound()
    {
        // 播放音效
        audioSource.Play();
    }
}
