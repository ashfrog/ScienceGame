using System;
using System.Data;
using System.IO;
using UnityEngine;
using ExcelDataReader;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using ChartAndGraph;

public class SatletExelDataReader : MonoBehaviour
{
    [Header("Charts")]
    public PieChart pieChartGroup;
    public PieChart pieChartCountry;

    [Header("Shared Material (used per-slice)")]
    public Material material;

    [Header("UI")]
    public Text yearText1;
    public Text yearText2;
    public Text text1;
    public Text text2;

    [Header("Excel settings")]
    // 分别给两个图配置各自的 xlsx 文件与工作表索引（0 基）
    const string groupExcelFile = "历年卫星星座在轨数量.xlsx";
    private int groupSheetIndex = 0; // 第二个表（与原代码保持一致）
    public bool groupStripChineseBrackets = true; // 是否对列名进行 “（…）” 前缀截取

    const string countryExcelFile = "历年各国卫星在轨数量.xlsx";
    private int countrySheetIndex = 0; // 第一个表（与原代码保持一致）
    public bool countryStripChineseBrackets = false;

    [Header("Colors")]
    // 颜色配置文件：键为分类名（与列名或规范化后的列名一致），值为 #RRGGBB
    public string pieCategoryColorJson = "PieCategoryColor.json";

    // 内部状态
    private Dictionary<string, string> pieCategoryColor;
    private DataTable groupTable;   // pieChartGroup 对应的数据表
    private DataTable countryTable; // pieChartCountry 对应的数据表

    private void OnEnable()
    {
        Initialize();
        //Hide();
    }

    void Start()
    {
    }

