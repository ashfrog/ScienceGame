using UnityEngine;
using UnityEngine.UI;

public class ResetSliderValue : MonoBehaviour
{

    [SerializeField]
    float value = 0.1f;
    private void OnEnable()
    {
        GetComponent<Slider>().value = value;
    }
}
