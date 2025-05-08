using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableButton : MonoBehaviour
{
    private ButtonGroupManager buttonGroupManager;
    private Button button;
    private Image buttonImage;
    public Sprite selectedSprite;
    public Sprite unselectedSprite;
    public Sprite displaySprite; // The sprite to display when this button is selected
    public GameObject uiToShow; // The UI element to show when this button is selected
    public List<GameObject> uiToHideList; // The list of UI elements to hide when this button is selected

    void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        if (buttonImage == null)
        {
            Debug.LogError("Image component is not set on " + gameObject.name);
        }
    }

    void Start()
    {
        button.onClick.AddListener(OnButtonClick);
        SetUnselectedState();
    }

    public void SetButtonGroupManager(ButtonGroupManager manager)
    {
        buttonGroupManager = manager;
    }

    public void SetUnselectedState()
    {
        if (buttonImage != null)
        {
            buttonImage.sprite = unselectedSprite;
        }
    }

    public void SetSelectedState()
    {
        if (buttonImage != null)
        {
            buttonImage.sprite = selectedSprite;
        }
    }

    private void OnButtonClick()
    {
        if (buttonGroupManager == null)
        {
            Debug.LogError("buttonGroupManager is null!");
            return;
        }

        buttonGroupManager.OnButtonSelected(this);
    }
}