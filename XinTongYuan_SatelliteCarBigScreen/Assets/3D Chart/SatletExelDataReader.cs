using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExcelDataReader;
using ChartAndGraph;

public class SatletExelDataReader : MonoBehaviour
{
    [Header("PieCharts")] public PieChart pieChartGroup, pieChartCountry;
    [Header("Shared Material")] public Material material;
    [Header("UI")] public Text yearText1, yearText2, text1, text2;

    [Header("Excel settings")]
    public string groupExcelFile = "历年卫星星座在轨数量.xlsx"; public int groupSheetIndex = 0; public bool groupStripChineseBrackets = true;
    public string countryExcelFile = "历年各国卫星在轨数量.xlsx"; public int countrySheetIndex = 0; public bool countryStripChineseBrackets = false;

    [Header("Colors")] public string pieCategoryColorJson = "PieCategoryColor.json";

    Dictionary<string, string> colorMap = new(); DataTable groupTable, countryTable;

    void OnEnable() { Init(); Hide(); }
    void Init()
    {
        // 颜色映射
        try
        {
            var p = Path.Combine(Application.streamingAssetsPath, pieCategoryColorJson);
            if (File.Exists(p))
            {
                var json = File.ReadAllText(p);
                colorMap = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new();
                foreach (var k in colorMap.Keys.ToList())
                    if (!string.IsNullOrEmpty(colorMap[k]) && !colorMap[k].StartsWith("#"))
                        colorMap[k] = "#" + colorMap[k];
            }
        }
        catch { }
        // 读 Excel
        groupTable = ReadExcel(Path.Combine(Application.streamingAssetsPath, groupExcelFile), groupSheetIndex);
        countryTable = ReadExcel(Path.Combine(Application.streamingAssetsPath, countryExcelFile), countrySheetIndex);
        ShowYear(2024);
    }

    public void ShowYear(int year)
    {
        if (yearText1) yearText1.text = year.ToString();
        if (yearText2) yearText2.text = year.ToString();
        float spacing = year > 2020 ? 14f : 6f;
        if (pieChartGroup) pieChartGroup.SpacingAngle = spacing;
        if (pieChartCountry) pieChartCountry.SpacingAngle = spacing;

        // 星座（百分比显示）
        RenderPie(pieChartGroup, groupTable, year, groupStripChineseBrackets, usePercentage: false);
        // 国家（实际数量显示）
        RenderPie(pieChartCountry, countryTable, year, countryStripChineseBrackets, usePercentage: false);
    }

    void RenderPie(PieChart chart, DataTable table, int year, bool strip, bool usePercentage)
    {
        if (!chart || table == null || table.Columns.Count == 0 || table.Rows.Count == 0)
        {
            SafeClear(chart);
            return;
        }
        chart.DataSource.StartBatch();
        DataRow row = null;
        foreach (DataRow r in table.Rows)
        {
            var s = r[0]?.ToString();
            if (int.TryParse(s, out int y) && y == year) { row = r; break; }
        }
        if (row == null)
        {
            SafeClear(chart);
            chart.DataSource.EndBatch();
            return;
        }

        var totals = new Dictionary<string, float>(); float sum = 0;
        for (int i = 1; i < table.Columns.Count; i++)
        {
            string name = Normalize(table.Columns[i].ColumnName?.Trim(), strip);
            if (string.IsNullOrEmpty(name)) continue;
            if (float.TryParse(row[i]?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float v) && v > 0)
            {
                sum += v;
                if (!totals.ContainsKey(name)) totals[name] = 0;
                totals[name] += v;
            }
        }
        if (sum <= 0 || totals.Count == 0)
        {
            SafeClear(chart);
            chart.DataSource.EndBatch();
            return;
        }

        chart.DataSource.Clear();
        foreach (var kv in totals)
        {
            var mat = new Material(material ? material : new Material(Shader.Find("Standard")));
            mat.color = TryColor(kv.Key, out var c) ? c : StableColor(kv.Key);
            if (!chart.DataSource.HasCategory(kv.Key))
                chart.DataSource.AddCategory(kv.Key, mat);
        }

        // 根据模式设置值：百分比或实际数量
        foreach (var kv in totals)
        {
            float valueToSet = usePercentage ? (kv.Value / sum * 100f) : kv.Value;
            chart.DataSource.SetValue(kv.Key, valueToSet);
        }

        var anim = chart.GetComponent<PieAnimation>(); if (anim) anim.Animate();
        chart.DataSource.EndBatch();
    }

    public void Show()
    {
        if (pieChartGroup) pieChartGroup.gameObject.SetActive(true);
        if (yearText2) yearText2.gameObject.SetActive(true);
        if (text2) text2.gameObject.SetActive(true);

        if (pieChartCountry) pieChartCountry.gameObject.SetActive(true);
        if (text1) text1.gameObject.SetActive(true);
        if (yearText1) yearText1.gameObject.SetActive(true);
    }
    public void Hide()
    {
        if (pieChartGroup) pieChartGroup.gameObject.SetActive(false);
        if (pieChartCountry) pieChartCountry.gameObject.SetActive(false);
        if (yearText1) yearText1.gameObject.SetActive(false); if (yearText2) yearText2.gameObject.SetActive(false);
        if (text1) text1.gameObject.SetActive(false); if (text2) text2.gameObject.SetActive(false);
    }

    string Normalize(string s, bool strip) { if (string.IsNullOrEmpty(s) || !strip) return s; int i = s.IndexOf('（'); return i > 0 ? s.Substring(0, i).Trim() : s.Trim(); }
    bool TryColor(string key, out Color c) { c = default; return colorMap != null && colorMap.TryGetValue(key, out var hex) && ColorUtility.TryParseHtmlString(hex, out c); }
    Color StableColor(string key) { unchecked { int h = 17; foreach (char ch in key) h = h * 31 + ch; float hue = (h & 0xFFFF) / (float)0xFFFF; return Color.HSVToRGB(hue, 0.6f, 0.9f); } }
    void SafeClear(PieChart chart) { try { chart?.DataSource?.Clear(); } catch { } }

    DataTable ReadExcel(string path, int sheet)
    {
        try
        {
            if (!File.Exists(path)) return null;
            using var s = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var r = ExcelReaderFactory.CreateReader(s);
            var ds = r.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } });
            if (ds == null || ds.Tables.Count == 0) return null;
            if (sheet < 0 || sheet >= ds.Tables.Count) return null;
            return ds.Tables[sheet];
        }
        catch { return null; }
    }
}