using UnityEngine;

public class ClickFeedback : MonoBehaviour
{
    public float perfectTimeRange = 0.1f; // Perfect 判定时间范围 
    public float normalTimeRange = 0.2f; // Normal 判定时间范围 
    private float targetTime; // 最佳点击时间 

    void Start()
    {
        // 假设这里设置最佳点击时间 
        targetTime = Time.time + 2f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float currentTime = Time.time;
            float timeDifference = Mathf.Abs(currentTime - targetTime);
            if (timeDifference < perfectTimeRange)
            {
                Debug.Log("Perfect");
            }
            else if (timeDifference < normalTimeRange)
            {
                Debug.Log("Normal");
            }
            else
            {
                Debug.Log("Miss");
            }
        }
    }
}