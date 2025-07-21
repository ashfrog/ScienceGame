using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
/// <summary>
/// https://app.keeptrack.space
/// </summary>
[System.Serializable]
public class SatelliteData
{
    public string tle1;
    public string tle2;
    public string name;
    public int catalogNumber;
    public int cachedYear;

    // TLE轨道参数
    public float inclination;
    public float raan;
    public float eccentricity;
    public float argPerigee;
    public float meanAnomaly;
    public float meanMotion;

    //国家
    public string country;
    public string bus;
    public string stableDate;
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

public enum DisplayMode
{
    None,           // 不显示任何内容
    OrbitOnly,      // 仅显示轨道
    SatelliteOnly,  // 仅显示卫星点
    Both            // 显示轨道和卫星点
}

public class SatelliteOrbitRenderer : MonoBehaviour
{

    [Header("渲染设置")]
    public Material orbitMaterial;
    public Material satelliteMaterial;
    public float orbitScale = 1f / 1000000f;
    public int orbitSegments = 90;
    public bool useGPUInstancing = true;
    public int maxOrbitsPerBatch = 100;
    public float satelliteSize = 0.01f;

    [Header("筛选设置")]
    public bool enableYearFilter = false;
    public bool enableCountryFilter = false;
    public int filterMinYear = 1970;
    public int filterMaxYear = 2025;
    public string[] selectedCountries = new string[] { "US", "CN", "RU" }; // 默认选择的国家

    // 添加筛选后的卫星数据缓存
    private List<SatelliteData> filteredSatellites = new List<SatelliteData>();

    [Header("显示模式")]
    public DisplayMode displayMode = DisplayMode.Both;

    [Header("国家颜色设置")]
    public Dictionary<string, Color[]> countryGroupColorGroups;
    // 当前显示星座的名称
    private string currentDisplayGroupName = "";


    // 核心数据 - 预解析的数据，避免重复解析
    private List<SatelliteData> allSatellites = new List<SatelliteData>();
    private Dictionary<int, OrbitElements> allOrbitElements = new Dictionary<int, OrbitElements>(); // 存储所有轨道元素
    private Dictionary<int, SatelliteData> catalogToSatellite = new Dictionary<int, SatelliteData>(); // catalog号到卫星的映射

    // 筛选后的数据缓存
    private HashSet<int> filteredCatalogNumbers = new HashSet<int>(); // 筛选后的catalog号集合
    private Dictionary<int, int> catalogToYear = new Dictionary<int, int>(); // catalog号到年份的映射，预计算
    private Dictionary<string, HashSet<int>> countryToCatalogs = new Dictionary<string, HashSet<int>>(); // 国家到catalog号的映射


    // 核心数据
    private Dictionary<int, OrbitElements> orbitElements = new Dictionary<int, OrbitElements>();

    // 渲染优化
    private Dictionary<int, Mesh> orbitMeshes = new Dictionary<int, Mesh>();
    private Dictionary<int, Vector3> currentSatellitePositions = new Dictionary<int, Vector3>();
    private Mesh satelliteMesh;

    // 当前显示的轨道
    private List<int> currentDisplayedOrbits = new List<int>();
    private HashSet<int> currentDisplayedOrbitsSet = new HashSet<int>(); // 用于快速查找
    Dictionary<string, TleSel> tleSelDic;

    // 在类的顶部添加时间偏移字典
    private Dictionary<int, float> satelliteTimeOffsets = new Dictionary<int, float>();

    [Header("性能优化")]
    public int maxDisplayOrbits = 200; // 最大显示轨道数量

    public int maxDisplaySatellites = 2000;//最大显示卫星数量

    // 用于避免重复GC的缓存列表
    private List<int> tempCatalogList = new List<int>(5000);
    private List<int> tempDisplayList = new List<int>(1000);

    void Start()
    {
        SetMaxDisplay(Settings.ini.Game.MaxDisplayOrbits, Settings.ini.Game.MaxDisplaySatellite);
        maxOrbitsPerBatch = Settings.ini.Game.MaxOrbitsPerBatch;
        InitializeMaterials();
        CreateSatelliteMesh();
        LoadSatelliteData();
        LoadCountryColorGroups();
        ParseSatelliteData();

        PreprocessSatelliteData(); // 预处理所有数据
        LoadSelectionGroups();

        // 默认显示GPS
        SetDisplayGroup("格洛纳斯");
    }

