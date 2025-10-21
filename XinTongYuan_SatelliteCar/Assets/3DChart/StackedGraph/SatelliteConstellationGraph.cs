#define Graph_And_Chart_PRO
using ChartAndGraph;
using System.Collections;
using System.Collections.Generic;
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

    private Dictionary<string, string> countryColor;

    // 卫星星座数据
    private readonly string[] categories = new string[]
    {
        "c6",
        "c2",
        "c3",
        "c4",
        "c5",
    };

    // 年份数据
    //private readonly int[] years = new int[]
    //{
    //    1,2,3,4,5,1978,1979,1980,1981,1982,1983,1984,1985,1986,1987,1988,1989,1990,1991,1992,1993,1994,1995,1996,1997,
    //    1998,1999,2000,2001,2002,2003,2004,2005,2006,2007,2008,2009,2010,2011,2012,2013,2014,2015,2016,2017,
    //    2018,2019,2020,2021,2022,2023,2024,2025
    //};
    private readonly int[] years = new int[]
    {
        1,2,3,4,5
    };

    // 卫星数量数据 [年份索引, 星座索引]
    private readonly int[,] satelliteData = new int[,]
    {
        {4,1,8,7,0}, //2021
        {4,3,2,6,0}, //2022
        {4,31,28,26,0}, //2023
        {50,31,32,24,56}, //2024
        {50,31,32,200,98}  //2025
    };

    void Start()
    {
        string countryColorStr = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "CountryColor.json"));
        countryColor = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(countryColorStr);
        foreach (var key in countryColor.Keys.ToList()) //统一添加#开头 给unity解析16进制颜色
        {
            var value = countryColor[key];
            if (!value.StartsWith("#"))
            {
                countryColor[key] = "#" + value;
            }
        }

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

        // 加载初始数据
        LoadSatelliteData();
    }

    // 初始化图表分类
    private void InitializeCategories()
    {
        // 清除现有分类
        foreach (var cat in graphManager.Chart.DataSource.CategoryNames.ToList())
        {
            graphManager.Chart.DataSource.RemoveCategory(cat);
        }



        // 添加新分类
        foreach (var cat in categories)
        {
            Material lineMaterial = new Material(lineMat);
            Material graphMaterial = new Material(graphMat);
            Material pointMaterial = new Material(pointMat);
            graphManager.Chart.DataSource.AddCategory(cat, lineMaterial, 2, new MaterialTiling(), graphMaterial, false, pointMaterial, 10);
            graphManager.Chart.DataSource.Set2DCategoryPrefabs(cat, LineHoverPrefab, PointHoverPrefab);
        }

        // 验证分类
        graphManager.VerifyCategories();

        Debug.Log("已添加分类数量: " + graphManager.Chart.DataSource.CategoryNames.Count());
    }

    // 加载卫星数据到图表
    private void LoadSatelliteData()
    {
        int dataCount = years.Length;
        double[] xData = years.Select(y => (double)y).ToArray();
        double[,] yData = new double[dataCount, categories.Length];

        // 填充数据
        for (int i = 0; i < dataCount; i++)
        {
            for (int j = 0; j < categories.Length; j++)
            {
                yData[i, j] = satelliteData[i, j];
            }
        }

        // 设置图表数据
        graphManager.InitialData(xData, yData);
    }

    public void ToggleCategory(string name)
    {
        if (graphManager != null)
        {
            graphManager.ToggleCategoryEnabled(name);
        }
    }

    public void GraphHovered(GraphChartBase.GraphEventArgs args)
    {
        if (infoText == null) return;

        var point = graphManager.GetPointValue(args.Category, args.Index);
        infoText.text = string.Format("{0} 年 - {1}: {2} 颗卫星",
            (int)point.x, args.Category, point.y);
    }

    public void NonHovered()
    {
        if (infoText == null) return;
        infoText.text = "";
    }
}