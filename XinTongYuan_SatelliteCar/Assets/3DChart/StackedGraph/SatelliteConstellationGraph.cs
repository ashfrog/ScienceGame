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
    public Text pageInfoText; // 显示页码信息，例如 "第 1/5 页"
    public Button prevPageButton;
    public Button nextPageButton;

    public Action<int> onYear;

    private StackedGraphManager graphManager;

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

    // 分页相关
    [SerializeField]
    private int dataPointsPerPage = 10; // 每页显示的数据点数量
    private int currentPage = 0;
    private int totalPages = 0;

    public int getYearByX(int id)
    {
        int tabRawId = currentPage * dataPointsPerPage + id;
        const int defaultYear = 2024;
        if (dataTable_country == null || dataTable_country.Rows.Count == 0)
            return defaultYear;

        object value = dataTable_country.Rows[tabRawId].ItemArray[0];

        if (value == null || value == DBNull.Value)
            return defaultYear;

        if (int.TryParse(value.ToString(), out int year))
            return year;
        else
            return defaultYear;
    }

    void Start()
    {
        string countryColorStr = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "CountryColor.json"));
        countryCategoryColor = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(countryColorStr);
        foreach (var key in countryCategoryColor.Keys.ToList())
        {
            var value = countryCategoryColor[key];
            if (!value.StartsWith("#"))
                countryCategoryColor[key] = "#" + value;
        }
        var dataset = SatelliteDataReader.ReadExcel(Path.Combine(Application.streamingAssetsPath, "历年各国卫星在轨数量.xlsx"), true);
        dataTable_country = dataset.Tables[0];
        graphManager = GetComponent<StackedGraphManager>();
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

        // 设置翻页按钮事件
        if (prevPageButton != null)
            prevPageButton.onClick.AddListener(PreviousPage);
        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(NextPage);

        // 加载所有数据
        PrepareSatelliteDataForRealtime();

        // 计算总页数
        totalPages = Mathf.CeilToInt((float)dataCount / dataPointsPerPage);

        // 显示第一页
        ShowCurrentPage();
        UpdatePageInfo();
    }

    private void OnEnable()
    {
        graphManager = GetComponent<StackedGraphManager>();
        if (graphManager == null || graphManager.Chart == null)
        {
            Debug.LogError("请确保物体上挂载了StackedGraphManager2组件并正确设置了Chart引用");
            return;
        }
        ResetPos();
    }

    public void ResetPos()
    {
        graphManager.Chart.HorizontalScrolling = 0;
    }

    private void InitializeCategories()
    {
        // 移除所有旧的
        foreach (var cat in graphManager.Chart.DataSource.CategoryNames.ToList())
            graphManager.Chart.DataSource.RemoveCategory(cat);

        // 重新建立categoriesList，严格和表头列顺一致
        categoriesList.Clear();
        categoryNameToColIndex.Clear();
        for (int col = 1; col < dataTable_country.Columns.Count; col++)
        {
            string catName = dataTable_country.Columns[col].ColumnName;
            categoriesList.Add(catName);
            categoryNameToColIndex[catName] = col - 1;

            Material lineMaterial = new Material(lineMat);
            Material graphMaterial = new Material(graphMat);
            Material pointMaterial = new Material(pointMat);

            SetMat(catName, lineMaterial);
            SetMat(catName, graphMaterial);
            SetMat(catName, pointMaterial);

            graphManager.Chart.DataSource.AddCategory(catName, lineMaterial, 5, new MaterialTiling(), graphMaterial, false, pointMaterial, 40);
            graphManager.Chart.DataSource.Set2DCategoryPrefabs(catName, LineHoverPrefab, PointHoverPrefab);
        }

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

    private void PrepareSatelliteDataForRealtime()
    {
        categoryCount = dataTable_country.Columns.Count - 1;
        dataCount = dataTable_country.Rows.Count;
        xData = new double[dataCount];
        yData = new double[dataCount, categoryCount];

        for (int i = 0; i < dataCount; i++)
        {
            DataRow row = dataTable_country.Rows[i];
            int year = Convert.ToInt32(row[0]);
            xData[i] = i;
            graphManager.Chart.HorizontalValueToStringMap[i] = year.ToString();

            for (int j = 1; j <= categoryCount; j++)
                yData[i, j - 1] = Convert.ToDouble(row[j]);
        }
        for (int i = 0; i < dataCount; i++)
        {
            DataRow row = dataTable_country.Rows[i];
            string debugRow = $"{row[0]}: ";
            for (int j = 1; j <= categoryCount; j++)
                debugRow += $"{row[j]},";
            Debug.Log(debugRow);
        }
    }


    private List<string> categoriesList = new List<string>();
    // 用于名字到yData列索引的映射
    private Dictionary<string, int> categoryNameToColIndex = new Dictionary<string, int>();
    // 显示当前页的数据
    private void ShowCurrentPage()
    {
        int startIndex = currentPage * dataPointsPerPage;
        int endIndex = Mathf.Min(startIndex + dataPointsPerPage, dataCount);
        int pageDataCount = endIndex - startIndex;

        // 创建当前页的数据数组
        double[] pageXData = new double[pageDataCount];
        double[,] pageYData = new double[pageDataCount, categoryCount];

        // 复制当前页的数据
        for (int i = 0; i < pageDataCount; i++)
        {
            int sourceIndex = startIndex + i;
            pageXData[i] = i; // 使用页内索引

            // 更新x轴标签映射（使用页内索引对应实际年份）
            if (graphManager.Chart.HorizontalValueToStringMap.ContainsKey(sourceIndex))
            {
                string yearLabel = graphManager.Chart.HorizontalValueToStringMap[sourceIndex];
                graphManager.Chart.HorizontalValueToStringMap[i] = yearLabel;
            }

            for (int j = 0; j < categoryCount; j++)
                pageYData[i, j] = yData[sourceIndex, j];
        }

        graphManager.InitialData(pageXData, pageYData, categoriesList, categoryNameToColIndex);
        ResetPos();
    }

    // 上一页
    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            ShowCurrentPage();
            UpdatePageInfo();
        }
    }

    // 下一页
    public void NextPage()
    {
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            ShowCurrentPage();
            UpdatePageInfo();
        }
    }

    // 更新页码信息
    private void UpdatePageInfo()
    {
        if (pageInfoText != null)
        {
            pageInfoText.text = $"第 {currentPage + 1}/{totalPages} 页";
        }

        // 更新按钮状态
        if (prevPageButton != null)
            prevPageButton.interactable = currentPage > 0;
        if (nextPageButton != null)
            nextPageButton.interactable = currentPage < totalPages - 1;
    }

    // 跳转到指定页
    public void GoToPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < totalPages)
        {
            currentPage = pageIndex;
            ShowCurrentPage();
            UpdatePageInfo();
        }
    }

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
        string yearStr = graphManager.Chart.HorizontalValueToStringMap.ContainsKey((int)point.x) ? graphManager.Chart.HorizontalValueToStringMap[(int)point.x] : ((int)point.x).ToString();
        infoText.text = string.Format("{0} 年 - {1}: {2} 颗卫星", yearStr
            ,
            args.Category, point.y);

        if (int.TryParse(yearStr, out int year))
        {
            onYear?.Invoke(year);
        }


    }

    public void NonHovered()
    {
        if (infoText == null) return;
        infoText.text = "";
    }
}