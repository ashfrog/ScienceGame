using System;
using System.Data;
using System.IO;
using UnityEngine;
using ExcelDataReader;
using System.Collections.Generic;
using ChartAndGraph;
using System.Linq;
using UnityEngine.UI;

public class SatletExelDataReader : MonoBehaviour
{
    public PieChart pieChartGroup;
    public PieChart pieChartCountry;

    public Material material;

    public Text yearText1;
    public Text yearText2;

    public Text text1;
    public Text text2;

    Dictionary<string, string> pieCategoryColor;

    private void OnEnable()
    {
        initialize(Application.streamingAssetsPath + "/卫星轨道数据.xlsx", 1);
        //Hide();
    }
    void Start()
    {

    }

    DataTable table;

    private void initialize(string filePath, int sheetindex)
    {
        string pieCategoryColorStr = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "PieCategoryColor.json"));
        pieCategoryColor = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(pieCategoryColorStr);
        foreach (var key in pieCategoryColor.Keys.ToList()) //统一添加#开头 给unity解析16进制颜色
        {
            var value = pieCategoryColor[key];
            if (!value.StartsWith("#"))
            {
                pieCategoryColor[key] = "#" + value;
            }
        }
        DataSet result = ReadExcel(filePath);

        if (result != null)
        {
            table = result.Tables[sheetindex];
            // Example: Find year 2020 and show in pie chart
            ShowPieCharts(2024);
        }
    }

    public void ShowPieCharts(int year)
    {
        yearText1.text = year.ToString();
        yearText2.text = yearText1.text;
        if (year > 2020)
        {
            pieChartCountry.SpacingAngle = 14;
        }
        else
        {
            pieChartCountry.SpacingAngle = 6;
        }
        pieChartGroup.SpacingAngle = pieChartCountry.SpacingAngle;

        int yearColIdx = 0; // 第一列为年份

        // 用于国家分类的字典
        Dictionary<string, float> countryData = new Dictionary<string, float>();
        Dictionary<string, float> groupData = new Dictionary<string, float>();
        float totalGroup = 0f;
        float totalCountry = 0f;

        // 先定位年份行
        foreach (DataRow row in table.Rows)
        {
            if (row[yearColIdx] == null || row[yearColIdx].ToString() == "")
                continue;
            if (int.TryParse(row[yearColIdx].ToString(), out int rowYear) && rowYear == year)
            {
                // 1. 统计各组总值
                for (int i = 1; i < table.Columns.Count; i++)
                {
                    string groupName = table.Columns[i].ColumnName; // 如“北斗星座（中国）”
                    if (float.TryParse(row[i].ToString(), out float val))
                    {
                        totalGroup += val;
                        string category = GetCategory(groupName);
                        if (!groupData.ContainsKey(category))
                            groupData[category] = 0;
                        groupData[category] += val;

                        // 2. 提取国家名（括号内内容）
                        int left = groupName.IndexOf('（');
                        int right = groupName.IndexOf('）');
                        if (left != -1 && right != -1 && right > left)
                        {
                            string country = groupName.Substring(left + 1, right - left - 1);
                            if (!countryData.ContainsKey(country))
                                countryData[country] = 0;
                            countryData[country] += val;
                            totalCountry += val;
                        }
                    }
                }

                // ==== 先清空，再添加新分类 ====
                if (pieChartGroup != null && pieChartGroup.DataSource != null)
                {
                    // 添加分类时只保留 value > 0 的分类
                    pieChartGroup.DataSource.Clear();
                    foreach (var category in groupData.Keys)
                    {
                        if (groupData[category] > 0) // 只添加有值的
                        {
                            Material mat = new Material(material);
                            if (ColorUtility.TryParseHtmlString(pieCategoryColor[category], out UnityEngine.Color color))
                            {
                                mat.color = color;
                            }
                            pieChartGroup.DataSource.AddCategory(category, mat);
                        }
                    }
                }
                if (pieChartCountry != null && pieChartCountry.DataSource != null)
                {
                    pieChartCountry.DataSource.Clear();
                    foreach (var country in countryData.Keys)
                    {
                        if (countryData[country] > 0)
                        {
                            Material mat = new Material(material);
                            if (ColorUtility.TryParseHtmlString(pieCategoryColor[country], out UnityEngine.Color color))
                            {
                                mat.color = color;
                            }
                            pieChartCountry.DataSource.AddCategory(country, mat);
                        }
                    }
                }

                // ==== 设置数据 ====
                foreach (var kvp in groupData)
                {
                    if (kvp.Value > 0)
                    {
                        float percent = totalGroup > 0 ? kvp.Value / totalGroup : 0;
                        pieChartGroup.DataSource.SetValue(kvp.Key, percent * 100);
                    }
                }
                foreach (var kvp in countryData)
                {
                    if (kvp.Value > 0)
                    {
                        float percent = totalCountry > 0 ? kvp.Value / totalCountry : 0;
                        pieChartCountry.DataSource.SetValue(kvp.Key, percent * 100);
                    }
                }

                break;
            }
        }


        pieChartGroup.GetComponent<PieAnimation>().Animate();


        pieChartCountry.GetComponent<PieAnimation>().Animate();
    }

    public void Show()
    {
        pieChartGroup.gameObject.SetActive(true);
        pieChartCountry.gameObject.SetActive(true);
        yearText1.gameObject.SetActive(true);
        yearText2.gameObject.SetActive(true);
        text1.gameObject.SetActive(true);
        text2.gameObject.SetActive(true);
    }
    public void Hide()
    {
        pieChartGroup.gameObject.SetActive(false);
        pieChartCountry.gameObject.SetActive(false);
        yearText1.gameObject.SetActive(false);
        yearText2.gameObject.SetActive(false);
        if (text1 != null)
        {
            text1.gameObject.SetActive(false);
        }
        if (text2 != null)
        {
            text2.gameObject.SetActive(false);
        }
    }

    string GetCategory(string fullName)
    {
        int left = fullName.IndexOf('（');
        return left > 0 ? fullName.Substring(0, left) : fullName;
    }

    DataSet ReadExcel(string filePath)
    {
        try
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var conf = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true // 让第一行作为表头
                    }
                };
                var result = reader.AsDataSet(conf);
                return result;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading Excel file: " + e.Message);
            return null;
        }
    }
}