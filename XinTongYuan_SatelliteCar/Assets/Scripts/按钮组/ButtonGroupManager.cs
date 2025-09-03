using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGroupManager : MonoBehaviour
{
    public List<SelectableButton> buttons;
    public Image displayImage;
    public Image displayImage2;
    public int defaultSelectedIndex = 0; // Index of the default selected button


    public void SetUnselected()
    {
        foreach (SelectableButton button in buttons)
        {
            button.SetUnselectedState();
        }
    }

    void Start()
    {
        InitializeButtons();
        SetDefaultSelectedButton();
    }

    void OnEnable()
    {
        // Set the default selected button when the object is enabled
        SetDefaultSelectedButton();
        SetUnselected();
    }

    void InitializeButtons()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] != null)
            {
                buttons[i].SetButtonGroupManager(this);
                buttons[i].SetUnselectedState();
            }
        }
    }

    public void SetDefaultSelectedButton()
    {
        if (buttons.Count > 0 && defaultSelectedIndex >= 0 && defaultSelectedIndex < buttons.Count)
        {
            OnButtonSelected(buttons[defaultSelectedIndex]);
        }
    }

    public void OnButtonSelected(SelectableButton selectedButton)
    {
        foreach (var button in buttons)
        {
            if (button != selectedButton)
            {
                button.SetUnselectedState();
                if (button.uiToShow != null)
                {
                    button.uiToShow.SetActive(false);
                }
                foreach (var ui in button.uiToHideList)
                {
                    if (ui != null)
                    {
                        ui.SetActive(true);
                    }
                }
            }
        }
        selectedButton.SetSelectedState();

        // Update display image
        if (displayImage != null)
        {
            displayImage.sprite = selectedButton.displaySprite;
        }
        if (displayImage2 != null)
        {
            displayImage2.sprite = selectedButton.displaySprite2;
        }

        // Show and hide UI elements for the selected button
        if (selectedButton.uiToShow != null)
        {
            selectedButton.uiToShow.SetActive(true);
        }
        foreach (var ui in selectedButton.uiToHideList)
        {
            if (ui != null)
            {
                ui.SetActive(false);
            }
        }
    }
}