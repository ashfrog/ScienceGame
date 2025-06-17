using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.IO;
using System.Collections;
using TMPro;
using Febucci.UI;

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

    int prefabid;

    static Dictionary<string, string> codeDic_NoZh;

    [SerializeField]
    TextMeshProUGUI textMeshProUGUI_Nor;
    [SerializeField]
    TextMeshProUGUI textMeshProUGUI_Zhr;

    [SerializeField]
    TextMeshProUGUI textMeshProUGUI_No;
    [SerializeField]
    TextMeshProUGUI textMeshProUGUI_Zh;

    [SerializeField]
    Animator textAnimator;

    public string animationName = "发报完成Ani";

    float waitResultTime = 6f;

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

    enum GameState
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
        Settings.ini.Game.Speed = Settings.ini.Game.Speed;
        Settings.ini.Game.Interval = Settings.ini.Game.Interval;
        Settings.ini.Game.WaitResultTime = Settings.ini.Game.WaitResultTime;
        scrollSpeed = Settings.ini.Game.Speed;
        spawnInterval = Settings.ini.Game.Interval;
        waitResultTime = Settings.ini.Game.WaitResultTime;
    }

    private void OnEnable()
    {
        textMeshProUGUI_No.text = "";
        textMeshProUGUI_Zh.text = "";
        textMeshProUGUI_Nor.text = "电报码:";
        textMeshProUGUI_Zhr.text = "情报:";
        textAnimator.Play(animationName, -1, 0f);
        textAnimator.speed = 0;
        startgame = false;
        gameState = GameState.prepare;
        String morsecodesZHStrs = System.IO.File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "报文.json"));
        List<String> morsecodesZHs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(morsecodesZHStrs);
        string morseCodeZH = morsecodesZHs[UnityEngine.Random.Range(0, morsecodesZHs.Count)];

        textMeshProUGUI_Zhr.text += morseCodeZH;

        PlayerPrefs.SetString("PrintKey", morseCodeZH);

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
    private string ConvertZH2MorsCode(string morseCodeStr, String codesDicStr)
    {
        codeDic_NoZh = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(codesDicStr);

        //将codesDirStr key value 翻转
        Dictionary<string, string> codeDicReverse = new Dictionary<string, string>();
        foreach (var item in codeDic_NoZh)
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
        textMeshProUGUI_Nor.text += CodeNo;
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
                textAnimator.Play(animationName, -1, 0f);
                textAnimator.speed = 1;
                Invoke(nameof(SwitchTab), waitResultTime);
            }
        }
    }

    void SwitchTab()
    {
        if (textMeshProUGUI_Zhr.text.EndsWith(textMeshProUGUI_Zh.text))
        {
            tabSwitcher.SwitchTab(nextTab);
        }
        else
        {
            tabSwitcher.SwitchTab(nextTab + 1);
        }
    }

    int morsCount;
    int morsNoCount;
    string morseCodes;
    string morsNos;
    void ScrollMorseCode()
    {
        for (int i = morseCodeObjects.Count - 1; i >= 0; i--)
        {
            GameObject morseCodeObject = morseCodeObjects[i];
            morseCodeObject.GetComponent<RectTransform>().anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;
            if (morseCodeObject.GetComponent<RectTransform>().anchoredPosition.x < endPoint.GetComponent<RectTransform>().anchoredPosition.x)
            {
                ItemPrefab itemPrefab = morseCodeObject.GetComponent<ItemPrefab>();
                ShowResultZH(itemPrefab);

                Destroy(morseCodeObject);
                morseCodeObjects.RemoveAt(i);
            }
        }
    }
    /// <summary>
    /// 显示发送结果
    /// </summary>
    /// <param name="itemPrefab"></param>
    private void ShowResultZH(ItemPrefab itemPrefab)
    {
        morsCount++;
        morseCodes += itemPrefab.pressDotChar;
        if (morsCount >= 5)
        {
            morsCount = 0;
            Debug.Log(morseCodes);
            string morsNo = morseCode2MorsNo(morseCodes);
            morsNos += morsNo;
            textMeshProUGUI_No.text += morsNo;
            textMeshProUGUI_No.text += ' ';
            Debug.Log("morsNo:" + morsNo);
            morsNoCount++;
            if (morsNoCount >= 4)
            {
                morsNoCount = 0;
                Debug.Log("morsNos:" + morsNos);
                string morseNoZH = morsNo2ZH(morsNos);
                textMeshProUGUI_Zh.text += morseNoZH;
                Debug.Log("morseNoZH:" + morseNoZH);
                morsNos = "";
            }
            morseCodes = "";
        }
    }

    string morseCode2MorsNo(string morseCode)
    {
        if (MorseToCharMap.ContainsKey(morseCode))
        {
            return MorseToCharMap[morseCode].ToString();
        }
        return "0";
    }
    string morsNo2ZH(String morseNo)
    {
        if (codeDic_NoZh.ContainsKey(morseNo))
        {
            return codeDic_NoZh[morseNo];
        }

        return "?";
    }
}