    private float logMinYear;
    private float logMaxYear;
    void Update()
    {
        HandleInput();

        if (displayMode == DisplayMode.None)
        {
            return;
        }

        UpdateSatellitePositions();

        if (displayMode == DisplayMode.OrbitOnly || displayMode == DisplayMode.Both)
        {
            RenderOrbitsWithInstancing();
        }

        if (displayMode == DisplayMode.SatelliteOnly || displayMode == DisplayMode.Both)
        {
            RenderSatellites();
        }
    }

    // 获取可用国家列表的方法
    public string[] GetAvailableCountries()
    {
        HashSet<string> countries = new HashSet<string>();
        foreach (var satellite in allSatellites)
        {
            if (!string.IsNullOrEmpty(satellite.country))
            {
                countries.Add(satellite.country);
            }
        }
        return countries.ToArray();
    }

    void InitializeMaterials()
    {
        if (orbitMaterial == null)
        {
            orbitMaterial = new Material(Shader.Find("HDRP/Unlit"));
            orbitMaterial.SetFloat("_AlphaCutoffEnable", 0);
            orbitMaterial.SetFloat("_SurfaceType", 0);
            orbitMaterial.SetFloat("_BlendMode", 0);
            orbitMaterial.SetFloat("_SrcBlend", 1);
            orbitMaterial.SetFloat("_DstBlend", 0);
            orbitMaterial.SetFloat("_ZWrite", 1);
            orbitMaterial.SetFloat("_CullMode", 0);
            orbitMaterial.EnableKeyword("_EMISSION");
            orbitMaterial.SetColor("_UnlitColor", Color.white);
            orbitMaterial.SetColor("_EmissiveColor", Color.white);
            orbitMaterial.SetFloat("_EmissiveIntensity", 1.0f);
        }

        if (satelliteMaterial == null)
        {
            satelliteMaterial = new Material(Shader.Find("HDRP/Unlit"));
            satelliteMaterial.SetFloat("_SurfaceType", 0);
            satelliteMaterial.EnableKeyword("_EMISSION");
            satelliteMaterial.SetColor("_UnlitColor", Color.white);
            satelliteMaterial.SetColor("_EmissiveColor", Color.white);
            satelliteMaterial.SetFloat("_EmissiveIntensity", 2.0f);
        }
    }

    void CreateSatelliteMesh()
    {
        satelliteMesh = new Mesh();

        int latitudeSegments = 16;   // 纬度分段数
        int longitudeSegments = 24;  // 经度分段数
        float radius = satelliteSize;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // 生成球体顶点
        for (int lat = 0; lat <= latitudeSegments; lat++)
        {
            float theta = lat * Mathf.PI / latitudeSegments;
            float sinTheta = Mathf.Sin(theta);
            float cosTheta = Mathf.Cos(theta);

            for (int lon = 0; lon <= longitudeSegments; lon++)
            {
                float phi = lon * 2 * Mathf.PI / longitudeSegments;
                float sinPhi = Mathf.Sin(phi);
                float cosPhi = Mathf.Cos(phi);

                float x = radius * sinTheta * cosPhi;
                float y = radius * cosTheta;
                float z = radius * sinTheta * sinPhi;
                vertices.Add(new Vector3(x, y, z));
            }
        }

        // 生成三角形索引
        for (int lat = 0; lat < latitudeSegments; lat++)
        {
            for (int lon = 0; lon < longitudeSegments; lon++)
            {
                int first = lat * (longitudeSegments + 1) + lon;
                int second = first + longitudeSegments + 1;

                triangles.Add(first);
                triangles.Add(second);
                triangles.Add(first + 1);

                triangles.Add(second);
                triangles.Add(second + 1);
                triangles.Add(first + 1);
            }
        }

        satelliteMesh.vertices = vertices.ToArray();
        satelliteMesh.triangles = triangles.ToArray();
        satelliteMesh.RecalculateNormals();
        satelliteMesh.RecalculateBounds();
    }
    void LoadSatelliteData()
    {
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "tle.json");
            string jsonData = File.ReadAllText(filePath);
            allSatellites = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SatelliteData>>(jsonData);

