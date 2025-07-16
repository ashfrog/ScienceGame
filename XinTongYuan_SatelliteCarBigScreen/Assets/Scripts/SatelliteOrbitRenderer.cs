using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SatelliteData
{
    public string tle1;
    public string tle2;
    public string name;
    public string altName;
    public float vmag;
    public string launchDate;
    public string owner;
    public string country;
    public float mass;
    public float diameter;
    public float length;

    // TLE轨道参数（从TLE2行解析）
    public float inclination;      // 倾角
    public float raan;            // 升交点赤经
    public float eccentricity;    // 离心率
    public float argPerigee;      // 近地点幅角
    public float meanAnomaly;     // 平近点角
    public float meanMotion;      // 平均角速度
    public int catalogNumber;     // 卫星编号
    public float epoch;           // 历元
}

[System.Serializable]
public class OrbitElements
{
    public float semiMajorAxis;
    public float eccentricity;
    public float inclination;
    public float raan;
    public float argPerigee;
    public float meanAnomaly;
    public float meanMotion;
    public float epoch;
}

public class SatelliteOrbitRenderer : MonoBehaviour
{
    [Header("卫星数据")]
    private List<SatelliteData> allSatellites = new List<SatelliteData>();

    [Header("显示设置")]
    private List<int> targetSatelliteNumbers = new List<int>() { 10684, 10893, 11054, 11141, 11690, 11783, 14189, 15039, 15271, 16129, 19802, 20061, 20185, 20302, 20361, 20452, 20533, 20724, 20830, 20959, 21552, 21890, 21930, 22014, 22108, 22231, 22275, 22446, 22581, 22657, 22700, 22779, 22877, 23027, 23833, 23953, 24320, 24876, 25030, 25933, 26360, 26407, 26605, 26690, 27663, 27704, 28129, 28190, 28361, 28474, 28874, 29486, 29601, 32260, 32384, 32711, 34661, 35752, 36585, 37753, 38833, 39166, 39533, 39741, 40105, 40294, 40534, 40730, 41019, 41328, 43873, 44506, 45854, 46826, 48859, 55268, 62339 }; // 默认显示的卫星编号
    public Material orbitMaterial;
    public Material satelliteMaterial;
    public GameObject satellitePrefab;
    public float orbitScale = 1f / 1000000f; // 轨道缩放比例
    public int orbitSegments = 360;
    public bool showOrbitLabels = true;
    public float updateInterval = 1f; // 更新间隔（秒）
    public float orbitLineWidth = 0.02f;
    [Header("视觉效果")]
    public Color[] orbitColors = {
        Color.red, Color.blue, Color.green, Color.yellow,
        Color.magenta, Color.cyan, Color.white
    };

    private Dictionary<int, GameObject> satelliteObjects = new Dictionary<int, GameObject>();
    private Dictionary<int, LineRenderer> orbitLines = new Dictionary<int, LineRenderer>();
    private Dictionary<int, OrbitElements> orbitElements = new Dictionary<int, OrbitElements>();
    private float lastUpdateTime;

    private List<int> 伽利略 = new List<int> { 37846, 37847, 38857, 38858, 40128, 40129, 40544, 40545, 40889, 40890, 41174, 41175, 41549, 41550, 41859, 41860, 41861, 41862, 43055, 43056, 43057, 43058, 43564, 43565, 43566, 43567, 49809, 49810, 59598, 59600, 61182, 61183 };
    Dictionary<string, TleSel> tleSelDic;
    void Start()
    {
        // 如果没有预设材质，创建默认材质
        if (orbitMaterial == null)
        {
            orbitMaterial = new Material(Shader.Find("Sprites/Default"));
        }
        if (satelliteMaterial == null)
        {
            satelliteMaterial = new Material(Shader.Find("Standard"));
        }
        String tlejsonfile = Path.Combine(Application.streamingAssetsPath, "tle.json");
        string tlejsonstr = File.ReadAllText(tlejsonfile);
        allSatellites = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SatelliteData>>(tlejsonstr);


        String tleseldicfile = Path.Combine(Application.streamingAssetsPath, "tlesel.json");
        string tleseldicstr = File.ReadAllText(tleseldicfile);
        tleSelDic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, TleSel>>(tleseldicstr);

        // 解析卫星数据并创建轨道
        ParseSatelliteData();




        SetTargetSatellites("GPS");

        lastUpdateTime = Time.time;
    }

