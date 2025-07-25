using System.Collections.Generic;
using System.IO;

using System.Text;
using UnityEngine;

/// <summary>
/// INI配置文件管理 (INI文件的底层读写操作)
/// </summary>
public class IniConfig
{
    private readonly string iniFilePath;
    private readonly Dictionary<string, Dictionary<string, string>> cachedConfig;
    private bool isDirty;

    public IniConfig(string fileName)
    {
        cachedConfig = new Dictionary<string, Dictionary<string, string>>();

        string directory = Application.streamingAssetsPath;
#if UNITY_ANDROID && !UNITY_EDITOR
        directory = Application.persistentDataPath;
#endif

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        iniFilePath = Path.Combine(directory, fileName);

        if (!File.Exists(iniFilePath))
            File.Create(iniFilePath).Dispose();

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
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";"))
                    continue;

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    if (!cachedConfig.ContainsKey(currentSection))
                        cachedConfig[currentSection] = new Dictionary<string, string>();
                }
                else
                {
                    int equalPos = trimmedLine.IndexOf('=');
                    if (equalPos > 0 && !string.IsNullOrEmpty(currentSection))
                    {
                        string key = trimmedLine.Substring(0, equalPos).Trim();
                        string value = trimmedLine.Substring(equalPos + 1).Trim();
                        cachedConfig[currentSection][key] = value;
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
                        writer.WriteLine($"{kvp.Key}={kvp.Value}");
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
        return cachedConfig.ContainsKey(section) && cachedConfig[section].ContainsKey(key)
            ? cachedConfig[section][key]
            : defaultValue;
    }

    public bool WriteValue(string section, string key, string value)
    {
        try
        {
            if (!cachedConfig.ContainsKey(section))
                cachedConfig[section] = new Dictionary<string, string>();

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
        if (bool.TryParse(value, out bool result)) return result;
        if (value == "1") return true;
        if (value == "0") return false;
        return defaultValue;
    }
}