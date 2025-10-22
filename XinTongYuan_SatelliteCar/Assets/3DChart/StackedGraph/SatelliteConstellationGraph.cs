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

    //x轴 年份标签索引
    private readonly int[] yearsid = new int[]
    {
        0,1,2,3,4
    };

    // 卫星数量数据 [年份索引, 星座索引] 需要从Excel读取，这里只是示例数据
    private readonly int[,] satelliteData = new int[,]
    {
        {0,4,0,0,0}, //1978
        {4,3,2,6,0}, //1980
        {4,3100,28,26,0}, //1981
        {50,31,32,24,56}, //1982
        {50,31,32,200,98}  //1983
    };
    DataTable dataTable_country;
    void Start()
    {
        string countryColorStr = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "CountryColor.json"));
        countryCategoryColor = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(countryColorStr);
        foreach (var key in countryCategoryColor.Keys.ToList()) //统一添加#开头 给unity解析16进制颜色
        {
            var value = countryCategoryColor[key];
            if (!value.StartsWith("#"))
            {
                countryCategoryColor[key] = "#" + value;
            }
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

            Color fromcolor = Color.white;
            fromcolor.a = 0.3f;
            graphMaterial.SetColor("_ColorFrom", fromcolor);
            Color tocolor = ColorUtility.TryParseHtmlString(countryCategoryColor[cat], out Color graphColor2) ? graphColor2 : Color.white;
            tocolor.a = 0.5f;
            graphMaterial.SetColor("_ColorTo", tocolor);
            graphManager.Chart.DataSource.AddCategory(cat, lineMaterial, 2, new MaterialTiling(), graphMaterial, false, pointMaterial, 20);

            graphManager.Chart.DataSource.Set2DCategoryPrefabs(cat, LineHoverPrefab, PointHoverPrefab);
        }

        // 验证分类
        graphManager.VerifyCategories();

        // x轴标签映射
        graphManager.Chart.HorizontalValueToStringMap.Add(0, "1978");
        graphManager.Chart.HorizontalValueToStringMap.Add(1, "1979");
        graphManager.Chart.HorizontalValueToStringMap.Add(2, "1980");
        graphManager.Chart.HorizontalValueToStringMap.Add(3, "1981");
        graphManager.Chart.HorizontalValueToStringMap.Add(4, "1982");
    }

    // dataTable_country 已经从 Excel 读取
    // 表头：A1=年份, B1=中国, C1=美国, D1=欧洲, E1=俄罗斯, F1=英国
    // 下面是动态读取 category 和数据的代码

    private void LoadSatelliteData()
    {
        // 读取 categories（国家名），从第二列开始
        int categoryCount = dataTable_country.Columns.Count - 1;
        string[] categories = new string[categoryCount];
        for (int i = 0; i < categoryCount; i++)
        {
            categories[i] = dataTable_country.Columns[i + 1].ColumnName;
        }

        int dataCount = dataTable_country.Rows.Count;
        double[] xData = new double[dataCount];
        double[,] yData = new double[dataCount, categoryCount];

        for (int i = 1; i < dataCount; i++)
        {
            DataRow row = dataTable_country.Rows[i];
            // x轴用年份
            int year = Convert.ToInt32(row[0]);
            xData[i] = i; // 也可以用 year
            graphManager.Chart.HorizontalValueToStringMap[i] = year.ToString();

            for (int j = 1; j < categoryCount; j++)
            {
                yData[i, j] = Convert.ToDouble(row[j + 1]);
            }
        }

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