    void Update()
    {
        if (Time.time - lastUpdateTime > updateInterval)
        {
            UpdateSatellitePositions();
            lastUpdateTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            SetTargetSatellites("GPS");
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            SetTargetSatellites("格洛纳斯");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            SetTargetSatellites("星链");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {

        }
    }

    void ParseSatelliteData()
    {
        foreach (var satellite in allSatellites)
        {
            if (string.IsNullOrEmpty(satellite.tle2)) continue;

            // 解析TLE第二行
            ParseTLE2(satellite);

            // 计算轨道要素
            var orbit = CalculateOrbitElements(satellite);
            orbitElements[satellite.catalogNumber] = orbit;
        }
    }

    void ParseTLE2(SatelliteData satellite)
    {
        string tle2 = satellite.tle2;
        if (tle2.Length < 69) return;

        try
        {
            // 解析卫星编号（列3-7）
            satellite.catalogNumber = int.Parse(tle2.Substring(2, 5).Trim());

            // 解析轨道要素
            satellite.inclination = float.Parse(tle2.Substring(8, 8).Trim());
            satellite.raan = float.Parse(tle2.Substring(17, 8).Trim());
            satellite.eccentricity = float.Parse("0." + tle2.Substring(26, 7).Trim());
            satellite.argPerigee = float.Parse(tle2.Substring(34, 8).Trim());
            satellite.meanAnomaly = float.Parse(tle2.Substring(43, 8).Trim());
            satellite.meanMotion = float.Parse(tle2.Substring(52, 11).Trim());

            Debug.Log($"解析卫星 {satellite.name} (编号: {satellite.catalogNumber})");
        }
        catch (Exception e)
        {
            Debug.LogError($"解析TLE数据失败: {satellite.name} - {e.Message}");
        }
    }

    OrbitElements CalculateOrbitElements(SatelliteData satellite)
    {
        var orbit = new OrbitElements();

        // 计算半长轴 a = (μ/(n²))^(1/3)
        // μ = 3.986004418e14 m³/s² (地球引力参数)
        // n = meanMotion * 2π / 86400 (弧度/秒)
        double mu = 3.986004418e14;
        double n = satellite.meanMotion * 2 * Math.PI / 86400.0;
        orbit.semiMajorAxis = (float)Math.Pow(mu / (n * n), 1.0 / 3.0);

        orbit.eccentricity = satellite.eccentricity;
        orbit.inclination = satellite.inclination * Mathf.Deg2Rad;
        orbit.raan = satellite.raan * Mathf.Deg2Rad;
        orbit.argPerigee = satellite.argPerigee * Mathf.Deg2Rad;
        orbit.meanAnomaly = satellite.meanAnomaly * Mathf.Deg2Rad;
        orbit.meanMotion = satellite.meanMotion;

        return orbit;
    }

    void CreateOrbitsForTargetSatellites(List<int> targetSatelliteNumbers)
    {
        foreach (int satNumber in targetSatelliteNumbers)
        {
            var satellite = allSatellites.Find(s => s.catalogNumber == satNumber);
            if (satellite != null && orbitElements.ContainsKey(satNumber))
            {
                CreateOrbitVisualization(satellite, orbitElements[satNumber]);
                CreateSatelliteObject(satellite);
            }
            else
            {
                Debug.LogWarning($"未找到卫星编号 {satNumber} 的数据");
            }
        }
    }

    void CreateOrbitVisualization(SatelliteData satellite, OrbitElements orbit)
    {
        // 创建轨道线渲染器
        GameObject orbitObject = new GameObject($"Orbit_{satellite.name}");
        orbitObject.transform.parent = transform;

        LineRenderer lineRenderer = orbitObject.AddComponent<LineRenderer>();
        lineRenderer.material = orbitMaterial;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = orbitSegments + 1;
        lineRenderer.useWorldSpace = true;

        // 设置轨道颜色
        Color orbitColor = orbitColors[satellite.catalogNumber % orbitColors.Length];
        lineRenderer.startColor = orbitColor;
        lineRenderer.endColor = orbitColor;
        //lineRenderer.color = orbitColor;

        // 计算轨道点
        Vector3[] orbitPoints = CalculateOrbitPoints(orbit);
        lineRenderer.SetPositions(orbitPoints);

        orbitLines[satellite.catalogNumber] = lineRenderer;

        // 添加标签
        if (showOrbitLabels)
        {
            CreateOrbitLabel(orbitObject, satellite);
        }
    }

    Vector3[] CalculateOrbitPoints(OrbitElements orbit)
    {
        Vector3[] points = new Vector3[orbitSegments + 1];

        for (int i = 0; i <= orbitSegments; i++)
        {
            float trueAnomaly = (float)i / orbitSegments * 2 * Mathf.PI;

            // 计算轨道平面内的位置
            float r = orbit.semiMajorAxis * (1 - orbit.eccentricity * orbit.eccentricity) /
                     (1 + orbit.eccentricity * Mathf.Cos(trueAnomaly));

            float x = r * Mathf.Cos(trueAnomaly);
            float y = r * Mathf.Sin(trueAnomaly);

            // 转换到地心坐标系
            Vector3 orbitalPos = new Vector3(x, y, 0);
            Vector3 worldPos = TransformToECI(orbitalPos, orbit);

            points[i] = worldPos * orbitScale;
        }

        return points;
    }

    Vector3 TransformToECI(Vector3 orbitalPos, OrbitElements orbit)
    {
        // 轨道平面到地心惯性坐标系的变换
        // 依次应用：近地点幅角、倾角、升交点赤经的旋转

        // 绕Z轴旋转近地点幅角
        float cosArgP = Mathf.Cos(orbit.argPerigee);
        float sinArgP = Mathf.Sin(orbit.argPerigee);

        float x1 = orbitalPos.x * cosArgP - orbitalPos.y * sinArgP;
        float y1 = orbitalPos.x * sinArgP + orbitalPos.y * cosArgP;
        float z1 = orbitalPos.z;

        // 绕X轴旋转倾角
        float cosInc = Mathf.Cos(orbit.inclination);
        float sinInc = Mathf.Sin(orbit.inclination);

        float x2 = x1;
        float y2 = y1 * cosInc - z1 * sinInc;
        float z2 = y1 * sinInc + z1 * cosInc;

        // 绕Z轴旋转升交点赤经
        float cosRaan = Mathf.Cos(orbit.raan);
        float sinRaan = Mathf.Sin(orbit.raan);

        float x3 = x2 * cosRaan - y2 * sinRaan;
        float y3 = x2 * sinRaan + y2 * cosRaan;
        float z3 = z2;

        return new Vector3(x3, z3, y3);//Unity Y 轴朝上，轨道动力学 Z 轴朝上
    }

    void CreateSatelliteObject(SatelliteData satellite)
    {
        GameObject satObject;

        if (satellitePrefab != null)
        {
            satObject = Instantiate(satellitePrefab);
        }
        else
        {
            satObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            satObject.transform.localScale = Vector3.one * 0.1f;
        }

        satObject.name = $"Satellite_{satellite.name}";
        satObject.transform.parent = transform;

        // 设置材质
        if (satelliteMaterial != null)
        {
            satObject.GetComponent<Renderer>().material = satelliteMaterial;
        }

        // 设置颜色
        Color satColor = orbitColors[satellite.catalogNumber % orbitColors.Length];
        satObject.GetComponent<Renderer>().material.color = satColor;

        satelliteObjects[satellite.catalogNumber] = satObject;
    }

    void CreateOrbitLabel(GameObject orbitObject, SatelliteData satellite)
    {
        GameObject labelObject = new GameObject($"Label_{satellite.name}");
        labelObject.transform.parent = orbitObject.transform;

        // 这里可以添加文本网格或UI文本来显示卫星信息
        // 由于Unity的TextMesh组件需要在Editor中设置，这里提供基本框架

        // 可以使用TextMeshPro或其他文本组件
        var textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = $"{satellite.name}\n编号: {satellite.catalogNumber}";
        textMesh.fontSize = 20;
        textMesh.color = Color.white;
        textMesh.anchor = TextAnchor.MiddleCenter;

        labelObject.transform.localScale = Vector3.one * 0.01f;
    }

    void UpdateSatellitePositions()
    {
        float currentTime = Time.time;

        foreach (var kvp in satelliteObjects)
        {
            int satNumber = kvp.Key;
            GameObject satObject = kvp.Value;

            if (orbitElements.ContainsKey(satNumber))
            {
                var orbit = orbitElements[satNumber];
                Vector3 currentPosition = CalculateCurrentPosition(orbit, currentTime);
                satObject.transform.position = currentPosition * orbitScale;
            }
        }
    }

    Vector3 CalculateCurrentPosition(OrbitElements orbit, float currentTime)
    {
        // 简化的轨道传播（实际应用中需要更精确的算法）
        float timeSinceEpoch = currentTime; // 简化时间计算
        float meanAnomaly = orbit.meanAnomaly + orbit.meanMotion * timeSinceEpoch * 2 * Mathf.PI / 86400f;

        // 求解开普勒方程得到偏近点角
        float eccentricAnomaly = SolveKeplerEquation(meanAnomaly, orbit.eccentricity);

        // 计算真近点角
        float trueAnomaly = 2 * Mathf.Atan2(
            Mathf.Sqrt(1 + orbit.eccentricity) * Mathf.Sin(eccentricAnomaly / 2),
            Mathf.Sqrt(1 - orbit.eccentricity) * Mathf.Cos(eccentricAnomaly / 2)
        );

        // 计算距离
        float r = orbit.semiMajorAxis * (1 - orbit.eccentricity * orbit.eccentricity) /
                 (1 + orbit.eccentricity * Mathf.Cos(trueAnomaly));

        // 轨道平面内的位置
        Vector3 orbitalPos = new Vector3(
            r * Mathf.Cos(trueAnomaly),
            r * Mathf.Sin(trueAnomaly),
            0
        );

        // 转换到地心坐标系
        return TransformToECI(orbitalPos, orbit);
    }

    float SolveKeplerEquation(float meanAnomaly, float eccentricity, int maxIterations = 10)
    {
        // 牛顿法求解开普勒方程 E - e*sin(E) = M
        float E = meanAnomaly; // 初始猜测

        for (int i = 0; i < maxIterations; i++)
        {
            float f = E - eccentricity * Mathf.Sin(E) - meanAnomaly;
            float df = 1 - eccentricity * Mathf.Cos(E);

            float deltaE = f / df;
            E -= deltaE;

            if (Mathf.Abs(deltaE) < 1e-8f)
                break;
        }

        return E;
    }

    // 公共方法：添加新的目标卫星
    public void AddTargetSatellite(int satelliteNumber)
    {
        if (!targetSatelliteNumbers.Contains(satelliteNumber))
        {
            targetSatelliteNumbers.Add(satelliteNumber);

            var satellite = allSatellites.Find(s => s.catalogNumber == satelliteNumber);
            if (satellite != null && orbitElements.ContainsKey(satelliteNumber))
            {
                CreateOrbitVisualization(satellite, orbitElements[satelliteNumber]);
                CreateSatelliteObject(satellite);
            }
        }
    }

    // 公共方法：移除目标卫星
    public void RemoveTargetSatellite(int satelliteNumber)
    {
        targetSatelliteNumbers.Remove(satelliteNumber);

        if (satelliteObjects.ContainsKey(satelliteNumber))
        {
            DestroyImmediate(satelliteObjects[satelliteNumber]);
            satelliteObjects.Remove(satelliteNumber);
        }

        if (orbitLines.ContainsKey(satelliteNumber))
        {
            DestroyImmediate(orbitLines[satelliteNumber].gameObject);
            orbitLines.Remove(satelliteNumber);
        }
    }

    // 公共方法：设置显示的卫星列表
    public void SetTargetSatellites(string tlekey)
    {
        tleSelDic.TryGetValue(tlekey, out TleSel tleSel);
        // 清除现有显示
        foreach (var kvp in satelliteObjects)
        {
            DestroyImmediate(kvp.Value);
        }
        foreach (var kvp in orbitLines)
        {
            DestroyImmediate(kvp.Value.gameObject);
        }

        satelliteObjects.Clear();
        orbitLines.Clear();

        // 设置新的目标列表
        targetSatelliteNumbers = tleSel.sel;
        CreateOrbitsForTargetSatellites(targetSatelliteNumbers);
    }
}

public class TleSel
{
    public List<int> sel;
    public float pitch;
    public float yaw;
    public float fov;
}

// 卫星信息组件
public class SatelliteInfo : MonoBehaviour
{
    public SatelliteData satelliteData;

    void OnMouseDown()
    {
        ShowSatelliteInfo();
    }

    void ShowSatelliteInfo()
    {
        if (satelliteData != null)
        {
            string info = $"卫星名称: {satelliteData.name}\n" +
                         $"编号: {satelliteData.catalogNumber}\n" +
                         $"所有者: {satelliteData.owner}\n" +
                         $"国家: {satelliteData.country}\n" +
                         $"质量: {satelliteData.mass} kg\n" +
                         $"发射日期: {satelliteData.launchDate}";

            Debug.Log(info);
            // 这里可以显示UI面板来展示卫星信息
        }
    }


}