#define Graph_And_Chart_PRO
using ChartAndGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SatelliteConstellationGraph : MonoBehaviour
{
    public Text infoText;
    private StackedGraphManager2 graphManager;

    [SerializeField]
    private Material lineMat;
    [SerializeField]
    private Material graphMat;
    [SerializeField]
    private Material pointMat;

    [SerializeField]
    private ChartItemEffect LineHoverPrefab;
    [SerializeField]
    private ChartItemEffect PointHoverPrefab;

    private Dictionary<string, string> countryCategoryColor;

    // 卫星星座第一行分类
    private readonly string[] categories = new string[]
    {
        "中国",
        "美国",
        "欧洲",
        "俄罗斯",
        "英国",
    };

    DataTable dataTable_country;

    // 实时绘制相关
    private int currentYearIndex = 0; // 当前追加到哪一年
    private double[] xData; // 年份数据
    private double[,] yData; // 卫星数量数据
    private int categoryCount; // 分类数
    private int dataCount; // 年份数

    void Start()
    {
        string countryColorStr = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "CountryColor.json"));
        countryCategoryColor = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(countryColorStr);
        foreach (var key in countryCategoryColor.Keys.ToList()) //统一添加#开头 给unity解析16进制颜色
        {
            var value = countryCategoryColor[key];
            if (!value.StartsWith("#"))
                countryCategoryColor[key] = "#" + value;
        }
        var dataset = SatelliteDataReader.ReadExcel(Path.Combine(Application.streamingAssetsPath, "卫星年份数量.xlsx"));
        dataTable_country = dataset.Tables[0];
        graphManager = GetComponent<StackedGraphManager2>();
        if (graphManager == null || graphManager.Chart == null)
        {
            Debug.LogError("请确保物体上挂载了StackedGraphManager2组件并正确设置了Chart引用");
            return;
        }

        // 初始化图表分类
        InitializeCategories();

        // 设置悬停事件
        graphManager.Chart.PointHovered.AddListener(GraphHovered);
        graphManager.Chart.NonHovered.AddListener(NonHovered);

        // 加载初始数据（只显示第一年）
        PrepareSatelliteDataForRealtime();
        ShowFirstYear();
        StartCoroutine(AnimateYears(0.01f));
    }

    private void InitializeCategories()
    {
        // 清除现有分类
        foreach (var cat in graphManager.Chart.DataSource.CategoryNames.ToList())
            graphManager.Chart.DataSource.RemoveCategory(cat);

        // 添加新分类
        foreach (var cat in categories)
        {
            Material lineMaterial = new Material(lineMat);
            Material graphMaterial = new Material(graphMat);
            Material pointMaterial = new Material(pointMat);

            SetMat(cat, lineMaterial);
            SetMat(cat, graphMaterial);
            SetMat(cat, pointMaterial);
            graphManager.Chart.DataSource.AddCategory(cat, lineMaterial, 2, new MaterialTiling(), graphMaterial, false, pointMaterial, 20);

            graphManager.Chart.DataSource.Set2DCategoryPrefabs(cat, LineHoverPrefab, PointHoverPrefab);
        }

        // 验证分类
        graphManager.VerifyCategories();

        // 清空x轴标签映射，稍后动态设置
        graphManager.Chart.HorizontalValueToStringMap.Clear();
    }

    private void SetMat(string cat, Material mat)
    {
        Color fromcolor = Color.white;
        fromcolor.a = 0.3f;
        mat.SetColor("_ColorFrom", fromcolor);
        Color tocolor = ColorUtility.TryParseHtmlString(countryCategoryColor[cat], out Color graphColor2) ? graphColor2 : Color.white;
        tocolor.a = 0.5f;
        mat.SetColor("_ColorTo", tocolor);
    }

    // 预处理原始表格数据到 xData/yData
    private void PrepareSatelliteDataForRealtime()
    {
        categoryCount = dataTable_country.Columns.Count - 1;
        dataCount = dataTable_country.Rows.Count - 1; // 跳过表头
        xData = new double[dataCount];
        yData = new double[dataCount, categoryCount];

        for (int i = 1; i <= dataCount; i++)
        {
            DataRow row = dataTable_country.Rows[i];
            int year = Convert.ToInt32(row[0]);
            xData[i - 1] = i - 1; // 用索引做x
            graphManager.Chart.HorizontalValueToStringMap[i - 1] = year.ToString();

            for (int j = 1; j <= categoryCount; j++)
                yData[i - 1, j - 1] = Convert.ToDouble(row[j]);
        }
    }

    // 显示第一年
    private void ShowFirstYear()
    {
        double[] initX = new double[] { xData[0] };
        double[,] initY = new double[1, categoryCount];
        for (int c = 0; c < categoryCount; c++)
            initY[0, c] = yData[0, c];

        graphManager.InitialData(initX, initY);
        currentYearIndex = 1; // 下一年
    }

    // 实时追加一年的数据
    public void AddNextYearData()
    {
        if (currentYearIndex >= dataCount)
            return;
        graphManager.AddPointRealtime(xData[currentYearIndex], GetYArray(currentYearIndex));
        currentYearIndex++;
    }

    // 取某一年所有分类的 y 数据
    private double[] GetYArray(int idx)
    {
        double[] arr = new double[categoryCount];
        for (int c = 0; c < categoryCount; c++)
            arr[c] = yData[idx, c];
        return arr;
    }

    // 支持自动动画
    public IEnumerator AnimateYears(float interval = 1f)
    {
        while (currentYearIndex < dataCount)
        {
            AddNextYearData();
            yield return new WaitForSeconds(interval);
        }
    }

    public void ToggleCategory(string name)
    {
        if (graphManager != null)
            graphManager.ToggleCategoryEnabled(name);
    }

    public void GraphHovered(GraphChartBase.GraphEventArgs args)
    {
        if (infoText == null) return;
        var point = graphManager.GetPointValue(args.Category, args.Index);
        infoText.text = string.Format("{0} 年 - {1}: {2} 颗卫星",
            graphManager.Chart.HorizontalValueToStringMap.ContainsKey((int)point.x) ? graphManager.Chart.HorizontalValueToStringMap[(int)point.x] : ((int)point.x).ToString(),
            args.Category, point.y);
    }

    public void NonHovered()
    {
        if (infoText == null) return;
        infoText.text = "";
    }
}