            List<string> buses = new List<string>();
            foreach (var satellite in allSatellites)
            {
                if (satellite.country == "CN" && satellite.bus != null)
                {
                    buses.Add(satellite.bus);
                }
            }
            Debug.Log(buses.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError($"加载卫星数据失败: {e.Message}");
        }
    }

    void LoadCountryColorGroups()
    {
        try
        {
            countryGroupColorGroups = LoadCountryGroupsColor(Path.Combine(Application.streamingAssetsPath, "CountryGroupColors.json"));
        }
        catch (Exception ex)
        {
            Debug.LogError($"加载国家颜色组失败: {ex.Message}");
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
    // 预处理所有卫星数据 - 一次性解析和验证
    void PreprocessSatelliteData()
    {
        int validCount = 0;
        int invalidCount = 0;

        // 清空映射表
        catalogToSatellite.Clear();
        catalogToYear.Clear();
        countryToCatalogs.Clear();
        allOrbitElements.Clear();

        foreach (var satellite in allSatellites)
        {
            if (string.IsNullOrEmpty(satellite.tle2))
            {
                invalidCount++;
                continue;
            }

            ParseTLE2(satellite);

            if (ValidateSatelliteData(satellite))
            {
                // 计算轨道元素并缓存
                var orbit = CalculateOrbitElements(satellite);
                allOrbitElements[satellite.catalogNumber] = orbit;
                catalogToSatellite[satellite.catalogNumber] = satellite;

                // 预计算年份
                int year = ParseYearFromDate(satellite.stableDate);
                catalogToYear[satellite.catalogNumber] = year;
                satellite.cachedYear = year;

                // 建立国家到catalog的映射
                if (!string.IsNullOrEmpty(satellite.country))
                {
                    if (!countryToCatalogs.ContainsKey(satellite.country))
                    {
                        countryToCatalogs[satellite.country] = new HashSet<int>();
                    }
                    countryToCatalogs[satellite.country].Add(satellite.catalogNumber);
                }

                validCount++;
            }
            else
            {
                invalidCount++;
            }
        }

        // 初始化筛选结果为所有有效卫星
        RefreshFilteredData();

        Debug.Log($"预处理完成: 有效 {validCount} 个, 无效 {invalidCount} 个");
    }

    // 解析日期中的年份
    private int ParseYearFromDate(string dateStr)
    {
        if (string.IsNullOrEmpty(dateStr) || dateStr.Length < 4)
        {
            return 1970; // 默认年份
        }

        string yearStr = dateStr.Substring(0, 4);
        if (int.TryParse(yearStr, out int year))
        {
            return year;
        }
        return 1970; // 解析失败时的默认年份
    }
    private int ParseCatalogNumber(string catalogStr)
    {
        if (string.IsNullOrEmpty(catalogStr))
            return 0;

        catalogStr = catalogStr.Trim();

        if (int.TryParse(catalogStr, out int result))
        {
            return result;
        }

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
                digitValue = char.ToUpper(c) - 'A' + 10;
            }
            else
            {
                Debug.LogWarning($"无效的catalog number字符: '{c}' in '{catalogStr}'");
                return 0;
            }

            value = value * 36 + digitValue;
        }

        return value;
    }

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
            string catalogStr = tle2.Substring(2, 5);
            satellite.catalogNumber = ParseCatalogNumber(catalogStr);

            satellite.inclination = ParseFloat(tle2.Substring(8, 8), "inclination");
            satellite.raan = ParseFloat(tle2.Substring(17, 8), "raan");

            string eccentricityStr = tle2.Substring(26, 7).Trim();
            satellite.eccentricity = ParseFloat("0." + eccentricityStr, "eccentricity");

            satellite.argPerigee = ParseFloat(tle2.Substring(34, 8), "argPerigee");
            satellite.meanAnomaly = ParseFloat(tle2.Substring(43, 8), "meanAnomaly");
            satellite.meanMotion = ParseFloat(tle2.Substring(52, 11), "meanMotion");


