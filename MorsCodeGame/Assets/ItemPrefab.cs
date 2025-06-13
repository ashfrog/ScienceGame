using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPrefab : MonoBehaviour
{
    public bool isDot;
    public int id;
    public char pressDotChar = ' '; // 设置默认值为空格

    [Header("反馈设置")]
    public bool hasBeenProcessed = false; // 是否已被处理
    public float processTime = 0f; // 处理时间

    private void Awake()
    {
        // 确保pressDotChar有合理的默认值
        if (pressDotChar == '\0')
        {
            pressDotChar = ' ';
        }
    }

    /// <summary>
    /// 标记物体已被处理
    /// </summary>
    public void MarkAsProcessed(float time)
    {
        hasBeenProcessed = true;
        processTime = time;
    }

    /// <summary>
    /// 检查物体是否已被处理
    /// </summary>
    public bool IsProcessed()
    {
        return hasBeenProcessed && pressDotChar != ' ' && pressDotChar != '\0';
    }
}