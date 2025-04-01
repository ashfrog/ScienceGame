using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.IO;
using System.Collections;
using TMPro;

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

    public int nextTab;

    public TextMeshProUGUI SecondText;

    public int prefabid;

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

    public string morseCode = "... --- ... --- ... --- ... --- ...";

    bool startgame;

    public enum GameState
    {
        prepare,
        starting,
        playing,
        end
    }

    private GameState gameState;


    void Start()
    {
        if (tabSwitcher == null)
        {
            tabSwitcher = GetComponentInParent<TabSwitcher>();
        }
    }

    private void OnEnable()
    {
        startgame = false;
        gameState = GameState.prepare;
        String morsecodesZHStrs = System.IO.File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "报文.json"));
        List<String> morsecodesZHs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(morsecodesZHStrs);
        string morseCodeZH = morsecodesZHs[UnityEngine.Random.Range(0, morsecodesZHs.Count)];
        String codesDicStr = System.IO.File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "中文电码表.json"));
        morseCode = ConvertZH2MorsCode(morseCodeZH, codesDicStr);
        SecondText.SetText(@"<size=60><bounce>轻击电键 开始发报</bounce></size>");
        SecondText.gameObject.SetActive(true);
    }

    /// <summary>
    /// 中文电报转换为摩尔斯电报
    /// </summary>
    /// <param name="morseCodeStr"></param>
    /// <returns></returns>
    private static string ConvertZH2MorsCode(string morseCodeStr, String codesDicStr)
    {
        Dictionary<string, string> codeDic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(codesDicStr);

        //将codesDirStr key value 翻转
        Dictionary<string, string> codeDicReverse = new Dictionary<string, string>();
        foreach (var item in codeDic)
        {
            codeDicReverse[item.Value] = item.Key;
        }
        string CodeNo = "";
        foreach (var item in morseCodeStr)
        {
            try
            {
                CodeNo += codeDicReverse[item.ToString()];
                CodeNo += ' ';
                CodeNo += ' ';
            }
            catch (Exception ex)
            {
                Debug.Log("中文电报编码成数字异常:" + ex.Message);
            }
        }
        //将数字电报码转换为摩尔斯码
        string morseCodeDt = "";
        foreach (var item in CodeNo)
        {
            try
            {
                if (char.IsWhiteSpace(item))
                {
                    morseCodeDt += ' ';
                }
                if (char.IsDigit(item))
                {
                    morseCodeDt += CharMapToMorse[item];
                    morseCodeDt += ' ';
                }
            }
            catch (Exception ex)
            {
                Debug.Log("数字编码成摩尔斯码异常:" + ex.Message);
            }
        }
        return morseCodeDt;
    }

    IEnumerator StartGame()
    {
        gameState = GameState.starting;
        SecondText.SetText("");
        SecondText.ForceMeshUpdate();
        //SecondText.gameObject.SetActive(false);
        for (int i = 3; i > 0; i--)
        {
            SecondText.SetText($@"<b><bounce>{i}</bounce></b> ");
            yield return new WaitForSeconds(1);
            SecondText.gameObject.SetActive(true);
        }

        SecondText.gameObject.SetActive(false);
        currentMorseIndex = 0;
        startgame = true;
    }

    void Update()
    {
        if (startgame == false && Input.GetKeyDown(KeyCodeInput.keyCode) && gameState == GameState.prepare)
        {
            //延迟3秒开始游戏
            StartCoroutine(StartGame());
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
            gameState = GameState.playing;
            char currentChar = morseCode[currentMorseIndex];
            GameObject prefab = null;
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
            }
            if (prefab != null)
            {
                GameObject morseCodeObject = Instantiate(prefab);
                morseCodeObject.transform.SetParent(spawnPoint, false);
                morseCodeObject.GetComponent<RectTransform>().anchoredPosition = spawnPoint.anchoredPosition;
                morseCodeObject.GetComponent<ItemPrefab>().id = prefabid++;
                morseCodeObjects.Add(morseCodeObject);
            }
            currentMorseIndex++;
        }
        else
        {
            if (morseCodeObjects.Count <= 0 && gameState == GameState.playing)
            {
                gameState = GameState.end;
                // 触发游戏结束事件
                GameOver?.Invoke();
                Debug.Log("GameOver");
                tabSwitcher.SwitchTab(nextTab);

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
