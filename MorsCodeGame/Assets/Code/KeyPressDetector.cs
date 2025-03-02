using UnityEngine;

/// <summary>
///  handle the detection of key presses when the Morse code reaches a trigger line.
/// </summary>
public class KeyPressDetector : MonoBehaviour
{
    public float perfectTimeRange = 0.1f;
    public float normalTimeRange = 0.2f;
    public Transform triggerLine;

    public Transform SpawnPointRoot;

    void Update()
    {
        if (Input.GetKeyDown(KeyCodeInput.keyCode))
        {
            DetectKeyPress();
        }
    }

    void DetectKeyPress()
    {
        GameObject morseCodeObject = FindClosestMorseCodeObject();
        if (morseCodeObject != null)
        {
            Vector2 morseCodeObjectScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, morseCodeObject.transform.position);
            Vector2 triggerLineScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, triggerLine.position);
            float distance = Vector2.Distance(morseCodeObjectScreenPosition, triggerLineScreenPosition);
            Debug.Log("Distance: " + distance);
            Debug.Log("PerfectTimeRange: " + perfectTimeRange);
            if (distance < perfectTimeRange)
            {
                Debug.Log("Perfect");
            }
            else if (distance < normalTimeRange)
            {
                Debug.Log("Normal");
            }
            else
            {
                Debug.Log("Miss");
            }
        }
    }

    GameObject FindClosestMorseCodeObject()
    {
        GameObject closestObject = null;
        float closestDistance = float.MaxValue;

        foreach (Transform child in SpawnPointRoot)
        {
            Vector2 childScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, child.position);
            Vector2 triggerLineScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, triggerLine.position);
            float distance = Vector2.Distance(childScreenPosition, triggerLineScreenPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = child.gameObject;
            }
        }

        return closestObject;
    }
}
