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

    DataTable dataTable_country;

    // 加载全部数据相关
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
        var dataset = SatelliteDataReader.ReadExcel(Path.Combine(Application.streamingAssetsPath, "卫星年份数量.xlsx"), true);
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

        // 一次性加载所有数据
        PrepareSatelliteDataForRealtime();
        ShowAllData();
    }
    private void OnEnable()
    {
        graphManager = GetComponent<StackedGraphManager2>();
        if (graphManager == null || graphManager.Chart == null)
        {
            Debug.LogError("请确保物体上挂载了StackedGraphManager2组件并正确设置了Chart引用");
            return;
        }
        ResetPos();
    }
    public void ResetPos()
    {
        // 重置x轴位置
        graphManager.Chart.HorizontalScrolling = 0;
    }

    private void InitializeCategories()
    {
        // 清除现有分类
        foreach (var cat in graphManager.Chart.DataSource.CategoryNames.ToList())
            graphManager.Chart.DataSource.RemoveCategory(cat);

        List<string> categories = new List<string>();
        for (int col = 1; col < dataTable_country.Columns.Count; col++)
        {
            Debug.Log($"列{col}: {dataTable_country.Columns[col].ColumnName}");
            categories.Add(dataTable_country.Columns[col].ColumnName);
        }

        // 添加新分类
        foreach (var cat in categories)
        {
            Material lineMaterial = new Material(lineMat);
            Material graphMaterial = new Material(graphMat);
            Material pointMaterial = new Material(pointMat);

            SetMat(cat, lineMaterial);
            SetMat(cat, graphMaterial);
            SetMat(cat, pointMaterial);
            graphManager.Chart.DataSource.AddCategory(cat, lineMaterial, 5, new MaterialTiling(), graphMaterial, false, pointMaterial, 40);
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
        fromcolor.a = 0.2f;
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

        for (int i = 0; i < dataCount; i++) //行
        {
            DataRow row = dataTable_country.Rows[i];
            int year = Convert.ToInt32(row[0]);
            xData[i] = i; // 用索引做x
            graphManager.Chart.HorizontalValueToStringMap[i] = year.ToString();

            for (int j = 1; j <= categoryCount; j++) //列
                yData[i, j - 1] = Convert.ToDouble(row[j]);
        }


    }

    // 一次性显示全部年份
    private void ShowAllData()
    {
        graphManager.InitialData(xData, yData);
    }

    // 支持自动动画的相关方法已不再使用

    public void ToggleCategory(string name)
    {
        if (graphManager != null)
            graphManager.ToggleCategoryEnabled(name);
    }

    public void OnMousePan()
    {
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