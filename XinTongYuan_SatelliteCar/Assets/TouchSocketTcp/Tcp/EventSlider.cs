using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EventSlider : Slider, IDragHandler, IEndDragHandler    //继承拖拽接口
{
    [SerializeField]
    public UnityEvent OnEndDragSlider;
    [SerializeField]
    public UnityEvent PointerUp;
    public UnityEvent PointerDown;
    /// <summary>
    /// 给 Slider 添加开始拖拽事件 //拖拽slider进度条时一直会触发
    /// </summary>
    /// <param name="eventData"></param>
    //public void OnDrag(PointerEventData eventData)
    //{
    //    Debug.Log("drag");
    //    //拖拽时要执行的内容
    //}

    /// <summary>
    /// 给 Slider 添加结束拖拽事件    当拖拽结束后触发
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        // 拖拽结束后要调用的方法
        if (OnEndDragSlider != null)
        {
            OnEndDragSlider.Invoke();
        }
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (PointerDown != null)
        {
            PointerDown.Invoke();
        }
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (PointerUp != null)
        {
            PointerUp.Invoke();
        }
    }
}