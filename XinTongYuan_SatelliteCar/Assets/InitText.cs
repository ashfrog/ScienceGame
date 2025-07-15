using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitText : MonoBehaviour
{
    [SerializeField]
    Text fileNameText;

    [SerializeField]
    Slider slider;

    [SerializeField]
    Text curT;

    [SerializeField]
    Text totalT;

    private void OnDisable()
    {
        if (fileNameText != null)
        {
            fileNameText.text = "";
        }
        if (slider != null)
        {
            slider.value = 0;
        }

        if (curT != null)
        {
            curT.text = "";
        }
        if (totalT != null)
        {
            totalT.text = "";
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
