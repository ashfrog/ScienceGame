using System.Collections.Generic;
using UnityEngine;

//控制ToggleButtonWithImage的Group 只控制一层子物体
public class ToggleButtonGroup : MonoBehaviour
{
    public bool allowMultipleSelection = false;
    private List<ToggleButtonWithImage> toggleButtons;

    private void Start()
    {
        if (toggleButtons == null || toggleButtons.Count == 0)
        {
            toggleButtons = new List<ToggleButtonWithImage>();
            foreach (var item in GetComponentsInChildren<ToggleButtonWithImage>())
            {
                if (item.transform.parent == transform)
                {
                    toggleButtons.Add(item);
                }
            }
        }
    }

    public void OnToggleStateChanged(ToggleButtonWithImage changedButton)
    {
        if (!allowMultipleSelection)
        {
            foreach (var toggleButton in toggleButtons)
            {
                if (toggleButton != changedButton)
                {
                    toggleButton.isOn = false;
                    toggleButton.UpdateButtonImage();
                }
            }
        }
    }
}