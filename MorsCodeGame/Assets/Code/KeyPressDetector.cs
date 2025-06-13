using System;
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

    [Header("容错时间设置")]
    [Tooltip("容错时间窗口开始时间（秒）")]
    public float toleranceTimeStart = 0.1f;
    [Tooltip("容错时间窗口结束时间（秒）")]
    public float toleranceTimeEnd = 0.2f;

    [Header("扩展检测设置")]
    public float visualDetectionRange = 800f;
    public bool enableMissedObjectRecovery = true;

    [Header("长按反馈设置")]
    [Tooltip("长按反馈触发时间（秒）")]
    public float longPressFeedbackTime = 0.3f;
    [Tooltip("长按时的颜色")]
    public Color longPressColor = Color.cyan;
    [Tooltip("长按反馈持续时间")]
    public float feedbackDuration = 0.4f;

    [Header("长按进度设置")]
    [Tooltip("长按进度最大UV X值（负值）")]
    public float maxProgressUV = -0.5f;
    [Tooltip("是否启用长按进度效果")]
    public bool enableProgressEffect = true;

    [Header("音频反馈")]
    public AudioSource audioSource;
    public AudioClip dotPressSound;
    public AudioClip dashPressSound;
    public AudioClip longPressFeedbackSound;
    public AudioClip toleranceHitSound;

    [Header("效果图像")]
    public RawImage effectImage;

    private void Start()
    {
        Settings.ini.Game.EazyMode = Settings.ini.Game.EazyMode;
        easyMode = Settings.ini.Game.EazyMode;
        Settings.ini.Game.DotTime = Settings.ini.Game.DotTime;
        dotDathTime = Settings.ini.Game.DotTime;

        // 确保音频组件存在
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();


    }

    private void OnEnable()
    {
        // 初始化effectImage循环滚动
        InitializeEffectImageScroll();
    }

    private float pressTime;
    private bool pressed;
    private RectTransform lockedMorseObject;
    private bool hasProcessedPress;
    private bool hasProcessedLongPress;
    private bool hasTriggeredLongPressFeedback; // 是否已触发长按反馈
    private Coroutine longPressFeedbackCoroutine;

    // 长按进度相关变量
    private RawImage currentProgressImage;
    private Rect originalUVRect;
    private bool isShowingProgress;

    // effectImage相关变量
    private Rect originalEffectUVRect;
    private bool isShowingEffectProgress;
    private Coroutine effectProgressCoroutine;

    [Header("Effect Image循环滚动设置")]
    [Tooltip("滚动速度（每秒滚动的UV单位）")]
    public float scrollSpeed = 1.0f;
    [Tooltip("是否启用循环滚动")]
    public bool enableContinuousScroll = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCodeInput.keyCode))
        {
            pressed = true;
            pressTime = Time.time;
            hasProcessedPress = false;
            hasProcessedLongPress = false;
            hasTriggeredLongPressFeedback = false;

            // 优先查找最接近触发线的物体
            lockedMorseObject = FindBestMorseCodeObject();

            // 开始长按进度效果
            if (enableProgressEffect && lockedMorseObject != null)
            {
                StartProgressEffect(lockedMorseObject);
            }

            // 如果是简单模式，直接处理按下事件
            if (easyMode && lockedMorseObject != null)
            {
                DetectKeyPress(true, 0f, lockedMorseObject);
                hasProcessedPress = true;
            }

            // 播放按下音效
            PlayPressSound(true); // 默认播放点按音效
        }

        if (Input.GetKeyUp(KeyCodeInput.keyCode) && pressed)
        {
            pressed = false;
            float duration = Time.time - pressTime;

            // 停止长按反馈
            if (longPressFeedbackCoroutine != null)
            {
                StopCoroutine(longPressFeedbackCoroutine);
                longPressFeedbackCoroutine = null;
            }

            // 停止长按进度效果
            StopProgressEffect();

            // 如果没有锁定物体，尝试重新查找
            if (lockedMorseObject == null && enableMissedObjectRecovery)
            {
                lockedMorseObject = FindBestMorseCodeObject();
                Debug.Log("恢复查找到物体: " + (lockedMorseObject != null));
            }

            // 确保有锁定的物体才进行处理
            if (lockedMorseObject != null)
            {
                bool isDotPress = DetermineInputType(duration);

                if (!hasProcessedPress && !hasProcessedLongPress)
                {
                    DetectKeyPress(isDotPress, duration, lockedMorseObject);
                    if (isDotPress)
                        hasProcessedPress = true;
                    else
                        hasProcessedLongPress = true;
                }
            }
            else
            {
                Debug.LogWarning("按键释放时没有找到目标物体");
            }

            // 清除状态
            lockedMorseObject = null;
            hasProcessedPress = false;
            hasProcessedLongPress = false;
            hasTriggeredLongPressFeedback = false;
        }

        // 长按检测和反馈
        if (pressed && lockedMorseObject != null)
        {
            float duration = Time.time - pressTime;

            // 更新长按进度效果
            if (enableProgressEffect && isShowingProgress)
            {
                UpdateProgressEffect(duration);
            }

            // 长按反馈触发
            if (duration >= longPressFeedbackTime && !hasTriggeredLongPressFeedback)
            {
                TriggerLongPressFeedback();
                hasTriggeredLongPressFeedback = true;
            }

            // 长按逻辑处理
            if (duration > dotDathTime && !hasProcessedLongPress && !easyMode)
            {
                // 检查是否在容错时间窗口内
                bool isInToleranceWindow = duration >= toleranceTimeStart && duration <= toleranceTimeEnd;

                if (!isInToleranceWindow)
                {
                    DetectKeyPress(false, duration, lockedMorseObject);
                    hasProcessedLongPress = true;
                }
            }
        }
    }

    Coroutine waveCroutine;
    /// <summary>
    /// 初始化effectImage循环滚动
    /// </summary>
    private void InitializeEffectImageScroll()
    {
        if (effectImage != null && enableContinuousScroll)
        {
            originalEffectUVRect = effectImage.uvRect;

            if (waveCroutine != null)
            {
                StopCoroutine(waveCroutine);
                waveCroutine = null;
            }

            waveCroutine = StartCoroutine(ContinuousScrollCoroutine());
        }
    }

    /// <summary>
    /// 持续循环滚动效果
    /// </summary>
    private System.Collections.IEnumerator ContinuousScrollCoroutine()
    {
        while (effectImage != null && enableContinuousScroll)
        {
            // 计算UV X偏移
            float uvOffset = (Time.time * scrollSpeed) % 1.0f;

            // 更新UV Rect
            Rect newUVRect = originalEffectUVRect;
            newUVRect.x = -uvOffset; // 负值滚动
            effectImage.uvRect = newUVRect;

            yield return null;
        }
    }

    /// <summary>
    /// 开始长按进度效果
    /// </summary>
    private void StartProgressEffect(RectTransform targetObject)
    {
        if (targetObject == null) return;

        // 获取物体的ItemPrefab组件
        ItemPrefab itemPrefab = targetObject.GetComponent<ItemPrefab>();
        if (itemPrefab == null) return;

        RawImage rawImage = targetObject.GetComponent<RawImage>();
        if (rawImage == null) return;

        currentProgressImage = rawImage;
        originalUVRect = rawImage.uvRect;
        isShowingProgress = true;

        Debug.Log("开始长按进度效果");
    }

    /// <summary>
    /// 更新长按进度效果
    /// </summary>
    private void UpdateProgressEffect(float pressDuration)
    {
        if (currentProgressImage == null || !isShowingProgress) return;

        // 计算进度比例 (0 到 dotDashTime 之间)
        float progress = Mathf.Clamp01(pressDuration / dotDathTime);

        // 更新UV Rect的X值，从0减少到maxProgressUV（负值）
        Rect newUVRect = originalUVRect;
        newUVRect.x = progress * maxProgressUV;
        currentProgressImage.uvRect = newUVRect;

        // Debug.Log($"更新进度: {progress:F2}, UV X: {newUVRect.x:F2}");
    }

    /// <summary>
    /// 停止长按进度效果
    /// </summary>
    private void StopProgressEffect()
    {
        if (currentProgressImage != null && isShowingProgress)
        {
            // 恢复原始UV Rect
            currentProgressImage.uvRect = originalUVRect;
            Debug.Log("停止长按进度效果，恢复原始UV");
        }

        currentProgressImage = null;
        isShowingProgress = false;
    }

    /// <summary>
    /// 根据按下时间和容错机制确定输入类型
    /// </summary>
    private bool DetermineInputType(float duration)
    {
        // 容错时间窗口：0.1-0.15秒内不区分点按和长按
        if (duration >= toleranceTimeStart && duration <= toleranceTimeEnd)
        {
            Debug.Log($"容错时间窗口命中 - 时长: {duration:F3}s");
            return true; // 容错窗口内默认为点按
        }

        // 正常判定逻辑
        return duration <= dotDathTime;
    }

    /// <summary>
    /// 触发长按反馈
    /// </summary>
    private void TriggerLongPressFeedback()
    {
        Debug.Log("触发长按反馈");

        // 音频反馈
        if (audioSource && longPressFeedbackSound)
        {
            audioSource.PlayOneShot(longPressFeedbackSound, 0.8f);
        }

        // 视觉反馈
        if (lockedMorseObject != null)
        {
            longPressFeedbackCoroutine = StartCoroutine(ShowLongPressFeedback(lockedMorseObject));
        }
    }

    /// <summary>
    /// 显示长按视觉反馈
    /// </summary>
    private System.Collections.IEnumerator ShowLongPressFeedback(RectTransform target)
    {
        RawImage targetImage = target.GetComponent<RawImage>();
        if (targetImage == null) yield break;

        Color originalColor = targetImage.color;

        // 渐变到长按颜色
        float elapsed = 0f;
        while (elapsed < feedbackDuration)
        {
            yield return null;
            try
            {
                elapsed += Time.deltaTime;
                float t = elapsed / feedbackDuration;
                targetImage.color = Color.Lerp(originalColor, longPressColor, Mathf.Sin(t * Mathf.PI * 2f) * 0.5f + 0.5f);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        // 恢复原色
        targetImage.color = originalColor;
    }

    void DetectKeyPress(bool isdot, float pressedTime = 0f, RectTransform targetObject = null)
    {
        RectTransform morseCodeObject = targetObject ?? FindBestMorseCodeObject();

        if (morseCodeObject == null)
        {
            Debug.LogWarning("DetectKeyPress: 没有找到摩尔斯码物体");
            return;
        }

        float distance = GetDistance(morseCodeObject);
        ItemPrefab morseCode = morseCodeObject.GetComponent<ItemPrefab>();

        if (morseCode == null)
        {
            Debug.LogWarning("DetectKeyPress: 物体没有ItemPrefab组件");
            return;
        }

        bool isDotObj = morseCode.isDot;
        bool isInToleranceWindow = pressedTime >= toleranceTimeStart && pressedTime <= toleranceTimeEnd;

        Debug.Log($"按键检测 - 距离: {distance}, 输入: {(isdot ? "点" : "划")}, 物体: {(isDotObj ? "点" : "划")}, 时长: {pressedTime:F3}s, 容错: {isInToleranceWindow}");

        // 使用更宽松的检测范围
        bool isInRange = distance < (enableMissedObjectRecovery ? visualDetectionRange : ignoreTimeRange);

        if (isInRange)
        {
            // 设置按下的字符
            SetMorseCharacter(morseCode, isdot, isDotObj, isInToleranceWindow);

            // 判定逻辑
            bool isCorrectType = isDotObj == isdot || isInToleranceWindow; // 容错窗口内总是正确

            if (distance < perfectTimeRange && isCorrectType)
            {
                Debug.Log(isInToleranceWindow ? "容错完美命中!" : "完美命中!");
                morseCodeObject.GetComponent<RawImage>().color = Color.green;

                // 播放容错命中音效
                if (isInToleranceWindow && audioSource && toleranceHitSound)
                {
                    audioSource.PlayOneShot(toleranceHitSound, 0.6f);
                }
            }
            else if (distance < normalTimeRange && isCorrectType)
            {
                Debug.Log(isInToleranceWindow ? "容错良好命中!" : "良好命中!");
                morseCodeObject.GetComponent<RawImage>().color = Color.yellow;

                // 播放容错命中音效
                if (isInToleranceWindow && audioSource && toleranceHitSound)
                {
                    audioSource.PlayOneShot(toleranceHitSound, 0.6f);
                }
            }
            else if (!isCorrectType && !isInToleranceWindow)
            {
                if (isDotObj && !isdot)
                {
                    morseCodeObject.GetComponent<RawImage>().color = Color.red;
                    Debug.Log("点按成长按");
                }
                else if (!isDotObj && isdot && pressedTime > 0 && pressedTime < dotDathTime)
                {
                    morseCodeObject.GetComponent<RawImage>().color = Color.red;
                    Debug.Log("线段按的时间不够");
                }
                else
                {
                    morseCodeObject.GetComponent<RawImage>().color = Color.red;
                    Debug.Log("类型不匹配");
                }
            }
            else
            {
                Debug.Log("距离过远但在检测范围内");
                morseCodeObject.GetComponent<RawImage>().color = Color.red;
            }

            // 简单模式下总是显示绿色
            if (easyMode)
            {
                morseCodeObject.GetComponent<RawImage>().color = Color.green;
            }
        }
        else
        {
            Debug.Log($"按键被忽略 - 距离过远: {distance} > {(enableMissedObjectRecovery ? visualDetectionRange : ignoreTimeRange)}");
            // 即使距离过远，也要设置字符，避免显示为空
            SetMorseCharacter(morseCode, isdot, isDotObj, isInToleranceWindow);
        }
    }

    private void SetMorseCharacter(ItemPrefab morseCode, bool isdot, bool isDotObj, bool isInToleranceWindow)
    {
        if (easyMode)
        {
            // 简单模式：根据物体类型设置正确的字符
            morseCode.pressDotChar = isDotObj ? '.' : '-';
        }
        else if (isInToleranceWindow)
        {
            // 容错模式：根据物体类型设置正确的字符（忽略玩家输入）
            morseCode.pressDotChar = isDotObj ? '.' : '-';
            Debug.Log($"容错模式设置字符: {morseCode.pressDotChar}");
        }
        else
        {
            // 普通模式：根据玩家输入设置字符
            morseCode.pressDotChar = isdot ? '.' : '-';
        }

        Debug.Log($"设置字符: {morseCode.pressDotChar} (物体ID: {morseCode.id})");
    }

    private float GetDistance(RectTransform morseCodeObject)
    {
        Vector2 morseCodeObjectScreenPosition = morseCodeObject.anchoredPosition;
        Vector2 triggerLineScreenPosition = triggerLine.anchoredPosition;
        float distance = Mathf.Abs(triggerLineScreenPosition.x - morseCodeObjectScreenPosition.x);
        return distance;
    }

    RectTransform FindBestMorseCodeObject()
    {
        RectTransform closestObject = null;
        float closestDistance = float.MaxValue;
        float bestScore = float.MaxValue;

        ItemPrefab[] morseCodeObjects = SpawnPointRoot.GetComponentsInChildren<ItemPrefab>();

        if (morseCodeObjects.Length == 0)
        {
            Debug.LogWarning("没有找到摩尔斯码物体");
            return null;
        }

        foreach (ItemPrefab morseCodeObject in morseCodeObjects)
        {
            if (morseCodeObject == null) continue;

            RectTransform rectTransform = morseCodeObject.GetComponent<RectTransform>();
            if (rectTransform == null) continue;

            float distance = GetDistance(rectTransform);
            float score = distance;

            // 如果物体已经有按键记录且不是空字符，降低优先级
            if (morseCodeObject.pressDotChar != '\0' && morseCodeObject.pressDotChar != ' ')
            {
                score += 1000f;
            }

            if (score < bestScore)
            {
                bestScore = score;
                closestDistance = distance;
                closestObject = rectTransform;
            }
        }

        if (closestObject != null)
        {
            Debug.Log($"选择物体 - 距离: {closestDistance}, ID: {closestObject.GetComponent<ItemPrefab>()?.id}");
        }

        return closestObject;
    }

    // 音效播放方法
    void PlayPressSound(bool isDot)
    {
        if (!audioSource) return;

        AudioClip clip = isDot ? dotPressSound : dashPressSound;
        if (clip) audioSource.PlayOneShot(clip, 0.7f);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 确保容错时间窗口设置合理
        if (toleranceTimeStart < 0) toleranceTimeStart = 0;
        if (toleranceTimeEnd < toleranceTimeStart) toleranceTimeEnd = toleranceTimeStart + 0.05f;
        if (longPressFeedbackTime < toleranceTimeEnd) longPressFeedbackTime = toleranceTimeEnd + 0.1f;

        // 确保进度UV值在合理范围内（负值）
        if (maxProgressUV > 0) maxProgressUV = -0.5f;
        if (maxProgressUV < -1) maxProgressUV = -1f;
    }
#endif
}