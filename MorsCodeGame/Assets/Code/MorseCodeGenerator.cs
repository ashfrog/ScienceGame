using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// handle the generation and scrolling of Morse code.
/// </summary>
public class MorseCodeGenerator : MonoBehaviour
{
    public GameObject dotPrefab;
    public GameObject dashPrefab;
    public RectTransform spawnPoint;
    public float scrollSpeed = 100f;
    public float spawnInterval = 0.5f;

    public List<GameObject> morseCodeObjects = new List<GameObject>();
    private float timer;


    public static Dictionary<string, char> MorseToCharMap = new Dictionary<string, char>
    {
        { ".-", 'A' }, { "-...", 'B' }, { "-.-.", 'C' }, { "-..", 'D' },
        { ".", 'E' }, { "..-.", 'F' }, { "--.", 'G' }, { "....", 'H' },
        { "..", 'I' }, { ".---", 'J' }, { "-.-", 'K' }, { ".-..", 'L' },
        { "--", 'M' }, { "-.", 'N' }, { "---", 'O' }, { ".--.", 'P' },
        { "--.-", 'Q' }, { ".-.", 'R' }, { "...", 'S' }, { "-", 'T' },
        { "..-", 'U' }, { "...-", 'V' }, { ".--", 'W' }, { "-..-", 'X' },
        { "-.--", 'Y' }, { "--..", 'Z' },
        { "-----", '0' }, { ".----", '1' }, { "..---", '2' }, { "...--", '3' },
        { "....-", '4' }, { ".....", '5' }, { "-....", '6' }, { "--...", '7' },
        { "---..", '8' }, { "----.", '9' }
    };

    public static Dictionary<char, string> CharMapToMorse = new Dictionary<char, string>
    {
        { 'A', ".-" }, { 'B', "-..." }, { 'C', "-.-." }, { 'D', "-.." },
        { 'E', "." }, { 'F', "..-." }, { 'G', "--." }, { 'H', "...." },
        { 'I', ".." }, { 'J', ".---" }, { 'K', "-.-" }, { 'L', ".-.." },
        { 'M', "--" }, { 'N', "-." }, { 'O', "---" }, { 'P', ".--." },
        { 'Q', "--.-" }, { 'R', ".-." }, { 'S', "..." }, { 'T', "-" },
        { 'U', "..-" }, { 'V', "...-" }, { 'W', ".--" }, { 'X', "-..-" },
        { 'Y', "-.--" }, { 'Z', "--.." },
        { '0', "-----" }, { '1', ".----" }, { '2', "..---" }, { '3', "...--" },
        { '4', "....-" }, { '5', "....." }, { '6', "-...." }, { '7', "--..." },
        { '8', "---.." }, { '9', "----." }
    };

    void Start()
    {

    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnMorseCode();
            timer = 0f;
        }

        ScrollMorseCode();
    }

    void SpawnMorseCode()
    {
        GameObject prefab = Random.Range(0, 2) == 0 ? dotPrefab : dashPrefab;
        GameObject morseCodeObject = Instantiate(prefab, spawnPoint.position, Quaternion.identity, transform);
        morseCodeObject.transform.SetParent(spawnPoint, false);
        morseCodeObjects.Add(morseCodeObject);
    }

    void ScrollMorseCode()
    {
        for (int i = morseCodeObjects.Count - 1; i >= 0; i--)
        {
            GameObject morseCodeObject = morseCodeObjects[i];
            morseCodeObject.GetComponent<RectTransform>().anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;

            if (morseCodeObject.GetComponent<RectTransform>().anchoredPosition.x < -Screen.width / 2)
            {
                Destroy(morseCodeObject);
                morseCodeObjects.RemoveAt(i);
            }
        }
    }
}
