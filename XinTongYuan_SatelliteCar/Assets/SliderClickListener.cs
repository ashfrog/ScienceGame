using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderClickListener : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    ButtonGroupManager groupManager;

    [SerializeField]
    Slider slider;

    public Action<float> SlicerClicked;
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("点击slider 重置按钮的选中状态");
        // 你的处理逻辑
        groupManager.SetUnselected();
        Debug.Log(slider.value);
        SlicerClicked?.Invoke(slider.value);
    }
}