    private void Initialize()
    {
        // 读取颜色映射
        try
        {
            string colorPath = Path.Combine(Application.streamingAssetsPath, pieCategoryColorJson);
            string pieCategoryColorStr = File.ReadAllText(colorPath);
            pieCategoryColor = Newtonsoft.Json.JsonConvert
                .DeserializeObject<Dictionary<string, string>>(pieCategoryColorStr) ?? new Dictionary<string, string>();
            // 统一加上 '#' 前缀
            foreach (var key in pieCategoryColor.Keys.ToList())
            {
                var value = pieCategoryColor[key] ?? "";
                if (!value.StartsWith("#"))
                    pieCategoryColor[key] = "#" + value;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Read {pieCategoryColorJson} failed: {e.Message}");
            pieCategoryColor = new Dictionary<string, string>();
        }

        // 分别读取两个图各自的 Excel
        groupTable = ReadExcelTable(Path.Combine(Application.streamingAssetsPath, groupExcelFile), groupSheetIndex);
        countryTable = ReadExcelTable(Path.Combine(Application.streamingAssetsPath, countryExcelFile), countrySheetIndex);

        // 示例：默认显示 2024 年
        ShowYear(2024);
    }

    public void ShowYear(int year)
    {
        if (yearText1) yearText1.text = year.ToString();
        if (yearText2) yearText2.text = year.ToString();

        // 统一间距逻辑
        float spacing = ComputeSpacingAngle(year);
        if (pieChartGroup != null) pieChartGroup.SpacingAngle = spacing;
        if (pieChartCountry != null) pieChartCountry.SpacingAngle = spacing;

        // 分别渲染两个饼图（各用自己的表）
        ShowPieChart(
            chart: pieChartGroup,
            table: groupTable,
            year: year,
            stripChineseBrackets: groupStripChineseBrackets
        );

        ShowPieChart(
            chart: pieChartCountry,
            table: countryTable,
            year: year,
            stripChineseBrackets: countryStripChineseBrackets
        );
    }

    private void ShowPieChart(PieChart chart, DataTable table, int year, bool stripChineseBrackets)
    {
        if (chart == null)
            return;

        // 如果没有表或数据，直接清空并返回
        if (table == null || table.Columns.Count == 0 || table.Rows.Count == 0)
        {
            SafeClearChart(chart);
            return;
        }

        chart.DataSource.StartBatch();

        // 年份列固定为第 0 列
        int yearColIdx = 0;

        // 定位目标年份的那一行
        DataRow targetRow = null;
        foreach (DataRow row in table.Rows)
        {
            if (row[yearColIdx] == null || string.IsNullOrWhiteSpace(row[yearColIdx].ToString()))
                continue;
            if (int.TryParse(row[yearColIdx].ToString(), out int rowYear) && rowYear == year)
            {
                targetRow = row;
                break;
            }
        }

        // 没有找到该年份
        if (targetRow == null)
        {
            SafeClearChart(chart);
            chart.DataSource.EndBatch();
            return;
        }

        // 统计分类数据（第 1 列开始为各分类）
        Dictionary<string, float> totals = new Dictionary<string, float>();
        float totalSum = 0f;

        for (int i = 1; i < table.Columns.Count; i++)
        {
            string rawName = table.Columns[i].ColumnName?.Trim() ?? "";
            if (string.IsNullOrEmpty(rawName))
                continue;

            string category = NormalizeCategory(rawName, stripChineseBrackets);

            // 尝试读取数值
            if (targetRow[i] != null && float.TryParse(targetRow[i].ToString(), out float val))
            {
                if (val <= 0f) continue;
                totalSum += val;
                if (!totals.ContainsKey(category)) totals[category] = 0f;
                totals[category] += val;
            }
        }

        // 没有有效数据
        if (totalSum <= 0f || totals.Count == 0)
        {
            SafeClearChart(chart);
            chart.DataSource.EndBatch();
            return;
        }

        // 先清空分类，再按 totals 添加分类与颜色
        chart.DataSource.Clear();

        foreach (var kv in totals)
        {
            string category = kv.Key;
            Material mat = new Material(material != null ? material : new Material(Shader.Find("Standard")));
            // 按颜色表着色，不存在则随机给色（或使用默认材质色）
            if (TryGetColor(category, out Color c))
                mat.color = c;
            else
                mat.color = GetStableRandomColor(category);

            if (!chart.DataSource.HasCategory(category))
                chart.DataSource.AddCategory(category, mat);
        }

        // 设置百分比值
        foreach (var kv in totals)
        {
            float percent = kv.Value / totalSum;
            chart.DataSource.SetValue(kv.Key, percent * 100f);
        }

        // 动画
        var anim = chart.GetComponent<PieAnimation>();
        if (anim != null)
            anim.Animate();

        chart.DataSource.EndBatch();
    }

    public void Show()
    {
        if (pieChartGroup) pieChartGroup.gameObject.SetActive(true);
        if (pieChartCountry) pieChartCountry.gameObject.SetActive(true);
        if (yearText1) yearText1.gameObject.SetActive(true);
        if (yearText2) yearText2.gameObject.SetActive(true);
        if (text1) text1.gameObject.SetActive(true);
        if (text2) text2.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (pieChartGroup) pieChartGroup.gameObject.SetActive(false);
        if (pieChartCountry) pieChartCountry.gameObject.SetActive(false);
        if (yearText1) yearText1.gameObject.SetActive(false);
        if (yearText2) yearText2.gameObject.SetActive(false);
        if (text1) text1.gameObject.SetActive(false);
        if (text2) text2.gameObject.SetActive(false);
    }

    // 工具方法

    private float ComputeSpacingAngle(int year)
    {
        // 与原逻辑一致：2020 年之后间距更大
        return year > 2020 ? 14f : 6f;
    }

    private void SafeClearChart(PieChart chart)
    {
        try
        {
            if (chart != null && chart.DataSource != null)
                chart.DataSource.Clear();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Clear chart failed: {e.Message}");
        }
    }

    private string NormalizeCategory(string fullName, bool stripChineseBrackets)
    {
        if (!stripChineseBrackets || string.IsNullOrEmpty(fullName))
            return fullName;

        int left = fullName.IndexOf('（');
        return left > 0 ? fullName.Substring(0, left) : fullName;
    }

    private bool TryGetColor(string key, out Color color)
    {
        color = default;
        if (pieCategoryColor != null && pieCategoryColor.TryGetValue(key, out string hex))
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color c))
            {
                color = c;
                return true;
            }
        }
        return false;
    }

    // 根据分类名生成稳定的随机色，避免每帧抖动
    private Color GetStableRandomColor(string key)
    {
        unchecked
        {
            int hash = 17;
            foreach (char c in key)
                hash = hash * 31 + c;

            // 将 hash 映射到 0..1
            float hue = (hash & 0xFFFF) / (float)0xFFFF;
            Color c1 = Color.HSVToRGB(hue, 0.6f, 0.9f);
            return c1;
        }
    }

    private DataTable ReadExcelTable(string filePath, int sheetIndex)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Excel not found: {filePath}");
                return null;
            }

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var conf = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                };

                var ds = reader.AsDataSet(conf);
                if (ds == null)
                {
                    Debug.LogWarning($"Excel empty: {filePath}");
                    return null;
                }

                if (sheetIndex < 0 || sheetIndex >= ds.Tables.Count)
                {
                    Debug.LogWarning($"Sheet index out of range: {sheetIndex} in {filePath}. Available sheets: {ds.Tables.Count}");
                    return null;
                }

                return ds.Tables[sheetIndex];
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading Excel file: " + e.Message);
            return null;
        }
    }
}