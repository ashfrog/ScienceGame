using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 读取 StreamingAssets/tle.json，按年份（1970-2025）与国家（CN / I-ESA / RU / US / UK）统计卫星数量，
/// 生成以 Tab 分割的 txt 表格（包含标题与表头行）到 StreamingAssets/launch_counts_1970_2025.txt。
/// </summary>
public class SatelliteLaunchCountExporter : MonoBehaviour
{
    // 统计的国家顺序
    private static readonly string[] TargetCountries = { "CN", "I-ESA", "RU", "US", "UK" };

    // 可选的国家别名映射（如源数据里可能把 ESA 标成 "ESA"，这里统一为 "I-ESA"）
    private static readonly Dictionary<string, string> CountryAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "ESA", "I-ESA" },
        { "EU", "I-ESA" },   // 如不需要可移除
        { "EUS", "I-ESA" },  // 如不需要可移除
    };

    private const int StartYear = 1970;
    private const int EndYear = 2025;

#if UNITY_EDITOR
    [MenuItem("Tools/TLE/导出 1970-2025 卫星发射数量表 (CN I-ESA RU US UK)")]
    public static void ExportFromMenu()
    {
        try
        {
            string outPath = Export();
            Debug.Log($"导出成功: {outPath}");
            EditorUtility.RevealInFinder(outPath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"导出失败: {ex.Message}\n{ex.StackTrace}");
        }
    }
