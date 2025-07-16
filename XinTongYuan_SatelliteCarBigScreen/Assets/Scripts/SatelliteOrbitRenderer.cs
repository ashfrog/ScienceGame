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
    public int catalogNumber;

    // TLE轨道参数
    public float inclination;
    public float raan;
    public float eccentricity;
    public float argPerigee;
    public float meanAnomaly;
    public float meanMotion;
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
}

public class SatelliteOrbitRenderer : MonoBehaviour
{
    [Header("数据设置")]
    public string tleJsonFile = "tle.json";
    public string tleSelectionFile = "tlesel.json";

    [Header("渲染设置")]
    public Material orbitMaterial;
    public float orbitScale = 1f / 1000000f;
    public int orbitSegments = 90; // 减少分段数提升性能
    public bool useGPUInstancing = true;
    public int maxOrbitsPerBatch = 100;

    [Header("视觉效果")]
    public Color[] orbitColors = {
        Color.red, Color.blue, Color.green, Color.yellow,
        Color.magenta, Color.cyan, Color.white
    };

    // 核心数据
    private List<SatelliteData> allSatellites = new List<SatelliteData>();
    private Dictionary<int, OrbitElements> orbitElements = new Dictionary<int, OrbitElements>();

    // 渲染优化
    private Dictionary<int, Mesh> orbitMeshes = new Dictionary<int, Mesh>();
    private Dictionary<int, Material> orbitMaterials = new Dictionary<int, Material>();
    private List<Matrix4x4[]> instanceMatrices = new List<Matrix4x4[]>();
    private List<MaterialPropertyBlock[]> propertyBlocks = new List<MaterialPropertyBlock[]>();

    // 当前显示的轨道
    private List<int> currentDisplayedOrbits = new List<int>();
    Dictionary<string, TleSel> tleSelDic;
    void Start()
    {
        InitializeMaterials();
        LoadSatelliteData();
        ParseSatelliteData();
        LoadSelectionGroups();

        // 默认显示GPS
        SetDisplayGroup("GPS");
    }

    void Update()
    {
        if (useGPUInstancing)
        {
            RenderOrbitsWithInstancing();
        }

        HandleInput();
    }

    void InitializeMaterials()
    {
        if (orbitMaterial == null)
        {
            orbitMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            orbitMaterial.SetFloat("_Metallic", 0f);
            orbitMaterial.SetFloat("_Smoothness", 0.5f);
        }
    }

