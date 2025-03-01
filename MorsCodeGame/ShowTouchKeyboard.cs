using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Diagnostics;

public class ShowTouchKeyboard : MonoBehaviour, IPointerClickHandler
{
    public TMP_InputField inputField;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inputField != null)
        {
            // 打开Windows屏幕键盘
            Process.Start("osk.exe");
        }
    }
}