#endif

    /// <summary>
    /// 执行导出，返回输出文件完整路径。
    /// </summary>
    public static string Export()
    {
        string streaming = Application.streamingAssetsPath;
        if (string.IsNullOrEmpty(streaming))
        {
            throw new InvalidOperationException("StreamingAssets 路径不可用。请在 Unity 工程内执行该操作。");
        }

        string tlePath = Path.Combine(streaming, "tle.json");
        if (!File.Exists(tlePath))
        {
            throw new FileNotFoundException($"未找到 TLE 数据文件: {tlePath}");
        }

        // 读入数据
        var json = File.ReadAllText(tlePath, Encoding.UTF8);
        var sats = JsonConvert.DeserializeObject<List<SatelliteData>>(json) ?? new List<SatelliteData>();

        // 先尝试从 TLE 第一行解析历元（备用）
        foreach (var s in sats)
        {
            ParseEpochFromTLELine1(s);
        }

        // 初始化统计结构：年 -> 国家 -> 计数
        var counts = new Dictionary<int, Dictionary<string, int>>();
        for (int y = StartYear; y <= EndYear; y++)
        {
            counts[y] = TargetCountries.ToDictionary(k => k, _ => 0);
        }

        // 统计
        foreach (var sat in sats)
        {
            // 剔除不是卫星（碎片 / 火箭体等）的条目
            if (IsDebrisName(sat.name))
                continue;

            // 归一化国家代码
            string country = NormalizeCountry(sat.country);

            // 如果不是需要统计的国家，跳过
            if (!TargetCountries.Contains(country))
                continue;

            // 解析年份（优先 stableDate 的前 4 位；若无则回退 TLE 历元年份）
            int? year = TryParseYearFromStableDate(sat.stableDate);
            if (!year.HasValue && sat.epochParsed)
            {
                year = sat.epochUtc.Year;
            }

            if (!year.HasValue) continue;
            if (year.Value < StartYear || year.Value > EndYear) continue;

            counts[year.Value][country] += 1;
        }

        // 生成表格文本
        var sb = new StringBuilder();
        // 标题行（中文要求）
        sb.AppendLine("CN I-ESA RU US UK 这5个国家 按照年份从1970年到2025年的卫星发射数量表");
        // 表头（Tab 分割）
        sb.AppendLine("Year\t中国\t欧洲\t俄罗斯\t美国\t英国");

        for (int y = StartYear; y <= EndYear; y++)
        {
            var row = counts[y];
            sb.Append(y);
            foreach (var c in TargetCountries)
            {
                sb.Append('\t');
                sb.Append(row[c]);
            }
            sb.AppendLine();
        }

        // 写出文件
        string outFile = Path.Combine(streaming, "launch_counts_1970_2025.txt");
        File.WriteAllText(outFile, sb.ToString(), Encoding.UTF8);
        return outFile;
    }

    private static string NormalizeCountry(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var trimmed = raw.Trim();
        if (CountryAliases.TryGetValue(trimmed, out var mapped))
            return mapped;
        return trimmed;
    }

    private static int? TryParseYearFromStableDate(string stableDate)
    {
        if (string.IsNullOrEmpty(stableDate) || stableDate.Length < 4) return null;
        if (int.TryParse(stableDate.Substring(0, 4), out int y)) return y;
        return null;
    }

    // 从 TLE 第一行解析历元（YYDDD.DDDDDDDD）
    private static void ParseEpochFromTLELine1(SatelliteData sat)
    {
        if (sat == null || string.IsNullOrEmpty(sat.tle1) || sat.tle1.Length < 32)
        {
            if (sat != null) sat.epochParsed = false;
            return;
        }

        try
        {
            string epochStr = sat.tle1.Substring(18, 14).Trim(); // YYDDD.DDDDDDDD
            string yy = epochStr.Substring(0, 2);
            string dddFrac = epochStr.Substring(2); // DDD.DDDDDDDD

            if (int.TryParse(yy, out int yearTwo) && double.TryParse(dddFrac, out double dayOfYear))
            {
                int year = (yearTwo < 57) ? (2000 + yearTwo) : (1900 + yearTwo);
                DateTime start = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                double intDay = Math.Floor(dayOfYear);
                double frac = dayOfYear - intDay;
                DateTime epoch = start.AddDays(intDay - 1).AddSeconds(frac * 86400.0);
                sat.epochUtc = epoch;
                sat.epochParsed = true;
            }
            else
            {
                sat.epochParsed = false;
            }
        }
        catch
        {
            sat.epochParsed = false;
        }
    }

    // 判定是否为碎片 / 非主要卫星（可根据需要继续扩展关键字列表）
    // 说明：
    // - 常见碎片标记包含 "DEB"
    // - 火箭级/火箭体: "R/B", "RB", "RKT", "STAGE"
    // - 其它可能的非主要载荷： "ADAPTER", "DUMMY"
    // - 使用大写比较，防止大小写差异
    private static bool IsDebrisName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return true; // 没名字基本当作无效
        string upper = name.ToUpperInvariant();

        // 可根据实际数据再增减
        string[] debrisKeywords =
        {
            " DEB",     // 碎片
            "DEB ",     // 前缀情况
            "DEB-",     // 组合
            "R/B",      // 火箭体
            " RB",      // 火箭体简写
            "STAGE",    // 级段
            "ADAPTER",  // 适配器
            "DUMMY",    // 假载荷
            "FRAGMENT", // 碎片
            "SL14",     // 俄火箭体常见编码
            "ATLAS",    // 有时标记火箭级
            "ARIANE",   // 火箭级相关
            "CENTAUR",  // 火箭级
            "STARLINK DEBRIS", // 特定集合
        };

        // 精确匹配：若名称以这些典型碎片后缀结尾也去除
        string[] suffixes =
        {
            " DEB",
            " R/B",
            " RB",
        };

        foreach (var kw in debrisKeywords)
        {
            if (upper.Contains(kw))
                return true;
        }

        foreach (var suf in suffixes)
        {
            if (upper.EndsWith(suf))
                return true;
        }

        // 若名称长度极短且不含数字，通常无效
        if (upper.Length <= 3 && !upper.Any(char.IsDigit))
            return true;

        return false;
    }

    // 与项目中相同的结构，确保 JSON 字段能够正确反序列化
    [Serializable]
    private class SatelliteData
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
}