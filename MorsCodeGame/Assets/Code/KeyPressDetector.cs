using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///  handle the detection of key presses when the Morse code reaches a trigger line.
/// </summary>
public class KeyPressDetector : MonoBehaviour
{
    public float perfectTimeRange = 40f;
    public float normalTimeRange = 80f;
    public float ignoreTimeRange = 120f;

    public RectTransform triggerLine;

    public RectTransform SpawnPointRoot;

    [SerializeField]
    TextMeshProUGUI scoreText;

    int score;

    private void OnEnable()
    {
        score = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCodeInput.keyCode))
        {
            DetectKeyPress();
        }

        scoreText.text = "" + score;
    }

    void DetectKeyPress()
    {
        RectTransform morseCodeObject = FindClosestMorseCodeObject();
        if (morseCodeObject != null)
        {
            float distance = GetDistance(morseCodeObject);
            Debug.Log("Distance: " + distance);
            if (distance < ignoreTimeRange)
            {

                if (distance < perfectTimeRange)
                {
                    Debug.Log("Perfect");
                    score++;
                    morseCodeObject.GetComponent<RawImage>().color = Color.green;
                }
                else if (distance < normalTimeRange)
                {
                    Debug.Log("Normal");

                    morseCodeObject.GetComponent<RawImage>().color = Color.yellow;
                }
                else
                {
                    Debug.Log("Miss");
                    score--;
                    morseCodeObject.GetComponent<RawImage>().color = Color.red;
                }
            }

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
