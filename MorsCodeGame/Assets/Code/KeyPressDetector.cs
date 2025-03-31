using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///  handle the detection of key presses when the Morse code reaches a trigger line.
/// </summary>
public class KeyPressDetector : MonoBehaviour
{
    public float perfectTimeRange = 300f;
    public float normalTimeRange = 400f;
    public float ignoreTimeRange = 500f;

    /// <summary>
    /// 200毫秒内按下的是点按，否则是长按
    /// </summary>
    public float dotDathTime = 0.2f;

    public RectTransform triggerLine;

    public RectTransform SpawnPointRoot;

    [SerializeField]
    TextMeshProUGUI scoreText;

    int score;

    private void OnEnable()
    {
        score = 0;
    }
    private float pressTime;
    bool pressed;
    void Update()
    {
        if (Input.GetKeyDown(KeyCodeInput.keyCode))
        {
            pressed = true;
            pressTime = Time.time;

        }
        if (Input.GetKeyUp(KeyCodeInput.keyCode))
        {
            pressed = false;
            float duration = Time.time - pressTime;
            if (duration <= dotDathTime)
            {
                DetectKeyPress(true);
            }
        }
        if (pressed)
        {
            float duration = Time.time - pressTime;
            if (duration > dotDathTime && duration < 0.3f && pressed)
            {
                DetectKeyPress(false);
                pressed = false;
            }
            //Debug.Log(duration);
        }



        scoreText.text = "" + score;
    }

    void DetectKeyPress(bool isdot)
    {
        RectTransform morseCodeObject = FindClosestMorseCodeObject();
        if (morseCodeObject != null)
        {
            float distance = GetDistance(morseCodeObject);
            Debug.Log("Distance: " + distance);
            bool isDotObj = morseCodeObject.GetComponent<ItemPrefab>().isDot;
            if (distance < ignoreTimeRange)
            {
                if (distance < perfectTimeRange && isDotObj == isdot)
                {
                    Debug.Log("Perfect");
                    morseCodeObject.GetComponent<RawImage>().color = Color.green;
                }
                else if (distance < normalTimeRange && isDotObj == isdot)
                {
                    Debug.Log("Normal");
                    morseCodeObject.GetComponent<RawImage>().color = Color.yellow;
                }
                else
                {
                    Debug.Log("Miss");
                    morseCodeObject.GetComponent<RawImage>().color = Color.red;
                }
            }

        }
    }

    private float GetDistance(RectTransform morseCodeObject)
    {
        Vector2 morseCodeObjectScreenPosition = morseCodeObject.anchoredPosition;
        Vector2 triggerLineScreenPosition = triggerLine.anchoredPosition;
        float distance = Mathf.Abs(triggerLineScreenPosition.x - morseCodeObjectScreenPosition.x);
        return distance;
    }

    RectTransform FindClosestMorseCodeObject()
    {
        RectTransform closestObject = null;
        float closestDistance = float.MaxValue;

        foreach (ItemPrefab morseCodeObject in SpawnPointRoot.GetComponentsInChildren<ItemPrefab>())
        {
            float distance = GetDistance(morseCodeObject.GetComponent<RectTransform>());
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = morseCodeObject.GetComponent<RectTransform>();
            }
        }

        return closestObject;
    }
}
