using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class SwitchBtn : MonoBehaviour
{
    /// <summary>
    /// 切换显示状态
    /// </summary>
    /// <param name="close">显示状态</param>
    public void SwitchDisplayState(GameObject DisplayState)
    {
        GameObject _click = EventSystem.current.currentSelectedGameObject; //获取点击的对应按钮
        _click.transform.parent.GetComponent<CanvasGroup>().alpha = 0;
        DisplayState.GetComponent<CanvasGroup>().alpha = 1;
        //_click.transform.parent.gameObject.SetActive(false);
        //DisplayState.SetActive(true);

        try
        {
            AudioSource audio = GameObject.Find("Audio Source").GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.Play();
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }

    }


}
