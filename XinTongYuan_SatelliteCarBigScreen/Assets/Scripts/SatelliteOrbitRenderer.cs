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
            // 使用HDRP/Unlit着色器，适合线条渲染
            orbitMaterial = new Material(Shader.Find("HDRP/Unlit"));

            // 设置HDRP Unlit材质属性
            orbitMaterial.SetFloat("_AlphaCutoffEnable", 0);
            orbitMaterial.SetFloat("_SurfaceType", 0); // 0 = Opaque, 1 = Transparent
            orbitMaterial.SetFloat("_BlendMode", 0);
            orbitMaterial.SetFloat("_SrcBlend", 1);
            orbitMaterial.SetFloat("_DstBlend", 0);
            orbitMaterial.SetFloat("_ZWrite", 1);
            orbitMaterial.SetFloat("_CullMode", 0);

            // 启用颜色属性
            orbitMaterial.EnableKeyword("_EMISSION");
            orbitMaterial.SetColor("_UnlitColor", Color.white);
            orbitMaterial.SetColor("_EmissiveColor", Color.white);
            orbitMaterial.SetFloat("_EmissiveIntensity", 1.0f);
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

    /// <summary>
    /// 解析TLE格式中的catalog number，支持字母编码
    /// </summary>
    /// <param name="catalogStr">catalog number字符串</param>
    /// <returns>解析后的数值</returns>
    private int ParseCatalogNumber(string catalogStr)
    {
        if (string.IsNullOrEmpty(catalogStr))
            return 0;

        catalogStr = catalogStr.Trim();

        // 尝试直接解析数字
        if (int.TryParse(catalogStr, out int result))
        {
            return result;
        }

        // 处理包含字母的情况
        // TLE格式中，当catalog number > 99999时，使用字母表示
        // A=10, B=11, C=12, ..., Z=35
        int value = 0;
        for (int i = 0; i < catalogStr.Length; i++)
        {
            char c = catalogStr[i];
            int digitValue;

            if (char.IsDigit(c))
            {
                digitValue = c - '0';
            }
            else if (char.IsLetter(c))
            {
                // 字母转换为数值：A=10, B=11, ..., Z=35
                digitValue = char.ToUpper(c) - 'A' + 10;
            }
            else
            {
                // 遇到无效字符，返回0或抛出异常
                Debug.LogWarning($"无效的catalog number字符: '{c}' in '{catalogStr}'");
                return 0;
            }

            value = value * 36 + digitValue; // 36进制
        }

        return value;
    }

    /// <summary>
    /// 改进的TLE第二行解析方法
    /// </summary>
    /// <param name="satellite">卫星数据对象</param>
    void ParseTLE2(SatelliteData satellite)
    {
        string tle2 = satellite.tle2;
        if (tle2.Length < 69)
        {
            Debug.LogWarning($"TLE第二行长度不足: {satellite.name} - 长度: {tle2.Length}");
            return;
        }

        try
        {
            // 解析catalog number (位置2-6，5个字符)
            string catalogStr = tle2.Substring(2, 5);
            satellite.catalogNumber = ParseCatalogNumber(catalogStr);

            // 解析其他轨道参数
            satellite.inclination = ParseFloat(tle2.Substring(8, 8), "inclination");
            satellite.raan = ParseFloat(tle2.Substring(17, 8), "raan");

            // 偏心率需要加上"0."前缀
            string eccentricityStr = tle2.Substring(26, 7).Trim();
            satellite.eccentricity = ParseFloat("0." + eccentricityStr, "eccentricity");

            satellite.argPerigee = ParseFloat(tle2.Substring(34, 8), "argPerigee");
            satellite.meanAnomaly = ParseFloat(tle2.Substring(43, 8), "meanAnomaly");
            satellite.meanMotion = ParseFloat(tle2.Substring(52, 11), "meanMotion");

            Debug.Log($"成功解析卫星: {satellite.name} (Catalog: {satellite.catalogNumber})");
        }
        catch (Exception e)
        {
            Debug.LogError($"解析TLE失败: {satellite.name} - {e.Message}");
            Debug.LogError($"TLE第二行: {tle2}");
        }
    }

    /// <summary>
    /// 安全的浮点数解析
    /// </summary>
    /// <param name="valueStr">要解析的字符串</param>
    /// <param name="fieldName">字段名称（用于错误信息）</param>
    /// <returns>解析后的浮点数</returns>
    private float ParseFloat(string valueStr, string fieldName)
    {
        valueStr = valueStr.Trim();

        if (float.TryParse(valueStr, out float result))
        {
            return result;
        }
        else
        {
            Debug.LogWarning($"无法解析{fieldName}: '{valueStr}'，使用默认值0");
            return 0f;
        }
    }

    /// <summary>
    /// 验证TLE数据完整性
    /// </summary>
    /// <param name="satellite">卫星数据</param>
    /// <returns>是否有效</returns>
    private bool ValidateSatelliteData(SatelliteData satellite)
    {
        // 检查必要字段
        if (satellite.catalogNumber <= 0)
        {
            Debug.LogWarning($"无效的catalog number: {satellite.catalogNumber} for {satellite.name}");
            return false;
        }

        if (satellite.inclination < 0 || satellite.inclination > 180)
        {
            Debug.LogWarning($"无效的倾角: {satellite.inclination}° for {satellite.name}");
            return false;
        }

        if (satellite.eccentricity < 0 || satellite.eccentricity >= 1)
        {
            Debug.LogWarning($"无效的偏心率: {satellite.eccentricity} for {satellite.name}");
            return false;
        }

        if (satellite.meanMotion <= 0)
        {
            Debug.LogWarning($"无效的平均运动: {satellite.meanMotion} for {satellite.name}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 改进的卫星数据解析方法
    /// </summary>
    void ParseSatelliteData()
    {
        int validCount = 0;
        int invalidCount = 0;

        foreach (var satellite in allSatellites)
        {
            if (string.IsNullOrEmpty(satellite.tle2))
            {
                Debug.LogWarning($"卫星 {satellite.name} 缺少TLE第二行数据");
                invalidCount++;
                continue;
            }

            ParseTLE2(satellite);

            if (ValidateSatelliteData(satellite))
            {
                var orbit = CalculateOrbitElements(satellite);
                orbitElements[satellite.catalogNumber] = orbit;
                validCount++;
            }
            else
            {
                invalidCount++;
            }
        }

        Debug.Log($"卫星数据解析完成: 有效 {validCount} 个, 无效 {invalidCount} 个");
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

        // 为每个轨道渲染
        for (int i = 0; i < batch.Count; i++)
        {
            int satNumber = batch[i];
            if (orbitMeshes.ContainsKey(satNumber))
            {
                var propertyBlock = new MaterialPropertyBlock();
                Color orbitColor = orbitColors[satNumber % orbitColors.Length];

                // 为HDRP设置正确的颜色属性
                propertyBlock.SetColor("_UnlitColor", orbitColor);
                propertyBlock.SetColor("_EmissiveColor", orbitColor);
                propertyBlock.SetFloat("_EmissiveIntensity", 1.0f);

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