            //Debug.Log($"成功解析卫星: {satellite.name} (Catalog: {satellite.catalogNumber})");
        }
        catch (Exception e)
        {
            Debug.LogError($"解析TLE失败: {satellite.name} - {e.Message}");
            Debug.LogError($"TLE第二行: {tle2}");
        }
    }

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

    private bool ValidateSatelliteData(SatelliteData satellite)
    {
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

    void ParseSatelliteData()
    {
        int validCount = 0;
        int invalidCount = 0;
        int filteredOutCount = 0;

        // 首先应用筛选条件
        filteredSatellites = ApplyFilters(allSatellites);

        foreach (var satellite in filteredSatellites)
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

        filteredOutCount = allSatellites.Count - filteredSatellites.Count;
        Debug.Log($"卫星数据解析完成: 有效 {validCount} 个, 无效 {invalidCount} 个, 筛选排除 {filteredOutCount} 个");
    }
    private List<SatelliteData> ApplyFilters(List<SatelliteData> satellites)
    {
        List<SatelliteData> filtered = new List<SatelliteData>();

        foreach (var satellite in satellites)
        {
            // 年份筛选
            if (enableYearFilter)
            {
                string yearStr = satellite.stableDate?.Substring(0, 4);

                if (string.IsNullOrEmpty(yearStr) || !int.TryParse(yearStr, out int year))
                {
                    Debug.Log("丢弃解析失败日期:" + satellite.stableDate);
                    continue;
                }
                if (year < filterMinYear || year > filterMaxYear)
                {
                    continue;
                }
            }

            // 国家筛选
            if (enableCountryFilter)
            {
                if (string.IsNullOrEmpty(satellite.country) ||
                    !selectedCountries.Contains(satellite.country))
                {
                    continue;
                }
            }

            filtered.Add(satellite);
        }

        return filtered;
    }

    public void SetYearFilter(bool enabled, int minYear = 1970, int maxYear = 2025)
    {
        bool wasEnabled = enableYearFilter;
        enableYearFilter = enabled;

        if (enabled)
        {
            if (!wasEnabled || minYear != filterMinYear || maxYear != filterMaxYear)
            {
                UpdateYearFilter(minYear, maxYear);
            }
        }
        else if (wasEnabled)
        {
            filterMinYear = minYear;
            filterMaxYear = maxYear;
            RefreshFilteredData();
            RefreshCurrentDisplay();
        }

        Debug.Log($"年份筛选{(enabled ? "已启用" : "已禁用")}: {minYear} - {maxYear}");
    }

    public void SetCountryFilter(bool enabled, string[] countries = null)
    {
        enableCountryFilter = enabled;
        if (countries != null)
        {
            selectedCountries = countries;
        }

        RefreshFilteredData();
        RefreshCurrentDisplay();

        Debug.Log($"国家筛选{(enabled ? "已启用" : "已禁用")}: {string.Join(", ", selectedCountries)}");
    }

    // 快速筛选方法 - 基于预计算的映射
    private void RefreshFilteredData()
    {
        filteredCatalogNumbers.Clear();

        foreach (var kvp in catalogToSatellite)
        {
            int catalogNumber = kvp.Key;
            var satellite = kvp.Value;

            bool passFilter = true;

            // 年份筛选
            if (enableYearFilter)
            {
                int year = catalogToYear[catalogNumber];
                if (year < filterMinYear || year > filterMaxYear)
                {
                    passFilter = false;
                }
            }

            // 国家筛选
            if (passFilter && enableCountryFilter)
            {
                if (string.IsNullOrEmpty(satellite.country) || !selectedCountries.Contains(satellite.country))
                {
                    passFilter = false;
                }
            }

            if (passFilter)
            {
                filteredCatalogNumbers.Add(catalogNumber);
            }
        }

        Debug.Log($"筛选后的卫星数量: {filteredCatalogNumbers.Count}");
    }

    // 增量更新年份筛选 - 支持平滑增减年份
    public void UpdateYearFilter(int minYear, int maxYear)
    {
        int oldMinYear = filterMinYear;
        int oldMaxYear = filterMaxYear;

        filterMinYear = minYear;
        filterMaxYear = maxYear;

        if (!enableYearFilter)
        {
            enableYearFilter = true;
            RefreshFilteredData();
            RefreshCurrentDisplay();
            return;
        }

        // 如果是扩大年份范围，增量添加
        if (minYear <= oldMinYear && maxYear >= oldMaxYear)
        {
            // 添加新的年份范围内的卫星
            foreach (var kvp in catalogToYear)
            {
                int catalogNumber = kvp.Key;
                int year = kvp.Value;

                if (!filteredCatalogNumbers.Contains(catalogNumber) &&
                    year >= minYear && year <= maxYear)
                {
                    var satellite = catalogToSatellite[catalogNumber];

                    // 检查国家筛选
                    bool passCountryFilter = true;
                    if (enableCountryFilter)
                    {
                        if (string.IsNullOrEmpty(satellite.country) || !selectedCountries.Contains(satellite.country))
                        {
                            passCountryFilter = false;
                        }
                    }

                    if (passCountryFilter)
                    {
                        filteredCatalogNumbers.Add(catalogNumber);
                    }
                }
            }
        }
        // 如果是缩小年份范围，移除不符合的卫星
        else if (minYear >= oldMinYear && maxYear <= oldMaxYear)
        {
            tempCatalogList.Clear();
            foreach (int catalogNumber in filteredCatalogNumbers)
            {
                int year = catalogToYear[catalogNumber];
                if (year < minYear || year > maxYear)
                {
                    tempCatalogList.Add(catalogNumber);
                }
            }

            foreach (int catalogNumber in tempCatalogList)
            {
                filteredCatalogNumbers.Remove(catalogNumber);
            }
        }
        else
        {
            // 复杂的范围变化，重新计算
            RefreshFilteredData();
        }

        RefreshCurrentDisplay();
        Debug.Log($"年份筛选更新: {minYear} - {maxYear}, 当前卫星数量: {filteredCatalogNumbers.Count}");
    }

    // 刷新当前显示的轨道
    private void RefreshCurrentDisplay()
    {
        if (!string.IsNullOrEmpty(currentDisplayGroupName))
        {
            SetDisplayGroup(currentDisplayGroupName);
        }
    }

    OrbitElements CalculateOrbitElements(SatelliteData satellite)
    {
        var orbit = new OrbitElements();

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

    // 修改 UpdateSatellitePositions 方法，使用时间偏移
    void UpdateSatellitePositions()
    {
        float currentTime = Time.time;

        foreach (int satNumber in currentDisplayedOrbits)
        {
            if (orbitElements.ContainsKey(satNumber))
            {
                var orbit = orbitElements[satNumber];
                // 使用时间偏移让卫星在轨道上分散
                float offsetTime = currentTime;
                if (satelliteTimeOffsets.ContainsKey(satNumber))
                {
                    offsetTime += satelliteTimeOffsets[satNumber];
                }
                Vector3 position = CalculateSatellitePosition(orbit, offsetTime);
                currentSatellitePositions[satNumber] = position;
            }
        }
    }
    // 添加设置最大轨道数量的公共方法
    public void SetMaxDisplay(int maxOrbits, int maxSatellites)
    {
        maxDisplayOrbits = Mathf.Clamp(maxOrbits, 10, 10000);

        maxDisplaySatellites = Mathf.Clamp(maxSatellites, 10, 30000);

        Debug.Log($"最大显示轨道数量设置为: {maxDisplayOrbits}");
    }

    // 简化的卫星位置计算方法
    Vector3 CalculateSatellitePosition(OrbitElements orbit, float time)
    {
        // 简化的卫星位置计算（实际应用中需要更精确的算法）
        float meanAnomalyNow = orbit.meanAnomaly + orbit.meanMotion * time * 2 * Mathf.PI / 86400f;
        float trueAnomaly = meanAnomalyNow; // 简化，实际需要解开普勒方程

        float r = orbit.semiMajorAxis * (1 - orbit.eccentricity * orbit.eccentricity) /
                 (1 + orbit.eccentricity * Mathf.Cos(trueAnomaly));

        float x = r * Mathf.Cos(trueAnomaly);
        float y = r * Mathf.Sin(trueAnomaly);

        Vector3 orbitalPos = new Vector3(x, y, 0);
        return TransformToECI(orbitalPos, orbit) * orbitScale;
    }

    // SatelliteOrbitRenderer.cs (仅返回修改部分)

    void RenderSatellites()
    {
        if (currentSatellitePositions.Count == 0) return;

        const int batchSize = 1023; // Unity顶点属性数组最大实例数
        var matrices = new Matrix4x4[batchSize];
        var propertyBlock = new MaterialPropertyBlock();

        // 预取摄像机朝向
        Quaternion camRot = Camera.main ? Camera.main.transform.rotation : Quaternion.identity;
        Debug.Log(currentSatellitePositions.Count);
        int i = 0;
        foreach (var kvp in currentSatellitePositions)
        {
            int satNumber = kvp.Key;
            Vector3 position = kvp.Value;

            if (!catalogToSatellite.TryGetValue(satNumber, out var satellite))
                continue;

            // 按国家着色
            Color[] countryColors = GetCountryColors(satellite.country);
            Color satelliteColor = countryColors[satNumber % countryColors.Length];

            matrices[i] = Matrix4x4.TRS(position, camRot, Vector3.one);

            // 设置本批颜色
            if (i == 0)
            {
                propertyBlock.SetColor("_UnlitColor", satelliteColor);
                propertyBlock.SetColor("_EmissiveColor", satelliteColor);
                propertyBlock.SetFloat("_EmissiveIntensity", 2.0f);
            }

            i++;

            // 满一批或最后一批
            if (i == batchSize)
            {
                Graphics.DrawMeshInstanced(satelliteMesh, 0, satelliteMaterial, matrices, batchSize, propertyBlock);
                i = 0;
            }
        }
        // 绘制最后不足一批的卫星
        if (i > 0)
        {
            Graphics.DrawMeshInstanced(satelliteMesh, 0, satelliteMaterial, matrices, i, propertyBlock);
        }
    }

    void CreateOrbitMeshes(List<int> satelliteNumbers)
    {
        // 只创建新的轨道mesh，不销毁现有的（增量更新）
        foreach (int satNumber in satelliteNumbers)
        {
            if (!orbitMeshes.ContainsKey(satNumber) && allOrbitElements.ContainsKey(satNumber))
            {
                var orbit = allOrbitElements[satNumber];
                var mesh = CreateOrbitMesh(orbit);
                orbitMeshes[satNumber] = mesh;
            }
        }

        // 清理不再需要的mesh
        tempCatalogList.Clear();
        foreach (var kvp in orbitMeshes)
        {
            if (!currentDisplayedOrbitsSet.Contains(kvp.Key))
            {
                tempCatalogList.Add(kvp.Key);
            }
        }
    }

    Mesh CreateOrbitMesh(OrbitElements orbit)
    {
        var mesh = new Mesh();
        var vertices = new Vector3[orbitSegments + 1];
        var indices = new int[orbitSegments * 2];

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
        float cosArgP = Mathf.Cos(orbit.argPerigee);
        float sinArgP = Mathf.Sin(orbit.argPerigee);
        float cosInc = Mathf.Cos(orbit.inclination);
        float sinInc = Mathf.Sin(orbit.inclination);
        float cosRaan = Mathf.Cos(orbit.raan);
        float sinRaan = Mathf.Sin(orbit.raan);

        float x1 = orbitalPos.x * cosArgP - orbitalPos.y * sinArgP;
        float y1 = orbitalPos.x * sinArgP + orbitalPos.y * cosArgP;
        float z1 = orbitalPos.z;

        float x2 = x1;
        float y2 = y1 * cosInc - z1 * sinInc;
        float z2 = y1 * sinInc + z1 * cosInc;

        float x3 = x2 * cosRaan - y2 * sinRaan;
        float y3 = x2 * sinRaan + y2 * cosRaan;
        float z3 = z2;

        return new Vector3(x3, z3, y3);
    }

    /// <summary>
    /// 绘制轨道
    /// </summary>
    void RenderOrbitsWithInstancing()
    {
        if (currentDisplayedOrbits.Count == 0) return;

        for (int i = 0; i < currentDisplayedOrbits.Count; i += maxOrbitsPerBatch)
        {
            int batchSize = Mathf.Min(maxOrbitsPerBatch, currentDisplayedOrbits.Count - i);
            var batch = currentDisplayedOrbits.GetRange(i, batchSize);
            RenderOrbitBatch(batch);
        }
    }

    /// <summary>
    /// 批量绘制轨道
    /// </summary>
    /// <param name="batch"></param>
    void RenderOrbitBatch(List<int> batch)
    {
        var matrices = new Matrix4x4[batch.Count];

        for (int i = 0; i < batch.Count; i++)
        {
            matrices[i] = Matrix4x4.identity;
        }

        // 获取当前显示组的颜色组
        Color[] groupColors = GetCountryColors(currentDisplayGroupName);

        for (int i = 0; i < batch.Count; i++)
        {
            int satNumber = batch[i];
            if (orbitMeshes.ContainsKey(satNumber))
            {
                var propertyBlock = new MaterialPropertyBlock();

                if (catalogToSatellite.TryGetValue(satNumber, out var satellite))
                {
                    Color[] countryColors = GetCountryColors(satellite.country);
                    Color orbitColor = countryColors[satNumber % countryColors.Length];

                    propertyBlock.SetColor("_UnlitColor", orbitColor);
                    propertyBlock.SetColor("_EmissiveColor", orbitColor);
                    propertyBlock.SetFloat("_EmissiveIntensity", 1.0f);

                    Graphics.DrawMesh(orbitMeshes[satNumber], matrices[i], orbitMaterial, 0, null, 0, propertyBlock);
                }
            }
        }
    }

    // 获取组颜色的辅助方法
    private Color[] GetCountryColors(string country)
    {
        if (!string.IsNullOrEmpty(country) && countryGroupColorGroups.ContainsKey(country))
            return countryGroupColorGroups[country];

        // 如果没有找到对应的国家颜色组，返回白色数组作为默认颜色
        return new Color[] { Color.white };
    }
    int endy = 1980;
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.A)) SetDisplayGroup("GPS");
        if (Input.GetKeyDown(KeyCode.S)) SetDisplayGroup("格洛纳斯");
        if (Input.GetKeyDown(KeyCode.D)) SetDisplayGroup("星链");
        if (Input.GetKeyDown(KeyCode.F)) SetDisplayGroup("伽利略");
        if (Input.GetKeyDown(KeyCode.G)) SetDisplayGroup("北斗");
        if (Input.GetKeyDown(KeyCode.H)) SetDisplayGroup("千帆");
        if (Input.GetKeyDown(KeyCode.J)) SetDisplayGroup("国网");
        if (Input.GetKeyDown(KeyCode.K)) SetDisplayGroup("一网");
        if (Input.GetKeyDown(KeyCode.UpArrow)) SetDisplayAll(1980, endy += 5);
        if (Input.GetKeyDown(KeyCode.DownArrow)) SetDisplayAll(1980, endy -= 5);


        // 显示模式切换
        if (Input.GetKeyDown(KeyCode.Alpha0)) SetDisplayMode(DisplayMode.None);
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetDisplayMode(DisplayMode.OrbitOnly);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetDisplayMode(DisplayMode.SatelliteOnly);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetDisplayMode(DisplayMode.Both);

        // 新增筛选快捷键
        if (Input.GetKeyDown(KeyCode.Y)) ToggleYearFilter();
        if (Input.GetKeyDown(KeyCode.C)) ToggleCountryFilter();
    }

    // 切换年份筛选
    private void ToggleYearFilter()
    {
        SetYearFilter(!enableYearFilter, filterMinYear, filterMaxYear);
    }

    // 切换国家筛选
    private void ToggleCountryFilter()
    {
        SetCountryFilter(!enableCountryFilter, selectedCountries);
    }
    /// <summary>
    /// 筛选所有符合年限的卫星数据
    /// </summary>
    /// <param name="startYear"></param>
    /// <param name="endYear"></param>
    public void SetDisplayAll(int startYear = 1980, int endYear = 2025)
    {

        currentDisplayGroupName = ""; // 清空分组名，表示非分组显示

        // 更新年份筛选条件（如果传入的区间与当前不同则更新）
        if (filterMinYear != startYear || filterMaxYear != endYear || !enableYearFilter)
        {
            SetYearFilter(true, startYear, endYear);
        }
        else
        {
            // 仅刷新筛选结果
            RefreshFilteredData();
        }

        // 构建当前显示轨道列表，只包含筛选后的卫星，并且轨道数据存在
        currentDisplayedOrbits = filteredCatalogNumbers
            .Where(catalogNum => allOrbitElements.ContainsKey(catalogNum))
            .ToList();

        // 性能限制：卫星数量超出最大值时均匀采样
        if (currentDisplayedOrbits.Count > maxDisplaySatellites)
        {
            List<int> selectedOrbits = new List<int>();
            float step = (float)currentDisplayedOrbits.Count / maxDisplaySatellites;
            for (int i = 0; i < maxDisplaySatellites; i++)
            {
                int index = Mathf.RoundToInt(i * step);
                if (index < currentDisplayedOrbits.Count)
                {
                    selectedOrbits.Add(currentDisplayedOrbits[index]);
                }
            }
            currentDisplayedOrbits = selectedOrbits;
            Debug.Log($"筛选后卫星数超限: 仅显示 {currentDisplayedOrbits.Count}/{filteredCatalogNumbers.Count}");
        }

        // 更新Set以便后续清理Mesh用
        currentDisplayedOrbitsSet = new HashSet<int>(currentDisplayedOrbits);

        // 清空当前卫星位置缓存
        currentSatellitePositions.Clear();

        // 为每个卫星计算时间偏移，让它们在轨道上均匀分布
        satelliteTimeOffsets.Clear();
        int count = currentDisplayedOrbits.Count;
        for (int i = 0; i < count; i++)
        {
            int satNumber = currentDisplayedOrbits[i];
            float timeOffset = (float)i / count * 86400f;
            satelliteTimeOffsets[satNumber] = timeOffset;
        }

        // 创建轨道Mesh
        CreateOrbitMeshes(currentDisplayedOrbits);

        Debug.Log($"显示全部（不分组）: {currentDisplayedOrbits.Count} 条卫星，年份 {startYear}-{endYear}");
    }

    /// <summary>
    /// 根据星座名映射表展示选中卫星编号列表卫星和轨道
    /// </summary>
    /// <param name="groupName">组名 星座卫星群</param>
    public void SetDisplayGroup(string groupName)
    {
        if (tleSelDic.ContainsKey(groupName))
        {
            currentDisplayGroupName = groupName; // 记录当前显示的组名

            currentSatellitePositions.Clear();  //清空卫星点
            List<int> allOrbits = tleSelDic[groupName].sel;

            // 限制轨道数量以优化性能
            if (allOrbits.Count > maxDisplayOrbits)
            {
                // 均匀采样选择轨道，而不是只取前N个
                List<int> selectedOrbits = new List<int>();
                float step = (float)allOrbits.Count / maxDisplayOrbits;

                for (int i = 0; i < maxDisplayOrbits; i++)
                {
                    int index = Mathf.RoundToInt(i * step);
                    if (index < allOrbits.Count)
                    {
                        selectedOrbits.Add(allOrbits[index]);
                    }
                }
                currentDisplayedOrbits = selectedOrbits;
                Debug.Log($"限制显示 {groupName} 卫星群: {currentDisplayedOrbits.Count}/{allOrbits.Count} 个轨道");
            }
            else
            {
                currentDisplayedOrbits = allOrbits;
                Debug.Log($"显示 {groupName} 卫星群: {currentDisplayedOrbits.Count} 个轨道");
            }

            // 为每个卫星计算时间偏移，让它们在轨道上均匀分布
            satelliteTimeOffsets.Clear();
            for (int i = 0; i < currentDisplayedOrbits.Count; i++)
            {
                int satNumber = currentDisplayedOrbits[i];
                // 计算该卫星在轨道上的均匀分布位置
                float timeOffset = (float)i / currentDisplayedOrbits.Count * 86400f; // 在一个轨道周期内均匀分布
                satelliteTimeOffsets[satNumber] = timeOffset;
            }

            CreateOrbitMeshes(currentDisplayedOrbits);
        }
        else
        {
            Debug.LogWarning($"未找到卫星群: {groupName}");
        }
    }


    /// <summary>
    /// 加载国家轨道颜色组字典
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static Dictionary<string, Color[]> LoadCountryGroupsColor(string filePath) =>
        JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(filePath))
        .ToDictionary(
            kv => kv.Key,
            kv => kv.Value
                .Select(hex => ColorUtility.TryParseHtmlString(hex.StartsWith("#") ? hex : "#" + hex, out var c) ? c : Color.black)
                .ToArray()
        );


    /// <summary>
    /// 隐藏-卫星-轨道-both 模式切换开关
    /// <param name="mode"></param>
    public void SetDisplayMode(DisplayMode mode)
    {
        displayMode = mode;
        Debug.Log($"显示模式切换为: {mode}");
    }

    void OnDestroy()
    {
        foreach (var mesh in orbitMeshes.Values)
        {
            if (mesh != null) DestroyImmediate(mesh);
        }
        orbitMeshes.Clear();

        if (satelliteMesh != null) DestroyImmediate(satelliteMesh);
    }

    class TleSel
    {
        public List<int> sel;
        public float pitch;
        public float yaw;
        public float fov;
    }
}