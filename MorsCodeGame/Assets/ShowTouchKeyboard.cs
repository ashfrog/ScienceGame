using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Diagnostics;
using UnityEngine.UI;

/// <summary>
/// 点击弹出键盘 需要挂载到输入框上
/// </summary>
public class ShowTouchKeyboard : MonoBehaviour, IPointerClickHandler
{
    private Selectable inputField;
    private void Start()
    {
        inputField = GetComponent<Selectable>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inputField != null)
        {
            // 打开osk键盘
            Process.Start("osk.exe");
        }
    }
}
