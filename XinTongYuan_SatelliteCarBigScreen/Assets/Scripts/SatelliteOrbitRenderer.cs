using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using File = System.IO.File;
using Input = UnityEngine.Input;

[System.Serializable]
public class SatelliteData
{
    public string tle1;
    public string tle2;
    public string name;
    public int catalogNumber;
    public int cachedYear;

    public float inclination;
    public float raan;
    public float eccentricity;
    public float argPerigee;
    public float meanAnomaly;
    public float meanMotion;

    public string country;
    public string bus;
    public string stableDate;

    public DateTime epochUtc;
    public bool epochParsed;
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
    None,
    OrbitOnly,
    SatelliteOnly,
    Both
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
    public List<string> selectedCountries = new List<string> { "US", "CN", "RU" };

    private List<SatelliteData> filteredSatellites = new List<SatelliteData>();

    [Header("显示模式")]
    public DisplayMode displayMode = DisplayMode.Both;

    [Header("国家颜色设置")]
    public Dictionary<string, Color[]> countryGroupColorGroups;
    private string currentDisplayGroupName = "";

    [Header("卫星视觉设置")]
    public bool keepConstantVisualSize = true;
    public float referenceFOV = 60f;
    public float minScale = 0.001f;
    public float maxScale = 10f;
    public float baseSatelliteScale = 1f;

    private List<SatelliteData> allSatellites = new List<SatelliteData>();
    private Dictionary<int, OrbitElements> allOrbitElements = new Dictionary<int, OrbitElements>();
    private Dictionary<int, SatelliteData> catalogToSatellite = new Dictionary<int, SatelliteData>();

    private HashSet<int> filteredCatalogNumbers = new HashSet<int>();
    private Dictionary<int, int> catalogToYear = new Dictionary<int, int>();
    private Dictionary<string, HashSet<int>> countryToCatalogs = new Dictionary<string, HashSet<int>>();

    private Dictionary<int, OrbitElements> orbitElements = new Dictionary<int, OrbitElements>();
    private Dictionary<int, Mesh> orbitMeshes = new Dictionary<int, Mesh>();
    private Dictionary<int, Vector3> currentSatellitePositions = new Dictionary<int, Vector3>();
    private Mesh satelliteMesh;

    private List<int> currentDisplayedOrbits = new List<int>();
    private HashSet<int> currentDisplayedOrbitsSet = new HashSet<int>();
    Dictionary<string, TleSel> tleSelDic;

    [Header("性能优化")]
    public int maxDisplayOrbits = 200;
    public int maxDisplaySatellites = 2000;

    private List<int> tempCatalogList = new List<int>(5000);
    private List<int> tempDisplayList = new List<int>(1000);

    [SerializeField] bool printSelectcatalogNumber = false;
    [SerializeField] private Camera objCamera;

    float orbitAlpha = 0.1f;
    [Header("同时显示卫星和轨道时卫星放大系数")]
    public float scaleSateliteWhenShowBoth = 1.5f;

    [Header("分组颜色设置")]
    public Dictionary<string, Color[]> groupColorGroups;
    private Color[] currentGroupColors = null;

    [Header("实时传播设置")]
    public bool useSystemTime = true;
    public float timeScale = 1f;
    public bool useSgp4Propagation = true;
    private double accumulatedSimSeconds = 0;

    // OPT: 改进连续时间
    private DateTime lastRealUtc;
    private double simulationElapsedMinutes = 0;
    private double simulationElapsedMinutesSim = 0;
    private Dictionary<int, double> initialEpochOffsets = new Dictionary<int, double>();
    private bool timeAnchored = false;

    [Header("时间倍率控制（运行时按键：[ 减速, ] 加速）")]
    public bool allowKeyboardTimeControls = true;
    public KeyCode slowerKey = KeyCode.LeftBracket;
    public KeyCode fasterKey = KeyCode.RightBracket;
    public float timeScaleStep = 2f;
    float minTimeScale = 1f;
    float maxTimeScale = 2048f;

    private Dictionary<int, Propagator> propagators = new Dictionary<int, Propagator>();

