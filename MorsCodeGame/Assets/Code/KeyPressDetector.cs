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

    public bool easyMode;

    private void Start()
    {
        Settings.ini.Game.EazyMode = Settings.ini.Game.EazyMode;
        easyMode = Settings.ini.Game.EazyMode;
        Settings.ini.Game.DotTime = Settings.ini.Game.DotTime;
        dotDathTime = Settings.ini.Game.DotTime;
    }

    private void OnEnable()
    {

    }
    private float pressTime;
    bool pressed;
    void Update()
    {
        if (Input.GetKeyDown(KeyCodeInput.keyCode))
        {
            pressed = true;
            pressTime = Time.time;
            DetectKeyPress(true, 0f);

        }
        if (Input.GetKeyUp(KeyCodeInput.keyCode))
        {
            pressed = false;
            float duration = Time.time - pressTime;
            if (duration <= dotDathTime)
            {
                DetectKeyPress(true, duration);
            }
        }
        if (pressed)
        {
            float duration = Time.time - pressTime;
            if (duration > dotDathTime && duration < 0.3f && pressed)
            {
                DetectKeyPress(false, duration);
                pressed = false;
            }
            //Debug.Log(duration);
        }




    }

    void DetectKeyPress(bool isdot, float pressedTime = 0f)
    {
        RectTransform morseCodeObject = FindClosestMorseCodeObject();
        if (morseCodeObject != null)
        {
            float distance = GetDistance(morseCodeObject);
            //Debug.Log("Distance: " + distance);
            ItemPrefab morseCode = morseCodeObject.GetComponent<ItemPrefab>();
            bool isDotObj = morseCode.isDot;

            if (distance < ignoreTimeRange)
            {
                if (distance < perfectTimeRange && isDotObj == isdot)
                {
                    //Debug.Log("Perfect");
                    morseCodeObject.GetComponent<RawImage>().color = Color.green;
                }
                else if (distance < normalTimeRange && isDotObj == isdot)
                {
                    //Debug.Log("Normal");
                    morseCodeObject.GetComponent<RawImage>().color = Color.yellow;
                }
                else if (isDotObj != isdot)
                {
                    if (isDotObj)
                    {
                        morseCodeObject.GetComponent<RawImage>().color = Color.red;
                        Debug.Log("点按成长按");
                    }
                    else if (pressedTime > 0 && pressedTime < dotDathTime) //线段长按时间不够
                    {
                        morseCodeObject.GetComponent<RawImage>().color = Color.red;
                        Debug.Log("线段按的时间不够");
                    }

                }
                else
                {
                    Debug.Log("其它");
                    morseCodeObject.GetComponent<RawImage>().color = Color.red;
                }
                if (easyMode)
                {
                    morseCodeObject.GetComponent<RawImage>().color = Color.green;
                }
            }
            if (easyMode)
            {
                morseCode.pressDotChar = isDotObj ? '.' : '-';
            }
            else
            {
                morseCode.pressDotChar = isdot ? '.' : '-';
            }
            //Debug.Log(morseCodeObject.GetComponent<ItemPrefab>().id);
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
