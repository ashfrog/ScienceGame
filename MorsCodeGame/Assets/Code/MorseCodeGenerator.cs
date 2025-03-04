using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.IO;

/// <summary>
/// handle the generation and scrolling of Morse code.
/// </summary>
public class MorseCodeGenerator : MonoBehaviour
{
    [SerializeField]
    TabSwitcher tabSwitcher;
    public GameObject dotPrefab;
    public GameObject dashPrefab;
    public GameObject emptyPrefab;

    public RectTransform spawnPoint;
    public RectTransform endPoint;
    public float scrollSpeed = 100f;
    public float spawnInterval = 0.5f;

    public List<GameObject> morseCodeObjects = new List<GameObject>();
    private float timer;
    private int currentMorseIndex = 0;

    public Action GameOver;

    static Dictionary<string, char> MorseToCharMap = new Dictionary<string, char>
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

    static Dictionary<char, string> CharMapToMorse = new Dictionary<char, string>
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

    string morseCode = "... --- ... --- ... --- ... --- ...";

    bool startgame;

    string gameState;

    void Start()
    {
        if (tabSwitcher == null)
        {
            tabSwitcher = GetComponentInParent<TabSwitcher>();
        }
    }

    private void OnEnable()
    {
        String morsecodesStr = System.IO.File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "morsecode.json"));
        List<String> morsecodes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(morsecodesStr);
        morseCode = morsecodes[UnityEngine.Random.Range(0, morsecodes.Count)];
        //延迟3秒开始游戏
        Invoke("StartGame", 1);
    }

    void StartGame()
    {
        currentMorseIndex = 0;
        startgame = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCodeInput.keyCode))
        {
            startgame = true;
        }
        if (startgame)
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                SpawnMorseCode();
                timer = 0f;
            }

            ScrollMorseCode();
        }

    }

    void SpawnMorseCode()
    {
        if (currentMorseIndex < morseCode.Length)
        {
            gameState = "playing";
            char currentChar = morseCode[currentMorseIndex];
            GameObject prefab;
            switch (currentChar)
            {
                case '.':
                    {
                        prefab = dotPrefab;
                    }
                    break;

                case '-':
                    {
                        prefab = dashPrefab;
                    }
                    break;
                default:
                    {
                        prefab = emptyPrefab;
                    }
                    break;
            }

            GameObject morseCodeObject = Instantiate(prefab);
            morseCodeObject.transform.SetParent(spawnPoint, false);
            morseCodeObject.GetComponent<RectTransform>().anchoredPosition = spawnPoint.anchoredPosition;
            morseCodeObjects.Add(morseCodeObject);
            currentMorseIndex++;
        }
        else
        {
            if (morseCodeObjects.Count <= 0 && "playing".EndsWith(gameState))
            {
                gameState = "end";
                // 触发游戏结束事件
                GameOver?.Invoke();
                Debug.Log("GameOver");
                tabSwitcher.SwitchTab(4);

            }
        }
    }

    void ScrollMorseCode()
    {
        for (int i = morseCodeObjects.Count - 1; i >= 0; i--)
        {
            GameObject morseCodeObject = morseCodeObjects[i];
            morseCodeObject.GetComponent<RectTransform>().anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;
            if (morseCodeObject.GetComponent<RectTransform>().anchoredPosition.x < endPoint.GetComponent<RectTransform>().anchoredPosition.x)
            {
                Destroy(morseCodeObject);
                morseCodeObjects.RemoveAt(i);
            }
        }
    }
}
