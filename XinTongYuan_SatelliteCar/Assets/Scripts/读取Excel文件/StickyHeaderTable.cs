using UnityEngine;
using UnityEngine.UI;

public class StickyHeaderTable : MonoBehaviour
{
    public GameObject headerRow; // 你的标题行对象
    public ScrollRect scrollRect; // 表格的Scroll Rect组件
    public RectTransform tableContent; // 表格内容对象的RectTransform

    public Vector2 initialHeaderPosition;

    void Start()
    {
        // 保存标题行的初始位置
        //initialHeaderPosition = headerRow.GetComponent<RectTransform>().anchoredPosition;
    }

    void Update()
    {
        if (headerRow != null)
        {
            // 使标题行在左右滑动时同步表格内容，但在上下滑动时保持在初始位置
            Vector2 contentPosition = tableContent.anchoredPosition;
            headerRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(initialHeaderPosition.x + contentPosition.x, initialHeaderPosition.y);
        }
    }

}