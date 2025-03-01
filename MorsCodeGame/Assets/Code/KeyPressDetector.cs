using UnityEngine;

/// <summary>
///  handle the detection of key presses when the Morse code reaches a trigger line.
/// </summary>
public class KeyPressDetector : MonoBehaviour
{
    public float perfectTimeRange = 0.1f;
    public float normalTimeRange = 0.2f;
    public Transform triggerLine;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            DetectKeyPress();
        }
    }

    void DetectKeyPress()
    {
        GameObject morseCodeObject = FindClosestMorseCodeObject();
        if (morseCodeObject != null)
        {
            float distance = Mathf.Abs(morseCodeObject.transform.position.x - triggerLine.position.x);
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

        foreach (Transform child in transform)
        {
            float distance = Mathf.Abs(child.position.x - triggerLine.position.x);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = child.gameObject;
            }
        }

        return closestObject;
    }
}