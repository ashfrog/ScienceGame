#define Graph_And_Chart_PRO
using ChartAndGraph;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StackedGraph2Example : MonoBehaviour
{
    public int NumCategories = 3;
    public Text infoText;
    void Start()
    {
        StartCoroutine(AddPoints());
    }
    private readonly int[] years = new int[]
    {
        1,2,3,4,5,6,100,150,200
    };
    IEnumerator AddPoints()
    {
        int x = 100;
        var manager = GetComponent<StackedGraphManager2>();
        //manager.Chart.PointHovered.AddListener(GraphHoverd);
        //manager.Chart.NonHovered.AddListener(NonHovered);
        //double[] xArr = Enumerable.Range(0, x).Select(num => (double)num).ToArray();
        double[] xArr = years.Select(y => (double)y).ToArray();
        double[,] yArr = new double[xArr.Length, NumCategories];
        for (int i = 0; i < yArr.GetLength(0); i++)
            for (int j = 0; j < yArr.GetLength(1); j++)
            {
                yArr[i, j] = Random.value * 3;
            }
        manager.InitialData(xArr, yArr);
        double[] newPointYArr = new double[NumCategories];
        //while (true)
        //{
        //    yield return new WaitForSeconds(5.0f);
        //    x++;
        //    for (int i = 0; i < newPointYArr.Length; i++)
        //        newPointYArr[i] = Random.value * 3;
        //    manager.AddPointRealtime(x, newPointYArr, 1f);

        //}
        yield return null;
    }

    public void Toogle(string name)
    {
        var manager = GetComponent<StackedGraphManager2>();
        if (manager != null)
        {
            manager.ToggleCategoryEnabled(name);
        }
    }
    public void GraphHoverd(GraphChartBase.GraphEventArgs args)
    {
        if (infoText == null)
            return;
        var manager = GetComponent<StackedGraphManager2>();
        var point = manager.GetPointValue(args.Category, args.Index);
        infoText.text = string.Format("{0} : {1},{2:0.##}", args.Category, point.x, point.y);
    }
    public void NonHovered()
    {
        if (infoText == null)
            return;
        infoText.text = "";
    }
    // Update is called once per frame
    void Update()
    {

    }
}
