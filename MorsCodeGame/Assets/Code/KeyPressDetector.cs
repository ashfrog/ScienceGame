using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 改进的按键检测器 - 增强反馈和可玩性
/// </summary>
public class KeyPressDetector : MonoBehaviour
{
    [Header("判定区间设置")]
    public float perfectTimeRange = 450f; // 增加宽容度
    public float normalTimeRange = 650f;  // 增加宽容度
    public float ignoreTimeRange = 800f;  // 增加宽容度

    [Header("按键时间设置")]
    public float dotDashTime = 0.25f; // 稍微增加点击时间

    [Header("音效设置")]
    public AudioSource audioSource;
    public AudioClip dotPressSound;
    public AudioClip dashPressSound;
    public AudioClip perfectSound;
    public AudioClip goodSound;
    public AudioClip missSound;
    public AudioClip comboSound;

    [Header("视觉反馈设置")]
    public GameObject feedbackTextPrefab;
    public Transform feedbackParent;
    public ParticleSystem perfectEffect;
    public ParticleSystem goodEffect;

    [Header("游戏对象引用")]
    public RectTransform triggerLine;
    public RectTransform SpawnPointRoot;
    public bool easyMode;

    // 连击系统
    private int comboCount = 0;
    private int maxCombo = 0;
    private float lastSuccessTime = 0f;

    // 统计数据
    private int totalHits = 0;
    private int perfectHits = 0;
    private int goodHits = 0;
    private int missHits = 0;

    // 原有变量
    private float pressTime;
    private bool pressed;
    private RectTransform lockedMorseObject;
    private bool hasProcessedPress;

    // UI反馈
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI accuracyText;
    public Slider accuracySlider;

    void Start()
    {
        InitializeSettings();
        InitializeUI();
    }

    void InitializeSettings()
    {
        Settings.ini.Game.EazyMode = Settings.ini.Game.EazyMode;
        easyMode = Settings.ini.Game.EazyMode;
        Settings.ini.Game.DotTime = Settings.ini.Game.DotTime;
        dotDashTime = Settings.ini.Game.DotTime;

        // 简单模式下更宽松的判定
        if (easyMode)
        {
            perfectTimeRange *= 1.5f;
            normalTimeRange *= 1.5f;
            ignoreTimeRange *= 1.3f;
        }
    }

    void InitializeUI()
    {
        if (comboText) comboText.gameObject.SetActive(false);
        UpdateAccuracy();
    }

