using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderEvent : Slider, IDragHandler, IEndDragHandler    //�̳���ק�ӿ�
{
    [SerializeField]
    public UnityEvent OnEndDragSlider;
    [SerializeField]
    public UnityEvent PointerUp;
    public UnityEvent PointerDown;
    /// <summary>
    /// �� Slider ��ӿ�ʼ��ק�¼� //��קslider������ʱһֱ�ᴥ��
    /// </summary>
    /// <param name="eventData"></param>
    //public void OnDrag(PointerEventData eventData)
    //{
    //    Debug.Log("drag");
    //    //��קʱҪִ�е�����
    //}

    /// <summary>
    /// �� Slider ��ӽ�����ק�¼�    ����ק�����󴥷�
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        // ��ק������Ҫ���õķ���
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