    void Start()
    {
        SetMaxDisplay(Settings.ini.Game.MaxDisplayOrbits, Settings.ini.Game.MaxDisplaySatellite);
        scaleSateliteWhenShowBoth = Settings.ini.Game.ScaleSateliteWhenShowBoth;
        orbitAlpha = Settings.ini.Game.OrbitAlpha;
        maxOrbitsPerBatch = Settings.ini.Game.MaxOrbitsPerBatch;

        InitializeMaterials();
        CreateSatelliteMesh();
        LoadSatelliteData();
        LoadCountryColorGroups();
        LoadGroupColorGroups();
        ParseSatelliteData();

        if (printSelectcatalogNumber)
        {
            string selectsatelite = "";
            foreach (var satellite in allSatellites)
                selectsatelite += satellite.catalogNumber + ",";
            Debug.Log(selectsatelite);
        }

        PreprocessSatelliteData();
        BuildPropagators();
        AnchorTimeInitialization(); // 初次锚定
        LoadSelectionGroups();

        timeScale = Settings.ini.Game.TimeScale;
    }

    void LoadGroupColorGroups()
    {
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "groupcolor.json");
            groupColorGroups = LoadCountryGroupsColor(filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"加载分组颜色组失败: {ex.Message}");
            groupColorGroups = new Dictionary<string, Color[]>();
        }
    }

    void Update()
    {
        if (displayMode == DisplayMode.None) return;

        HandleTimeControls();

        UpdateSatellitePositions();

        if (displayMode == DisplayMode.OrbitOnly || displayMode == DisplayMode.Both)
            RenderOrbitsWithInstancing();

        if (displayMode == DisplayMode.SatelliteOnly || displayMode == DisplayMode.Both)
            RenderSatellites();

        referenceFOV = objCamera != null ? objCamera.fieldOfView : referenceFOV;
    }

    void BuildPropagators()
    {
        propagators.Clear();
        foreach (var sat in catalogToSatellite.Values)
        {
            if (!sat.epochParsed) continue;
            if (!orbitElements.ContainsKey(sat.catalogNumber)) continue;
            propagators[sat.catalogNumber] = new Propagator(sat, orbitElements[sat.catalogNumber]);
        }
        Debug.Log($"传播器构建完成: {propagators.Count} 个");
    }

    void AnchorTimeInitialization()
    {
        initialEpochOffsets.Clear();
        foreach (var kv in catalogToSatellite)
        {
            var sat = kv.Value;
            if (!sat.epochParsed) continue;
            initialEpochOffsets[sat.catalogNumber] = (DateTime.UtcNow - sat.epochUtc).TotalMinutes;
        }
        lastRealUtc = DateTime.UtcNow;
        simulationElapsedMinutes = 0;
        simulationElapsedMinutesSim = 0;
        timeAnchored = true;
    }

    public void ResetSimulationTimeAnchor()
    {
        simulationElapsedMinutes = 0;
        simulationElapsedMinutesSim = 0;
        lastRealUtc = DateTime.UtcNow;
    }

    public void ResetSystemTimeAnchor()
    {
        lastRealUtc = DateTime.UtcNow;
    }

    void HandleTimeControls()
    {
        if (!allowKeyboardTimeControls) return;
        if (Input.GetKeyDown(slowerKey)) SpeedDown();
        if (Input.GetKeyDown(fasterKey)) SpeedUp();
    }

    public void SpeedUp() => SetTimeScale(timeScale * timeScaleStep);
    public void SpeedDown() => SetTimeScale(timeScale / timeScaleStep);

    public void SetTimeScale(float scale)
    {
        timeScale = Mathf.Clamp(scale, minTimeScale, maxTimeScale);
        Settings.ini.Game.TimeScale = timeScale;
        lastRealUtc = DateTime.UtcNow; // 避免首帧大步
    }

    public string[] GetAvailableCountries()
    {
        HashSet<string> countries = new HashSet<string>();
        foreach (var s in allSatellites)
            if (!string.IsNullOrEmpty(s.country)) countries.Add(s.country);
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
        int latitudeSegments = 12;
        int longitudeSegments = 18;
        float radius = satelliteSize;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

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
                vertices.Add(new Vector3(
                    radius * sinTheta * cosPhi,
                    radius * cosTheta,
                    radius * sinTheta * sinPhi));
            }
        }

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

    public enum SelectName { QIANFAN, BEIDOU, ONEWEB, HULIANGWANG, CZ }
    [SerializeField] SelectName selectName;

    void LoadSatelliteData()
    {
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "tle.json");
            string jsonData = File.ReadAllText(filePath);
            allSatellites = JsonConvert.DeserializeObject<List<SatelliteData>>(jsonData);

            int beforeCount = allSatellites.Count;
            allSatellites = allSatellites.Where(s => !IsDebrisName(s.name)).ToList();
            int removed = beforeCount - allSatellites.Count;
            if (removed > 0)
                Debug.Log($"剔除非主要对象 {removed} 个 (原 {beforeCount} -> {allSatellites.Count})");

            if (printSelectcatalogNumber)
            {
                if (!String.IsNullOrEmpty(selectName.ToString()))
                {
                    allSatellites = allSatellites.Where(s => s.name != null && s.name.StartsWith(selectName.ToString())).ToList();
                    Debug.Log($"筛选 '{selectName}' 后数量: {allSatellites.Count}");
                }
            }

            foreach (var sat in allSatellites)
                ParseEpochFromTLELine1(sat);
        }
        catch (Exception e)
        {
            Debug.LogError($"加载卫星数据失败: {e.Message}");
        }
    }

    private bool IsDebrisName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return true;
        string upper = name.ToUpperInvariant();
        string[] debrisKeywords = {
            " DEB","DEB ","DEB-","R/B"," RB","STAGE","ADAPTER","DUMMY",
            "FRAGMENT","SL14","ATLAS","ARIANE","CENTAUR","STARLINK DEBRIS"
        };
        string[] suffixes = { " DEB", " R/B", " RB" };
        foreach (var kw in debrisKeywords) if (upper.Contains(kw)) return true;
        foreach (var suf in suffixes) if (upper.EndsWith(suf)) return true;
        if (upper.Length <= 3 && !upper.Any(char.IsDigit)) return true;
        return false;
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
            string file = Path.Combine(Application.streamingAssetsPath, "tlesel.json");
            tleSelDic = JsonConvert.DeserializeObject<Dictionary<string, TleSel>>(File.ReadAllText(file));
        }
        catch (Exception e)
        {
            Debug.LogError($"加载选择组失败: {e.Message}");
        }
    }

    void PreprocessSatelliteData()
    {
        int validCount = 0, invalidCount = 0;
        catalogToSatellite.Clear();
        catalogToYear.Clear();
        countryToCatalogs.Clear();
        allOrbitElements.Clear();

        foreach (var satellite in allSatellites)
        {
            if (string.IsNullOrEmpty(satellite.tle2)) { invalidCount++; continue; }
            ParseTLE2(satellite);
            if (ValidateSatelliteData(satellite))
            {
                allOrbitElements[satellite.catalogNumber] = CalculateOrbitElements(satellite);
                catalogToSatellite[satellite.catalogNumber] = satellite;
                int year = ParseYearFromDate(satellite.stableDate);
                catalogToYear[satellite.catalogNumber] = year;
                satellite.cachedYear = year;
                if (!string.IsNullOrEmpty(satellite.country))
                {
                    if (!countryToCatalogs.ContainsKey(satellite.country))
                        countryToCatalogs[satellite.country] = new HashSet<int>();
                    countryToCatalogs[satellite.country].Add(satellite.catalogNumber);
                }
                validCount++;
            }
            else invalidCount++;
        }

        RefreshFilteredData();
        Debug.Log($"预处理完成: 有效 {validCount} 无效 {invalidCount}");
    }

    private int ParseYearFromDate(string dateStr)
    {
        if (string.IsNullOrEmpty(dateStr) || dateStr.Length < 4) return 1970;
        return int.TryParse(dateStr.Substring(0, 4), out int y) ? y : 1970;
    }

    private int ParseCatalogNumber(string catalogStr)
    {
        if (string.IsNullOrEmpty(catalogStr)) return 0;
        catalogStr = catalogStr.Trim();
        if (int.TryParse(catalogStr, out int result)) return result;
        int value = 0;
        foreach (char c in catalogStr)
        {
            int digitValue;
            if (char.IsDigit(c)) digitValue = c - '0';
            else if (char.IsLetter(c)) digitValue = char.ToUpper(c) - 'A' + 10;
            else return 0;
            value = value * 36 + digitValue;
        }
        return value;
    }

    void ParseEpochFromTLELine1(SatelliteData sat)
    {
        if (string.IsNullOrEmpty(sat.tle1) || sat.tle1.Length < 32) { sat.epochParsed = false; return; }
        try
        {
            string epochStr = sat.tle1.Substring(18, 14).Trim();
            string yy = epochStr.Substring(0, 2);
            string dddFrac = epochStr.Substring(2);
            if (int.TryParse(yy, out int yearTwo) && double.TryParse(dddFrac, out double dayOfYear))
            {
                int year = (yearTwo < 57) ? 2000 + yearTwo : 1900 + yearTwo;
                DateTime start = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                double intDay = Math.Floor(dayOfYear);
                double frac = dayOfYear - intDay;
                sat.epochUtc = start.AddDays(intDay - 1).AddSeconds(frac * 86400.0);
                sat.epochParsed = true;
            }
            else sat.epochParsed = false;
        }
        catch { sat.epochParsed = false; }
    }

    void ParseTLE2(SatelliteData satellite)
    {
        string tle2 = satellite.tle2;
        if (string.IsNullOrEmpty(tle2) || tle2.Length < 69)
        {
            Debug.LogWarning($"TLE第二行长度不足: {satellite.name} 长度:{tle2?.Length}");
            return;
        }
        try
        {
            satellite.catalogNumber = ParseCatalogNumber(tle2.Substring(2, 5));
            satellite.inclination = ParseFloat(tle2.Substring(8, 8));
            satellite.raan = ParseFloat(tle2.Substring(17, 8));
            satellite.eccentricity = ParseFloat("0." + tle2.Substring(26, 7).Trim());
            satellite.argPerigee = ParseFloat(tle2.Substring(34, 8));
            satellite.meanAnomaly = ParseFloat(tle2.Substring(43, 8));
            satellite.meanMotion = ParseFloat(tle2.Substring(52, 11));
        }
        catch (Exception e)
        {
            Debug.LogError($"解析TLE失败: {satellite.name} - {e.Message}");
        }
    }

    private float ParseFloat(string valueStr)
    {
        valueStr = valueStr.Trim();
        return float.TryParse(valueStr, out float r) ? r : 0f;
    }

    private bool ValidateSatelliteData(SatelliteData s)
    {
        if (s.catalogNumber <= 0) return false;
        if (s.inclination < 0 || s.inclination > 180) return false;
        if (s.eccentricity < 0 || s.eccentricity >= 1) return false;
        if (s.meanMotion <= 0) return false;
        return true;
    }

    void ParseSatelliteData()
    {
        filteredSatellites = ApplyFilters(allSatellites);
        foreach (var s in filteredSatellites)
        {
            if (string.IsNullOrEmpty(s.tle2)) continue;
            ParseTLE2(s);
            if (ValidateSatelliteData(s))
                orbitElements[s.catalogNumber] = CalculateOrbitElements(s);
        }
    }

    private List<SatelliteData> ApplyFilters(List<SatelliteData> sats)
    {
        List<SatelliteData> filtered = new List<SatelliteData>();
        foreach (var s in sats)
        {
            if (enableYearFilter)
            {
                string yearStr = s.stableDate?.Substring(0, 4);
                if (string.IsNullOrEmpty(yearStr)) continue;
                if (!int.TryParse(yearStr, out int year)) continue;
                if (year < filterMinYear || year > filterMaxYear) continue;
            }
            if (enableCountryFilter)
            {
                if (string.IsNullOrEmpty(s.country) || !selectedCountries.Contains(s.country))
                    continue;
            }
            filtered.Add(s);
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
                UpdateYearFilter(minYear, maxYear);
        }
        else if (wasEnabled)
        {
            filterMinYear = minYear;
            filterMaxYear = maxYear;
            RefreshFilteredData();
            RefreshCurrentDisplay();
        }
    }

    public void SetCountryFilter(bool enabled, List<string> countries = null)
    {
        enableCountryFilter = enabled;
        if (countries != null) selectedCountries = countries;
        RefreshFilteredData();
        RefreshCurrentDisplay();
    }

    public void RefreshData()
    {
        SetDisplayAll(filterMinYear, filterMaxYear, string.Join(",", selectedCountries));
    }

    public void SetCountryFilter(bool enabled, string country = "")
    {
        SetCountryFilter(enabled, country.Split(',').ToList());
    }

    private void RefreshFilteredData()
    {
        filteredCatalogNumbers.Clear();
        foreach (var kvp in catalogToSatellite)
        {
            int catalogNumber = kvp.Key;
            var s = kvp.Value;
            bool pass = true;

            if (enableYearFilter)
            {
                int year = catalogToYear[catalogNumber];
                if (year < filterMinYear || year > filterMaxYear) pass = false;
            }

            if (pass && enableCountryFilter)
            {
                if (string.IsNullOrEmpty(s.country) || !selectedCountries.Contains(s.country))
                    pass = false;
            }

            if (pass) filteredCatalogNumbers.Add(catalogNumber);
        }
    }

    public void UpdateYearFilter(int minYear, int maxYear)
    {
        filterMinYear = minYear;
        filterMaxYear = maxYear;
        if (!enableYearFilter)
        {
            enableYearFilter = true;
            RefreshFilteredData();
            RefreshCurrentDisplay();
            return;
        }
        RefreshFilteredData();
        RefreshCurrentDisplay();
    }

    private void RefreshCurrentDisplay()
    {
        if (!string.IsNullOrEmpty(currentDisplayGroupName))
            SetDisplayGroup(currentDisplayGroupName, displayMode);
    }

    OrbitElements CalculateOrbitElements(SatelliteData s)
    {
        var orbit = new OrbitElements();
        double mu = 3.986004418e14;
        double n = s.meanMotion * 2 * Math.PI / 86400.0;
        orbit.semiMajorAxis = (float)Math.Pow(mu / (n * n), 1.0 / 3.0);
        orbit.eccentricity = s.eccentricity;
        orbit.inclination = s.inclination * Mathf.Deg2Rad;
        orbit.raan = s.raan * Mathf.Deg2Rad;
        orbit.argPerigee = s.argPerigee * Mathf.Deg2Rad;
        orbit.meanAnomaly = s.meanAnomaly * Mathf.Deg2Rad;
        orbit.meanMotion = s.meanMotion;
        return orbit;
    }

    // FIX: 移除频繁锚定导致闪烁；即时更新位置
    void UpdateSatellitePositions()
    {
        if (!timeAnchored) AnchorTimeInitialization();

        if (useSystemTime)
        {
            DateTime now = DateTime.UtcNow;
            double realDeltaSeconds = (now - lastRealUtc).TotalSeconds;
            realDeltaSeconds = Math.Min(realDeltaSeconds, 0.25);
            lastRealUtc = now;
            simulationElapsedMinutes += (realDeltaSeconds * timeScale) / 60.0;
        }
        else
        {
            accumulatedSimSeconds += Time.deltaTime * timeScale;
            simulationElapsedMinutesSim += (Time.deltaTime * timeScale) / 60.0;
        }

        if (currentDisplayedOrbits.Count == 0) return;

        // 不再清空后等待下一帧
        currentSatellitePositions.Clear();

        foreach (int satNumber in currentDisplayedOrbits)
        {
            if (!catalogToSatellite.TryGetValue(satNumber, out var sat)) continue;
            if (!orbitElements.ContainsKey(satNumber)) continue;
            if (!sat.epochParsed) continue;
            if (!propagators.ContainsKey(satNumber)) continue;
            if (!initialEpochOffsets.ContainsKey(satNumber)) continue;

            double baseMinutesSinceEpoch = initialEpochOffsets[satNumber];
            double minutesSinceEpoch = useSystemTime
                ? baseMinutesSinceEpoch + simulationElapsedMinutes
                : simulationElapsedMinutesSim;

            Vector3 eci = useSgp4Propagation
                ? propagators[satNumber].PropagateSgp4(minutesSinceEpoch)
                : propagators[satNumber].PropagateKepler(minutesSinceEpoch);

            currentSatellitePositions[satNumber] = eci * orbitScale;
        }
    }

    // FIX: 提供立即刷新方法（避免调用者在同一帧看到空白）
    private void ForceImmediatePositionUpdate()
    {
        UpdateSatellitePositions();
    }

    public void SetMaxDisplay(int maxOrbits, int maxSatellites)
    {
        maxDisplayOrbits = Mathf.Clamp(maxOrbits, 10, 10000);
        maxDisplaySatellites = Mathf.Clamp(maxSatellites, 10, 30000);
    }

    void RenderSatellites()
    {
        if (currentSatellitePositions.Count == 0) return;
        Camera cam = objCamera != null ? objCamera : Camera.main;
        if (cam == null) return;

        Vector3 camPos = cam.transform.position;
        Quaternion camRot = cam.transform.rotation;
        float currentFOV = cam.fieldOfView;

        var colorGroups = new Dictionary<Color, List<Matrix4x4>>();
        foreach (var kvp in currentSatellitePositions)
        {
            int satNumber = kvp.Key;
            Vector3 position = kvp.Value;
            if (!catalogToSatellite.TryGetValue(satNumber, out var sat)) continue;

            Vector3 scale = CalculateScale(camPos, position, currentFOV);
            Matrix4x4 matrix = Matrix4x4.TRS(position, camRot, scale);

            Color[] colors = GetCurrentColors(sat.country);
            Color c = colors[satNumber % colors.Length];

            if (!colorGroups.ContainsKey(c))
                colorGroups[c] = new List<Matrix4x4>();
            colorGroups[c].Add(matrix);
        }

        const int batchSize = 1023;
        var propertyBlock = new MaterialPropertyBlock();

        foreach (var group in colorGroups)
        {
            var matrices = group.Value;
            Color c = group.Key;
            propertyBlock.SetColor("_UnlitColor", c);
            propertyBlock.SetColor("_EmissiveColor", c);
            propertyBlock.SetFloat("_EmissiveIntensity", 1.0f);

            for (int i = 0; i < matrices.Count; i += batchSize)
            {
                int count = Mathf.Min(batchSize, matrices.Count - i);
                Matrix4x4[] batch = new Matrix4x4[count];
                matrices.CopyTo(i, batch, 0, count);
                Graphics.DrawMeshInstanced(satelliteMesh, 0, satelliteMaterial, batch, count, propertyBlock);
            }
        }
    }

    [Header("近距离视觉缩放补偿")]
    public float satelliteNearAdjustBias = 1.5f;
    public float satelliteNearAdjustPower = 1.30f;

    private Vector3 CalculateScale(Vector3 camPos, Vector3 position, float currentFOV)
    {
        float pixelDiameter = 12f;
        Camera cam = objCamera != null ? objCamera : Camera.main;
        float distance = Vector3.Distance(camPos, position);
        float screenHeight = Screen.height;
        float fovRad = cam.fieldOfView * Mathf.Deg2Rad;

        float worldDiameter = 2f * distance * Mathf.Tan(0.5f * fovRad) * (pixelDiameter / screenHeight);
        float meshDiameter = satelliteSize * 2f;
        float scale = worldDiameter / meshDiameter;

        float nearComp = Mathf.Pow(distance / (distance + satelliteNearAdjustBias), satelliteNearAdjustPower);
        scale *= nearComp;
        scale = Mathf.Clamp(scale, minScale, maxScale);
        if (displayMode == DisplayMode.Both) scale *= scaleSateliteWhenShowBoth;

        return Vector3.one * baseSatelliteScale * scale;
    }

    public void SetConstantVisualSize(bool enabled, float refFOV = 60f)
    {
        keepConstantVisualSize = enabled;
        referenceFOV = refFOV;
    }

    public void SetBaseSatelliteScale(float scale)
    {
        baseSatelliteScale = Mathf.Clamp(scale, 0.1f, 5f);
    }

    void CreateOrbitMeshes(List<int> satelliteNumbers)
    {
        foreach (int satNumber in satelliteNumbers)
        {
            if (!orbitMeshes.ContainsKey(satNumber) && allOrbitElements.ContainsKey(satNumber))
            {
                var orbit = allOrbitElements[satNumber];
                orbitMeshes[satNumber] = CreateOrbitMesh(orbit);
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

    void RenderOrbitsWithInstancing()
    {
        if (currentDisplayedOrbits.Count == 0) return;
        for (int i = 0; i < currentDisplayedOrbits.Count; i += maxOrbitsPerBatch)
        {
            int batchSize = Mathf.Min(maxOrbitsPerBatch, currentDisplayedOrbits.Count - i);
            RenderOrbitBatch(currentDisplayedOrbits.GetRange(i, batchSize));
        }
    }

    void RenderOrbitBatch(List<int> batch)
    {
        var matrices = new Matrix4x4[batch.Count];
        for (int i = 0; i < batch.Count; i++) matrices[i] = Matrix4x4.identity;

        for (int i = 0; i < batch.Count; i++)
        {
            int satNumber = batch[i];
            if (!orbitMeshes.ContainsKey(satNumber)) continue;
            var propertyBlock = new MaterialPropertyBlock();

            if (catalogToSatellite.TryGetValue(satNumber, out var sat))
            {
                Color[] countryColors = GetCurrentColors(sat.country);
                Color orbitColor = countryColors[satNumber % countryColors.Length];
                orbitColor.a = orbitAlpha;
                propertyBlock.SetColor("_UnlitColor", orbitColor);
                propertyBlock.SetColor("_EmissiveColor", orbitColor);
                propertyBlock.SetFloat("_EmissiveIntensity", 1.0f);
                Graphics.DrawMesh(orbitMeshes[satNumber], matrices[i], orbitMaterial, 0, null, 0, propertyBlock);
            }
        }
    }

    private Color[] GetCountryColors(string country)
    {
        if (!string.IsNullOrEmpty(country) && countryGroupColorGroups.ContainsKey(country))
            return countryGroupColorGroups[country];
        return new Color[] { Color.white };
    }

    // FIX: 去除 AnchorTimeInitialization 与 Clear 导致的闪烁；增加即时刷新
    public void SetDisplayAll(int startYear = 1980, int endYear = 2025, string country = "", bool allowAnchorReset = false)
    {
        if (displayMode == DisplayMode.None)
            displayMode = DisplayMode.SatelliteOnly;

        currentDisplayGroupName = "";
        SetYearFilter(true, startYear, endYear);

        if (!string.IsNullOrEmpty(country))
            SetCountryFilter(true, country.Split(',').ToList());
        else
            SetCountryFilter(false, "");

        currentDisplayedOrbits = filteredCatalogNumbers
            .Where(catalogNum => allOrbitElements.ContainsKey(catalogNum))
            .ToList();

        if (currentDisplayedOrbits.Count > maxDisplaySatellites)
        {
            List<int> selected = new List<int>();
            float step = (float)currentDisplayedOrbits.Count / maxDisplaySatellites;
            for (int i = 0; i < maxDisplaySatellites; i++)
            {
                int index = Mathf.RoundToInt(i * step);
                if (index < currentDisplayedOrbits.Count) selected.Add(currentDisplayedOrbits[index]);
            }
            currentDisplayedOrbits = selected;
        }

        currentDisplayedOrbitsSet = new HashSet<int>(currentDisplayedOrbits);

        CreateOrbitMeshes(currentDisplayedOrbits);

        // 不清空位置，直接更新
        if (allowAnchorReset)
        {
            // 可选：只有在确实重载大量数据时重锚
            AnchorTimeInitialization();
        }

        ForceImmediatePositionUpdate(); // 关键：避免出现空帧
    }

    public void SetDisplayGroup(string groupName, DisplayMode mode = DisplayMode.Both)
    {
        if (tleSelDic == null || !tleSelDic.ContainsKey(groupName))
        {
            Debug.LogWarning($"未找到卫星群: {groupName}");
            return;
        }

        currentDisplayGroupName = groupName;
        SetDisplayMode(mode);

        List<int> allOrbits = tleSelDic[groupName].sel;
        currentGroupColors = (groupColorGroups != null && groupColorGroups.ContainsKey(groupName))
            ? groupColorGroups[groupName]
            : null;

        if (allOrbits.Count > maxDisplayOrbits)
        {
            List<int> selected = new List<int>();
            float step = (float)allOrbits.Count / maxDisplayOrbits;
            for (int i = 0; i < maxDisplayOrbits; i++)
            {
                int idx = Mathf.RoundToInt(i * step);
                if (idx < allOrbits.Count) selected.Add(allOrbits[idx]);
            }
            currentDisplayedOrbits = selected;
        }
        else
        {
            currentDisplayedOrbits = allOrbits;
        }

        currentDisplayedOrbitsSet = new HashSet<int>(currentDisplayedOrbits);
        CreateOrbitMeshes(currentDisplayedOrbits);

        // 不重锚时间，保持连续
        ForceImmediatePositionUpdate();
    }

    private Color[] GetCurrentColors(string country)
    {
        if (!string.IsNullOrEmpty(currentDisplayGroupName) && currentGroupColors != null)
            return currentGroupColors;
        if (!string.IsNullOrEmpty(country) && countryGroupColorGroups.ContainsKey(country))
            return countryGroupColorGroups[country];
        return new Color[] { Color.white };
    }

    public static Dictionary<string, Color[]> LoadCountryGroupsColor(string filePath) =>
        JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(filePath))
        .ToDictionary(
            kv => kv.Key,
            kv => kv.Value
                .Select(hex => ColorUtility.TryParseHtmlString(hex.StartsWith("#") ? hex : "#" + hex, out var c) ? c : Color.black)
                .ToArray()
        );

    public void SetDisplayMode(DisplayMode mode) => displayMode = mode;

    void OnDestroy()
    {
        foreach (var mesh in orbitMeshes.Values)
            if (mesh != null) DestroyImmediate(mesh);
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

    private class Propagator
    {
        private SatelliteData sat;
        private OrbitElements orbit;

        public Propagator(SatelliteData s, OrbitElements o)
        {
            sat = s;
            orbit = o;
        }

        public Vector3 PropagateKepler(double minutesSinceEpoch)
        {
            double M0 = orbit.meanAnomaly;
            double n = sat.meanMotion * 2.0 * Math.PI / 86400.0;
            double t = minutesSinceEpoch * 60.0;
            double M = NormalizeRadians(M0 + n * t);

            double E = SolveKepler(M, orbit.eccentricity);
            double cosE = Math.Cos(E);
            double sinE = Math.Sin(E);
            double sqrtOneMinusESq = Math.Sqrt(1 - orbit.eccentricity * orbit.eccentricity);
            double nu = Math.Atan2(sqrtOneMinusESq * sinE, cosE - orbit.eccentricity);

            double a = orbit.semiMajorAxis;
            double r = a * (1 - orbit.eccentricity * cosE);

            double xOrb = r * Math.Cos(nu);
            double yOrb = r * Math.Sin(nu);
            Vector3 perifocal = new Vector3((float)xOrb, (float)yOrb, 0f);
            return TransformPerifocalToECI(perifocal, orbit);
        }

        public Vector3 PropagateSgp4(double minutesSinceEpoch) => PropagateKepler(minutesSinceEpoch);

        private double NormalizeRadians(double ang)
        {
            ang %= (2.0 * Math.PI);
            if (ang < 0) ang += 2.0 * Math.PI;
            return ang;
        }

        private double SolveKepler(double M, double e)
        {
            double E = M;
            for (int i = 0; i < 15; i++)
            {
                double f = E - e * Math.Sin(E) - M;
                double fp = 1 - e * Math.Cos(E);
                double d = f / fp;
                E -= d;
                if (Math.Abs(d) < 1e-10) break;
            }
            return E;
        }

        private Vector3 TransformPerifocalToECI(Vector3 perifocal, OrbitElements o)
        {
            float cosArgP = Mathf.Cos(o.argPerigee);
            float sinArgP = Mathf.Sin(o.argPerigee);
            float cosInc = Mathf.Cos(o.inclination);
            float sinInc = Mathf.Sin(o.inclination);
            float cosRaan = Mathf.Cos(o.raan);
            float sinRaan = Mathf.Sin(o.raan);

            float x1 = perifocal.x * cosArgP - perifocal.y * sinArgP;
            float y1 = perifocal.x * sinArgP + perifocal.y * cosArgP;
            float z1 = perifocal.z;

            float x2 = x1;
            float y2 = y1 * cosInc - z1 * sinInc;
            float z2 = y1 * sinInc + z1 * cosInc;

            float x3 = x2 * cosRaan - y2 * sinRaan;
            float y3 = x2 * sinRaan + y2 * cosRaan;
            float z3 = z2;

            return new Vector3(x3, z3, y3);
        }
    }
}