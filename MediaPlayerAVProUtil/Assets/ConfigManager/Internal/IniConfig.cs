using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public class IniConfig
{
    private string iniFilePath;
    private Dictionary<string, Dictionary<string, string>> cachedConfig;
    private bool isDirty = false;

    public IniConfig(string fileName)
    {
        cachedConfig = new Dictionary<string, Dictionary<string, string>>();

        // Android使用持久化数据路径，Windows使用StreamingAssets
#if UNITY_ANDROID && !UNITY_EDITOR
        string directory = Application.persistentDataPath;
#else
        string directory = Application.streamingAssetsPath;
#endif

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        iniFilePath = Path.Combine(directory, fileName);

        // 如果文件不存在，创建文件
        if (!File.Exists(iniFilePath))
        {
            File.Create(iniFilePath).Dispose();
        }

        // 初始加载配置
        LoadConfig();
    }

    private void LoadConfig()
    {
        try
        {
            string[] lines = File.ReadAllLines(iniFilePath);
            string currentSection = "";

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";")) continue;

                // 检查是否是节名
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    if (!cachedConfig.ContainsKey(currentSection))
                    {
                        cachedConfig[currentSection] = new Dictionary<string, string>();
                    }
                }
                // 检查是否是键值对
                else
                {
                    int equalPos = trimmedLine.IndexOf('=');
                    if (equalPos > 0)
                    {
                        string key = trimmedLine.Substring(0, equalPos).Trim();
                        string value = trimmedLine.Substring(equalPos + 1).Trim();
                        if (!string.IsNullOrEmpty(currentSection))
                        {
                            cachedConfig[currentSection][key] = value;
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load INI file: {e.Message}");
        }
    }

    private void SaveConfig()
    {
        if (!isDirty) return;

        try
        {
            using (StreamWriter writer = new StreamWriter(iniFilePath, false, Encoding.UTF8))
            {
                foreach (var section in cachedConfig)
                {
                    writer.WriteLine($"[{section.Key}]");
                    foreach (var kvp in section.Value)
                    {
                        writer.WriteLine($"{kvp.Key}={kvp.Value}");
                    }
                    writer.WriteLine();
                }
            }
            isDirty = false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save INI file: {e.Message}");
        }
    }

    public string ReadValue(string section, string key, string defaultValue = "")
    {
        // 其他平台使用文件读写
        if (cachedConfig.ContainsKey(section) && cachedConfig[section].ContainsKey(key))
        {
            return cachedConfig[section][key];
        }
        return defaultValue;
    }

    public bool WriteValue(string section, string key, string value)
    {
        try
        {
            // 跨平台使用文件读写
            if (!cachedConfig.ContainsKey(section))
            {
                cachedConfig[section] = new Dictionary<string, string>();
            }
            cachedConfig[section][key] = value;
            isDirty = true;
            SaveConfig();
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to write value: {e.Message}");
            return false;
        }
    }

    public int ReadInt(string section, string key, int defaultValue = 0)
    {
        string value = ReadValue(section, key, defaultValue.ToString());
        return int.TryParse(value, out int result) ? result : defaultValue;
    }

    public float ReadFloat(string section, string key, float defaultValue = 0f)
    {
        string value = ReadValue(section, key, defaultValue.ToString());
        return float.TryParse(value, out float result) ? result : defaultValue;
    }

    public bool ReadBool(string section, string key, bool defaultValue = false)
    {
        string value = ReadValue(section, key, defaultValue.ToString());
        return bool.TryParse(value, out bool result) ? result : defaultValue;
    }
}