    void Update()
    {
        HandleInput();
        UpdateComboDisplay();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCodeInput.keyCode))
        {
            OnKeyPress();
        }

        if (Input.GetKeyUp(KeyCodeInput.keyCode))
        {
            OnKeyRelease();
        }

        if (pressed)
        {
            HandleLongPress();
        }
    }

    void OnKeyPress()
    {
        pressed = true;
        pressTime = Time.time;
        hasProcessedPress = false;
        lockedMorseObject = FindClosestMorseCodeObject();

        // 视觉反馈：按下动画
        if (lockedMorseObject != null)
        {
            StartCoroutine(PressAnimation(lockedMorseObject));
        }

        // 音效反馈
        PlayPressSound(true); // 预播放点击音效

        // 简单模式立即处理
        if (easyMode)
        {
            DetectKeyPress(true, 0f, lockedMorseObject);
            hasProcessedPress = true;
        }
    }

    void OnKeyRelease()
    {
        pressed = false;
        float duration = Time.time - pressTime;

        bool isDot = duration <= dotDashTime;

        // 播放对应音效
        PlayPressSound(isDot);

        // 执行判定
        if (duration <= dotDashTime)
        {
            DetectKeyPress(true, duration, lockedMorseObject);
        }
        else if (!hasProcessedPress)
        {
            DetectKeyPress(false, duration, lockedMorseObject);
        }

        // 清理状态
        lockedMorseObject = null;
        hasProcessedPress = false;
    }

    void HandleLongPress()
    {
        float duration = Time.time - pressTime;
        if (duration > dotDashTime && duration < 0.4f && !hasProcessedPress)
        {
            DetectKeyPress(false, duration, lockedMorseObject);
            hasProcessedPress = true;
        }
    }

    void DetectKeyPress(bool isDot, float pressedTime = 0f, RectTransform targetObject = null)
    {
        RectTransform morseCodeObject = targetObject ?? FindClosestMorseCodeObject();

        if (morseCodeObject != null)
        {
            float distance = GetDistance(morseCodeObject);
            ItemPrefab morseCode = morseCodeObject.GetComponent<ItemPrefab>();
            bool isDotObj = morseCode.isDot;

            // 判定逻辑
            JudgmentResult result = CalculateJudgment(distance, isDot, isDotObj, pressedTime);

            // 应用判定结果
            ApplyJudgmentResult(morseCodeObject, result);

            // 更新统计
            UpdateStatistics(result);

            // 设置字符
            SetMorseCharacter(morseCode, isDot, isDotObj);
        }
    }

    JudgmentResult CalculateJudgment(float distance, bool isDot, bool isDotObj, float pressedTime)
    {
        JudgmentResult result = new JudgmentResult();

        if (distance >= ignoreTimeRange)
        {
            result.type = JudgmentType.Miss;
            result.message = "太远了";
            return result;
        }

        // 类型匹配检查
        bool typeMatch = isDotObj == isDot;

        // 特殊情况：长按时间不够的处理
        if (!isDotObj && pressedTime > 0 && pressedTime < dotDashTime)
        {
            result.type = JudgmentType.Miss;
            result.message = "长按时间不够";
            return result;
        }

        if (!typeMatch)
        {
            // 容错机制：如果距离很近，给予Good判定而不是Miss
            if (distance < perfectTimeRange * 1.2f)
            {
                result.type = JudgmentType.Good;
                result.message = isDotObj ? "应该点击" : "应该长按";
            }
            else
            {
                result.type = JudgmentType.Miss;
                result.message = isDotObj ? "应该点击" : "应该长按";
            }
            return result;
        }

        // 距离判定
        if (distance < perfectTimeRange)
        {
            result.type = JudgmentType.Perfect;
            result.message = "完美!";
        }
        else if (distance < normalTimeRange)
        {
            result.type = JudgmentType.Good;
            result.message = "不错!";
        }
        else
        {
            result.type = JudgmentType.Miss;
            result.message = "时机不对";
        }

        return result;
    }

    void ApplyJudgmentResult(RectTransform morseCodeObject, JudgmentResult result)
    {
        // 颜色反馈
        Color color = GetJudgmentColor(result.type);
        morseCodeObject.GetComponent<RawImage>().color = color;

        // 简单模式强制绿色
        if (easyMode && result.type != JudgmentType.Miss)
        {
            morseCodeObject.GetComponent<RawImage>().color = Color.green;
        }

        // 文字反馈
        ShowFeedbackText(result.message, morseCodeObject.position, color);

        // 音效反馈
        PlayJudgmentSound(result.type);

        // 粒子效果
        PlayJudgmentEffect(result.type, morseCodeObject.position);

        // 连击处理
        HandleCombo(result.type);
    }

    Color GetJudgmentColor(JudgmentType type)
    {
        switch (type)
        {
            case JudgmentType.Perfect: return Color.green;
            case JudgmentType.Good: return Color.yellow;
            case JudgmentType.Miss: return Color.red;
            default: return Color.white;
        }
    }

    void UpdateStatistics(JudgmentResult result)
    {
        totalHits++;
        switch (result.type)
        {
            case JudgmentType.Perfect: perfectHits++; break;
            case JudgmentType.Good: goodHits++; break;
            case JudgmentType.Miss: missHits++; break;
        }
        UpdateAccuracy();
    }

    void SetMorseCharacter(ItemPrefab morseCode, bool isDot, bool isDotObj)
    {
        if (easyMode)
        {
            morseCode.pressDotChar = isDotObj ? '.' : '-';
        }
        else
        {
            morseCode.pressDotChar = isDot ? '.' : '-';
        }
    }

    void HandleCombo(JudgmentType type)
    {
        if (type == JudgmentType.Perfect || type == JudgmentType.Good)
        {
            comboCount++;
            if (comboCount > maxCombo)
                maxCombo = comboCount;
            lastSuccessTime = Time.time;

            // 连击奖励
            if (comboCount >= 5 && comboCount % 5 == 0)
            {
                ShowComboBonus();
            }
        }
        else
        {
            comboCount = 0;
        }
    }

    void ShowComboBonus()
    {
        if (audioSource && comboSound)
            audioSource.PlayOneShot(comboSound);

        ShowFeedbackText($"连击 x{comboCount}!", Camera.main.WorldToScreenPoint(triggerLine.position), Color.cyan);
    }

    // 音效播放方法
    void PlayPressSound(bool isDot)
    {
        if (!audioSource) return;

        AudioClip clip = isDot ? dotPressSound : dashPressSound;
        if (clip) audioSource.PlayOneShot(clip, 0.7f);
    }

    void PlayJudgmentSound(JudgmentType type)
    {
        if (!audioSource) return;

        AudioClip clip = null;
        switch (type)
        {
            case JudgmentType.Perfect: clip = perfectSound; break;
            case JudgmentType.Good: clip = goodSound; break;
            case JudgmentType.Miss: clip = missSound; break;
        }

        if (clip) audioSource.PlayOneShot(clip);
    }

    void PlayJudgmentEffect(JudgmentType type, Vector3 position)
    {
        ParticleSystem effect = null;
        switch (type)
        {
            case JudgmentType.Perfect: effect = perfectEffect; break;
            case JudgmentType.Good: effect = goodEffect; break;
        }

        if (effect)
        {
            effect.transform.position = position;
            effect.Play();
        }
    }

    // 视觉反馈协程
    IEnumerator PressAnimation(RectTransform target)
    {
        Vector3 originalScale = target.localScale;
        Vector3 pressScale = originalScale * 1.1f;

        // 按下放大
        float timer = 0f;
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            target.localScale = Vector3.Lerp(originalScale, pressScale, timer / 0.1f);
            yield return null;
        }
    }

    void ShowFeedbackText(string text, Vector3 worldPos, Color color)
    {
        if (feedbackTextPrefab == null || feedbackParent == null) return;

        GameObject obj = Instantiate(feedbackTextPrefab, feedbackParent);

        // 转换世界坐标到屏幕坐标
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        obj.transform.position = screenPos;

        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        if (tmp)
        {
            tmp.text = text;
            tmp.color = color;
        }

        // 添加上升动画
        StartCoroutine(FeedbackTextAnimation(obj));
    }

    IEnumerator FeedbackTextAnimation(GameObject obj)
    {
        Vector3 startPos = obj.transform.position;
        Vector3 endPos = startPos + Vector3.up * 50f;

        float timer = 0f;
        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();

        while (timer < 1f)
        {
            timer += Time.deltaTime;
            obj.transform.position = Vector3.Lerp(startPos, endPos, timer);

            if (tmp)
            {
                Color color = tmp.color;
                color.a = 1f - timer;
                tmp.color = color;
            }

            yield return null;
        }

        Destroy(obj);
    }

    void UpdateComboDisplay()
    {
        if (comboText)
        {
            if (comboCount > 1)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"连击 x{comboCount}";
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    void UpdateAccuracy()
    {
        if (totalHits == 0) return;

        float accuracy = (float)(perfectHits + goodHits) / totalHits * 100f;

        if (accuracyText)
            accuracyText.text = $"准确率: {accuracy:F1}%";

        if (accuracySlider)
            accuracySlider.value = accuracy / 100f;
    }

    // 原有方法保持不变
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

    // 公共方法：获取游戏统计
    public GameStatistics GetStatistics()
    {
        return new GameStatistics
        {
            TotalHits = totalHits,
            PerfectHits = perfectHits,
            GoodHits = goodHits,
            MissHits = missHits,
            MaxCombo = maxCombo,
            Accuracy = totalHits > 0 ? (float)(perfectHits + goodHits) / totalHits : 0f
        };
    }
}

// 辅助类定义
public enum JudgmentType
{
    Perfect,
    Good,
    Miss
}

public struct JudgmentResult
{
    public JudgmentType type;
    public string message;
}

public struct GameStatistics
{
    public int TotalHits;
    public int PerfectHits;
    public int GoodHits;
    public int MissHits;
    public int MaxCombo;
    public float Accuracy;
}