    void LoadSatelliteData()
    {
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, tleJsonFile);
            string jsonData = File.ReadAllText(filePath);
            allSatellites = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SatelliteData>>(jsonData);
            //allSatellites = Newtonsoft.Json.JsonConvert.DeserializeObject<SatelliteDataWrapper>(jsonData).satellites;
        }
        catch (Exception e)
        {
            Debug.LogError($"加载卫星数据失败: {e.Message}");
            // 创建测试数据
            CreateTestData();
        }
    }

    void LoadSelectionGroups()
    {
        try
        {
            String tleseldicfile = Path.Combine(Application.streamingAssetsPath, "tlesel.json");
            string tleseldicstr = File.ReadAllText(tleseldicfile);
            tleSelDic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, TleSel>>(tleseldicstr);
        }
        catch (Exception e)
        {
            Debug.LogError($"加载选择组失败: {e.Message}");
        }
    }

    void ParseSatelliteData()
    {
        foreach (var satellite in allSatellites)
        {
            if (string.IsNullOrEmpty(satellite.tle2)) continue;

            ParseTLE2(satellite);
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
            satellite.catalogNumber = int.Parse(tle2.Substring(2, 5).Trim());
            satellite.inclination = float.Parse(tle2.Substring(8, 8).Trim());
            satellite.raan = float.Parse(tle2.Substring(17, 8).Trim());
            satellite.eccentricity = float.Parse("0." + tle2.Substring(26, 7).Trim());
            satellite.argPerigee = float.Parse(tle2.Substring(34, 8).Trim());
            satellite.meanAnomaly = float.Parse(tle2.Substring(43, 8).Trim());
            satellite.meanMotion = float.Parse(tle2.Substring(52, 11).Trim());
        }
        catch (Exception e)
        {
            Debug.LogError($"解析TLE失败: {satellite.name} - {e.Message}");
        }
    }

    OrbitElements CalculateOrbitElements(SatelliteData satellite)
    {
        var orbit = new OrbitElements();

        // 计算半长轴
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

    void CreateOrbitMeshes(List<int> satelliteNumbers)
    {
        // 清除现有网格
        foreach (var mesh in orbitMeshes.Values)
        {
            if (mesh != null) DestroyImmediate(mesh);
        }
        orbitMeshes.Clear();

        // 为每个轨道创建网格
        foreach (int satNumber in satelliteNumbers)
        {
            if (orbitElements.ContainsKey(satNumber))
            {
                var orbit = orbitElements[satNumber];
                var mesh = CreateOrbitMesh(orbit);
                orbitMeshes[satNumber] = mesh;
            }
        }
    }

    Mesh CreateOrbitMesh(OrbitElements orbit)
    {
        var mesh = new Mesh();
        var vertices = new Vector3[orbitSegments + 1];
        var indices = new int[orbitSegments * 2];

        // 计算轨道点
        for (int i = 0; i <= orbitSegments; i++)
        {
            float trueAnomaly = (float)i / orbitSegments * 2 * Mathf.PI;
            float r = orbit.semiMajorAxis * (1 - orbit.eccentricity * orbit.eccentricity) /
                     (1 + orbit.eccentricity * Mathf.Cos(trueAnomaly));

            float x = r * Mathf.Cos(trueAnomaly);
            float y = r * Mathf.Sin(trueAnomaly);

            Vector3 orbitalPos = new Vector3(x, y, 0);
            Vector3 worldPos = TransformToECI(orbitalPos, orbit);
            vertices[i] = worldPos * orbitScale;
        }

        // 创建线条索引
        for (int i = 0; i < orbitSegments; i++)
        {
            indices[i * 2] = i;
            indices[i * 2 + 1] = i + 1;
        }

        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        mesh.RecalculateBounds();

        return mesh;
    }

    Vector3 TransformToECI(Vector3 orbitalPos, OrbitElements orbit)
    {
        // 轨道变换到地心坐标系
        float cosArgP = Mathf.Cos(orbit.argPerigee);
        float sinArgP = Mathf.Sin(orbit.argPerigee);
        float cosInc = Mathf.Cos(orbit.inclination);
        float sinInc = Mathf.Sin(orbit.inclination);
        float cosRaan = Mathf.Cos(orbit.raan);
        float sinRaan = Mathf.Sin(orbit.raan);

        // 近地点幅角旋转
        float x1 = orbitalPos.x * cosArgP - orbitalPos.y * sinArgP;
        float y1 = orbitalPos.x * sinArgP + orbitalPos.y * cosArgP;
        float z1 = orbitalPos.z;

        // 倾角旋转
        float x2 = x1;
        float y2 = y1 * cosInc - z1 * sinInc;
        float z2 = y1 * sinInc + z1 * cosInc;

        // 升交点赤经旋转
        float x3 = x2 * cosRaan - y2 * sinRaan;
        float y3 = x2 * sinRaan + y2 * cosRaan;
        float z3 = z2;

        return new Vector3(x3, z3, y3); // Unity坐标系转换
    }

    void RenderOrbitsWithInstancing()
    {
        if (currentDisplayedOrbits.Count == 0) return;

        // 批量渲染轨道
        for (int i = 0; i < currentDisplayedOrbits.Count; i += maxOrbitsPerBatch)
        {
            int batchSize = Mathf.Min(maxOrbitsPerBatch, currentDisplayedOrbits.Count - i);
            var batch = currentDisplayedOrbits.GetRange(i, batchSize);

            RenderOrbitBatch(batch);
        }
    }

    void RenderOrbitBatch(List<int> batch)
    {
        var matrices = new Matrix4x4[batch.Count];
        var colors = new Vector4[batch.Count];

        for (int i = 0; i < batch.Count; i++)
        {
            matrices[i] = Matrix4x4.identity;
            colors[i] = orbitColors[batch[i] % orbitColors.Length];
        }

        // 使用GPU实例化渲染
        var propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetVectorArray("_Color", colors);

        // 为每个轨道渲染
        for (int i = 0; i < batch.Count; i++)
        {
            int satNumber = batch[i];
            if (orbitMeshes.ContainsKey(satNumber))
            {
                propertyBlock.SetColor("_Color", orbitColors[satNumber % orbitColors.Length]);
                Graphics.DrawMesh(orbitMeshes[satNumber], matrices[i], orbitMaterial, 0, null, 0, propertyBlock);
            }
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.A)) SetDisplayGroup("GPS");
        if (Input.GetKeyDown(KeyCode.S)) SetDisplayGroup("格洛纳斯");
        if (Input.GetKeyDown(KeyCode.D)) SetDisplayGroup("星链");
        if (Input.GetKeyDown(KeyCode.G)) SetDisplayGroup("Galileo");
        if (Input.GetKeyDown(KeyCode.F)) SetDisplayGroup("BeiDou");

    }

    public void SetDisplayGroup(string groupName)
    {
        if (tleSelDic.ContainsKey(groupName))
        {
            currentDisplayedOrbits = tleSelDic[groupName].sel;
            CreateOrbitMeshes(currentDisplayedOrbits);
            Debug.Log($"显示 {groupName} 卫星群: {currentDisplayedOrbits.Count} 个轨道");
        }
        else
        {
            Debug.LogWarning($"未找到卫星群: {groupName}");
        }
    }

    void CreateTestData()
    {
        // 创建测试数据
        for (int i = 0; i < 10; i++)
        {
            var satellite = new SatelliteData
            {
                name = $"TestSat_{i}",
                catalogNumber = 10000 + i,
                inclination = UnityEngine.Random.Range(0f, 180f),
                raan = UnityEngine.Random.Range(0f, 360f),
                eccentricity = UnityEngine.Random.Range(0f, 0.1f),
                argPerigee = UnityEngine.Random.Range(0f, 360f),
                meanAnomaly = UnityEngine.Random.Range(0f, 360f),
                meanMotion = UnityEngine.Random.Range(10f, 16f)
            };
            allSatellites.Add(satellite);
        }
    }



    public void SetOrbitSegments(int segments)
    {
        orbitSegments = Mathf.Clamp(segments, 30, 360);
        if (currentDisplayedOrbits.Count > 0)
        {
            CreateOrbitMeshes(currentDisplayedOrbits);
        }
    }

    public void SetMaxOrbitsPerBatch(int maxBatch)
    {
        maxOrbitsPerBatch = Mathf.Clamp(maxBatch, 10, 1000);
    }

    void OnDestroy()
    {
        // 清理资源
        foreach (var mesh in orbitMeshes.Values)
        {
            if (mesh != null) DestroyImmediate(mesh);
        }
        orbitMeshes.Clear();
    }

    public class TleSel
    {
        public List<int> sel;
        public float pitch;
        public float yaw;
        public float fov;
    }
}