using System;
using System.Data;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using ExcelDataReader;

public class SatelliteDataReader : MonoBehaviour
{
    public GameObject satelliteDataRowPrefab; // 预制体
    public Transform contentParent; // 用于放置预制体的父对象
    public Sprite s;
    void Start()
    {
        //initialize(0);
    }

    public void initialize(int index) //index代表读取第几个表
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        string filePath = Application.streamingAssetsPath + "/所有星座.xlsx";
        DataSet result = ReadExcel(filePath);

        if (result != null)
        {
            DataTable table = result.Tables[index];

            for (int i = 0; i < table.Rows.Count; i++) // 从1开始跳过标题行
            {
                if (i == 0)
                {
                    
                }
                DataRow row = table.Rows[i];
                CreateDataRow(row,i);
            }
        }
    }

    DataSet ReadExcel(string filePath)
    {
        try
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    return result;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading Excel file: " + e.Message);
            return null;
        }
    }

    void CreateDataRow(DataRow row,int i)
    {
        
        GameObject newRow = Instantiate(satelliteDataRowPrefab, contentParent);
        newRow.transform.Find("ChineseNameText").GetComponent<Text>().text = row[0].ToString();
        newRow.transform.Find("EnglishNameText").GetComponent<Text>().text = row[1].ToString();
        newRow.transform.Find("ResearchInstitutionText").GetComponent<Text>().text = row[2].ToString();
        newRow.transform.Find("LaunchDateText").GetComponent<Text>().text = row[3].ToString();
        newRow.transform.Find("CarrierRocketText").GetComponent<Text>().text = row[4].ToString();
        newRow.transform.Find("LaunchSiteText").GetComponent<Text>().text = row[5].ToString();
        newRow.transform.Find("OrbitHeightText").GetComponent<Text>().text = row[6].ToString();
        newRow.transform.Find("OrbitInclinationText").GetComponent<Text>().text = row[7].ToString();
        newRow.transform.Find("DesignLifeText").GetComponent<Text>().text = row[8].ToString();
        newRow.transform.Find("MassText").GetComponent<Text>().text = row[9].ToString();
        newRow.transform.Find("COSPARText").GetComponent<Text>().text = row[10].ToString();
        if (i % 2 != 0)
        {
            var img = newRow.GetComponent<Image>();
            Destroy(img);
        }
        if (i == 0)
        {
            newRow.GetComponent<Image>().sprite = s;
            for (int j = 0; j < newRow.transform.childCount; j++)
            {
                newRow.transform.GetChild(j).GetComponent<Text>().fontStyle = FontStyle.Bold;
            }
            
            Manager._ins._stickyHeaderTable.headerRow = newRow;
            newRow.transform.SetParent(Manager._ins._stickyHeaderTable.scrollRect.transform.GetChild(0).transform);
        }
    }
}