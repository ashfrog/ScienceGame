using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextLoad : MonoBehaviour
{
    [SerializeField]
    TMP_Text tMP_Text;
    // Start is called before the first frame update
    void Start()
    {
        Settings.ini.Game.TextTooltip = Settings.ini.Game.TextTooltip;
        if (tMP_Text == null)
        {
            tMP_Text = GetComponent<TMP_Text>();
        }
        tMP_Text.fontSize = 50;
        tMP_Text.SetText(Settings.ini.Game.TextTooltip);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
