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
    private bool pressed;
    private RectTransform lockedMorseObject; // 锁定的摩尔斯码物体
    private bool hasProcessedPress; // 是否已经处理过按下事件

    void Update()
    {
        if (Input.GetKeyDown(KeyCodeInput.keyCode))
        {
            pressed = true;
            pressTime = Time.time;
            hasProcessedPress = false;

            // 在按下时锁定最近的物体
            lockedMorseObject = FindClosestMorseCodeObject();

            // 如果是简单模式，直接处理按下事件
            if (easyMode)
            {
                DetectKeyPress(true, 0f, lockedMorseObject);
                hasProcessedPress = true;
            }
        }

        if (Input.GetKeyUp(KeyCodeInput.keyCode))
        {
            pressed = false;
            float duration = Time.time - pressTime;

            // 使用锁定的物体进行判定
            if (duration <= dotDathTime)
            {
                // 点击
                DetectKeyPress(true, duration, lockedMorseObject);
            }
            else
            {
                // 长按（如果在按住期间没有处理过）
                if (!hasProcessedPress)
                {
                    DetectKeyPress(false, duration, lockedMorseObject);
                }
            }

            // 清除锁定的物体
            lockedMorseObject = null;
            hasProcessedPress = false;
        }

        if (pressed)
        {
            float duration = Time.time - pressTime;
            // 长按判定：超过点击时间且小于0.3秒，且还没处理过
            if (duration > dotDathTime && duration < 0.3f && !hasProcessedPress)
            {
                DetectKeyPress(false, duration, lockedMorseObject);
                hasProcessedPress = true; // 标记已处理，避免重复处理
            }
        }
    }

    void DetectKeyPress(bool isdot, float pressedTime = 0f, RectTransform targetObject = null)
    {
        // 使用传入的目标物体，如果没有则查找最近的
        RectTransform morseCodeObject = targetObject ?? FindClosestMorseCodeObject();

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