using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.IO;
using System.Collections;
using TMPro;
using Febucci.UI;

/// <summary>
/// 摩尔斯码生成和滚动控制器
/// </summary>
public class MorseCodeGenerator : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] TabSwitcher tabSwitcher;
    [SerializeField] TextMeshProUGUI textMeshProUGUI_Nor;
    [SerializeField] TextMeshProUGUI textMeshProUGUI_Zhr;
    [SerializeField] TextMeshProUGUI textMeshProUGUI_No;
    [SerializeField] TextMeshProUGUI textMeshProUGUI_Zh;
    [SerializeField] Animator textAnimator;
    public TextMeshProUGUI SecondText;

    [Header("预制体")]
    public GameObject dotPrefab;
    public GameObject dashPrefab;
    public GameObject emptyPrefab;

    [Header("生成设置")]
    public RectTransform spawnPoint;
    public RectTransform endPoint;
    public float scrollSpeed = 100f;
    public float fixedSpacing = 80f; // 固定间距
    public int nextTab;
    public string animationName = "发报完成Ani";
    public float waitResultTime = 6f;

    // 私有变量
    private List<GameObject> morseCodeObjects = new List<GameObject>();
    private int currentMorseIndex = 0;
    private int prefabId = 0;
    private string morseCode = "";
    private bool startGame = false;
    private GameState gameState = GameState.prepare;

    // 结果统计
    private int morseCount = 0;
    private int morseNoCount = 0;
    private string morseCodes = "";
    private string morseNos = "";

    // 静态字典
    private static Dictionary<string, string> codeDic_NoZh;
    private static readonly Dictionary<string, char> MorseToCharMap = new Dictionary<string, char>
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

    private static readonly Dictionary<char, string> CharMapToMorse = new Dictionary<char, string>
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

    public Action GameOver;

    private enum GameState
    {
        prepare,
        starting,
        playing,
        end
    }

    void Start()
    {
        InitializeComponents();
        LoadSettings();
    }

    private void InitializeComponents()
    {
        if (tabSwitcher == null)
            tabSwitcher = GetComponentInParent<TabSwitcher>();
    }

    private void LoadSettings()
    {
        scrollSpeed = Settings.ini.Game.Speed;
        waitResultTime = Settings.ini.Game.WaitResultTime;
    }

    private void OnEnable()
    {
        InitializeGame();
        LoadMorseCode();
    }

    private void InitializeGame()
    {
        // 重置UI
        textMeshProUGUI_No.text = "";
        textMeshProUGUI_Zh.text = "";
        textMeshProUGUI_Nor.text = "电报码:";
        textMeshProUGUI_Zhr.text = "情报:";

        // 重置动画
        textAnimator.Play(animationName, -1, 0f);
        textAnimator.speed = 0;

        // 重置游戏状态
        startGame = false;
        gameState = GameState.prepare;
        currentMorseIndex = 0;

        // 清理现有物体
        foreach (GameObject obj in morseCodeObjects)
        {
            if (obj != null) Destroy(obj);
        }
        morseCodeObjects.Clear();

        // 重置统计
        morseCount = 0;
        morseNoCount = 0;
        morseCodes = "";
        morseNos = "";

        // 显示开始提示
        SecondText.SetText(@"<size=60><bounce>按键开始</bounce></size>");
        SecondText.gameObject.SetActive(true);
    }

    private void LoadMorseCode()
    {
        try
        {
            // 加载中文报文
            string morsecodesZHStrs = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "报文.json"));
            List<string> morsecodesZHs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(morsecodesZHStrs);
            string morseCodeZH = morsecodesZHs[UnityEngine.Random.Range(0, morsecodesZHs.Count)];

            textMeshProUGUI_Zhr.text += morseCodeZH;
            PlayerPrefs.SetString("PrintKey", morseCodeZH);

            // 转换为摩尔斯码
            string codesDicStr = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "中文电码表.json"));
            morseCode = ConvertZH2MorsCode(morseCodeZH, codesDicStr);
        }
        catch (Exception ex)
        {
            Debug.LogError($"加载摩尔斯码失败: {ex.Message}");
        }
    }

    private string ConvertZH2MorsCode(string morseCodeStr, string codesDicStr)
    {
        codeDic_NoZh = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(codesDicStr);

        // 翻转字典
        var codeDicReverse = new Dictionary<string, string>();
        foreach (var item in codeDic_NoZh)
        {
            codeDicReverse[item.Value] = item.Key;
        }

        // 中文转数字码
        string codeNo = "";
        foreach (char ch in morseCodeStr)
        {
            if (codeDicReverse.TryGetValue(ch.ToString(), out string code))
            {
                codeNo += code + "  ";
            }
        }
        textMeshProUGUI_Nor.text += codeNo;

        // 数字码转摩尔斯码
        string morseCodeResult = "";
        foreach (char ch in codeNo)
        {
            if (char.IsWhiteSpace(ch))
            {
                morseCodeResult += ' ';
            }
            else if (char.IsDigit(ch) && CharMapToMorse.TryGetValue(ch, out string morse))
            {
                morseCodeResult += morse + ' ';
            }
        }

        return morseCodeResult;
    }

    void Update()
    {
        HandleInput();

        if (startGame)
        {
            UpdateMorseCode();
        }
    }

    private void HandleInput()
    {
        if (!startGame && Input.GetKeyDown(KeyCodeInput.keyCode) && gameState == GameState.prepare)
        {
            StartCoroutine(StartGameCoroutine());
        }
    }

    private IEnumerator StartGameCoroutine()
    {
        gameState = GameState.starting;
        SecondText.SetText("");

        // 倒计时
        for (int i = 3; i > 0; i--)
        {
            SecondText.SetText($@"<b><bounce>{i}</bounce></b>");
            yield return new WaitForSeconds(1);
        }

        SecondText.gameObject.SetActive(false);
        startGame = true;
        gameState = GameState.playing;
    }

    private void UpdateMorseCode()
    {
        SpawnMorseCodeIfNeeded();
        ScrollMorseCode();
    }

    private void SpawnMorseCodeIfNeeded()
    {
        // 检查是否需要生成新的摩尔斯码对象
        bool canSpawn = currentMorseIndex < morseCode.Length;

        if (canSpawn)
        {
            // 如果没有物体，直接生成第一个
            if (morseCodeObjects.Count == 0)
            {
                SpawnMorseCode();
            }
            // 检查最后一个物体是否已经移动了足够的距离
            else if (morseCodeObjects.Count > 0)
            {
                GameObject lastObject = morseCodeObjects[morseCodeObjects.Count - 1];
                float lastObjectX = lastObject.GetComponent<RectTransform>().anchoredPosition.x;

                // 当最后一个物体向左移动了足够距离时，生成新物体
                if (spawnPoint.anchoredPosition.x - lastObjectX >= fixedSpacing)
                {
                    SpawnMorseCode();
                }
            }
        }
        else if (morseCodeObjects.Count == 0 && gameState == GameState.playing)
        {
            EndGame();
        }
    }

    private void SpawnMorseCode()
    {
        char currentChar = morseCode[currentMorseIndex];
        GameObject prefab = GetPrefabForChar(currentChar);

        if (prefab != null)
        {
            GameObject morseCodeObject = Instantiate(prefab, spawnPoint);
            morseCodeObject.GetComponent<RectTransform>().anchoredPosition = spawnPoint.anchoredPosition;
            morseCodeObject.GetComponent<ItemPrefab>().id = prefabId++;
            morseCodeObjects.Add(morseCodeObject);
        }

        currentMorseIndex++;
    }

    private GameObject GetPrefabForChar(char ch)
    {
        return ch switch
        {
            '.' => dotPrefab,
            '-' => dashPrefab,
            _ => null
        };
    }

    private void ScrollMorseCode()
    {
        for (int i = morseCodeObjects.Count - 1; i >= 0; i--)
        {
            GameObject morseCodeObject = morseCodeObjects[i];
            RectTransform rectTransform = morseCodeObject.GetComponent<RectTransform>();

            // 移动对象
            rectTransform.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;

            // 检查是否到达终点
            if (rectTransform.anchoredPosition.x < endPoint.anchoredPosition.x)
            {
                ProcessMorseResult(morseCodeObject.GetComponent<ItemPrefab>());
                Destroy(morseCodeObject);
                morseCodeObjects.RemoveAt(i);
            }
        }
    }

    private void ProcessMorseResult(ItemPrefab itemPrefab)
    {
        morseCount++;
        morseCodes += itemPrefab.pressDotChar;

        if (morseCount >= 5)
        {
            ProcessMorseGroup();
        }
    }

    private void ProcessMorseGroup()
    {
        morseCount = 0;
        string morseNo = ConvertMorseToNumber(morseCodes);
        morseNos += morseNo;

        textMeshProUGUI_No.text += morseNo + " ";
        morseNoCount++;

        if (morseNoCount >= 4)
        {
            ProcessNumberGroup();
        }

        morseCodes = "";
    }

    private void ProcessNumberGroup()
    {
        morseNoCount = 0;
        string morseNoZH = ConvertNumberToZH(morseNos);
        textMeshProUGUI_Zh.text += morseNoZH;
        morseNos = "";
    }

    private string ConvertMorseToNumber(string morse)
    {
        return MorseToCharMap.TryGetValue(morse, out char result) ? result.ToString() : "0";
    }

    private string ConvertNumberToZH(string morseNo)
    {
        return codeDic_NoZh.TryGetValue(morseNo, out string result) ? result : "?";
    }

    private void EndGame()
    {
        gameState = GameState.end;
        GameOver?.Invoke();

        textAnimator.Play(animationName, -1, 0f);
        textAnimator.speed = 1;

        Invoke(nameof(SwitchTab), waitResultTime);
    }

    private void SwitchTab()
    {
        int targetTab = textMeshProUGUI_Zhr.text.EndsWith(textMeshProUGUI_Zh.text) ? nextTab : nextTab + 1;
        tabSwitcher.SwitchTab(targetTab);
    }
}