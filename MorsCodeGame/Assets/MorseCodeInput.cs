using UnityEngine;

public class MorseCodeInput : MonoBehaviour
{
    public float dotTimeThreshold = 0.3f; // 短按时间阈值 
    private float pressTime;

    public static bool isline = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCodeInput.keyCode)) // 假设使用空格键输入 
        {
            pressTime = Time.time;
        }
        if (Input.GetKeyUp(KeyCodeInput.keyCode))
        {
            float duration = Time.time - pressTime;
            if (duration < dotTimeThreshold)
            {
                // 短按，代表点 
                //Debug.Log("点");
            }
            else
            {
                // 长按，代表划 
                //Debug.Log("划");
            }
        }
    }
}