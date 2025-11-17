using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResetSliderValue : MonoBehaviour
{

    [SerializeField]
    float value = 0.1f;

    float increateWaitTime = 0.1f;

    Slider slider;

    Coroutine c;


    private void OnEnable()
    {
        slider = GetComponent<Slider>();
        slider.value = value;
        increateWaitTime = Settings.ini.Game.IncreateWaitTime;

        if (c != null)
        {
            StopCoroutine(c);
        }
        c = StartCoroutine(AutoIncreate());
    }

    public IEnumerator AutoIncreate()
    {
        while (slider.value < 1)
        {
            yield return new WaitForSeconds(increateWaitTime);
            slider.value += 0.002f;
        }
    }
}
