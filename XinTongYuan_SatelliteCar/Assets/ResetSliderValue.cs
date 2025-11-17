using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResetSliderValue : MonoBehaviour
{

    [SerializeField]
    float value = 0.1f;

    [SerializeField]
    Toggle togglePlay;

    float increateWaitTime = 0.1f;

    Slider slider;

    Coroutine c;


    private void OnEnable()
    {
        slider = GetComponent<Slider>();
        slider.value = value;
        increateWaitTime = Settings.ini.Game.IncreateWaitTime;
    }

    private float curt = 0;

    private void Update()
    {
        curt += Time.deltaTime;
        if (curt >= increateWaitTime)
        {
            curt = 0;
            if (togglePlay.isOn && slider.value < 1) //选中表示正在播放
            {
                slider.value += 0.002f;
            }
        }
    }
}
