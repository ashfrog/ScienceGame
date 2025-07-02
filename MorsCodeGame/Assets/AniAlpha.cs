using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AniAlpha : MonoBehaviour
{
    [SerializeField]
    Image image;

    [Header("透明度设置")]
    [SerializeField, Range(0f, 1f)]
    float alphamin = 0.2f;  // 最小透明度

    [SerializeField, Range(0f, 1f)]
    float alphamax = 0.5f;  // 最大透明度

    [Header("动画设置")]
    [SerializeField]
    float speed = 1f;     // 动画速度

    [SerializeField]
    bool autoStart = true; // 是否自动开始动画

    private bool isAnimating = false;
    private float startTime;

    void Start()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        // 确保最小值不大于最大值
        if (alphamin > alphamax)
        {
            float temp = alphamin;
            alphamin = alphamax;
            alphamax = temp;
        }

        if (autoStart)
        {
            StartAnimation();
        }
    }

    /// <summary>
    /// 开始透明度PingPong动画
    /// </summary>
    public void StartAnimation()
    {
        isAnimating = true;
        startTime = Time.time;
    }

    /// <summary>
    /// 停止透明度动画
    /// </summary>
    public void StopAnimation()
    {
        isAnimating = false;
    }

    /// <summary>
    /// 暂停/恢复动画
    /// </summary>
    public void ToggleAnimation()
    {
        if (isAnimating)
            StopAnimation();
        else
            StartAnimation();
    }

    private void PingPong()
    {
        if (image == null || !isAnimating) return;

        // 使用Mathf.PingPong实现在min和max之间的往复运动
        float normalizedTime = (Time.time - startTime) * speed;
        float pingPongValue = Mathf.PingPong(normalizedTime, 1f);

        // 将0-1的值映射到alphamin-alphamax范围
        float currentAlpha = Mathf.Lerp(alphamin, alphamax, pingPongValue);

        // 获取当前颜色并修改alpha值
        Color currentColor = image.color;
        currentColor.a = currentAlpha;
        image.color = currentColor;
    }

    void Update()
    {
        PingPong();
    }

    /// <summary>
    /// 设置透明度范围
    /// </summary>
    /// <param name="min">最小透明度</param>
    /// <param name="max">最大透明度</param>
    public void SetAlphaRange(float min, float max)
    {
        alphamin = Mathf.Clamp01(min);
        alphamax = Mathf.Clamp01(max);

        // 确保最小值不大于最大值
        if (alphamin > alphamax)
        {
            float temp = alphamin;
            alphamin = alphamax;
            alphamax = temp;
        }
    }

    /// <summary>
    /// 设置动画速度
    /// </summary>
    /// <param name="newSpeed">新的动画速度</param>
    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Max(0f, newSpeed);
    }

    /// <summary>
    /// 重置动画到起始状态
    /// </summary>
    public void ResetAnimation()
    {
        startTime = Time.time;
        if (image != null)
        {
            Color currentColor = image.color;
            currentColor.a = alphamin;
            image.color = currentColor;
        }
    }
}