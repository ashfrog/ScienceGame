using UnityEngine;

public static class Settings
{
    public static SettingsManager ini => ConfigManager.Instance.Settings;
}

public class ConfigManager : MonoBehaviour
{
    private static ConfigManager instance;
    private IniConfig config;
    private SettingsManager settings;

    public static ConfigManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ConfigManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ConfigManager");
                    instance = go.AddComponent<ConfigManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    public SettingsManager Settings
    {
        get
        {
            if (settings == null)
            {
                settings = new SettingsManager(Config);
            }
            return settings;
        }
    }

    public IniConfig Config
    {
        get
        {
            if (config == null)
            {
                config = new IniConfig("settings.ini");
            }
            return config;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

/// <summary>
/// 基础设置类，提供通用的配置读写功能
/// </summary>
public abstract class BaseSettings
{
    protected readonly IniConfig config;
    protected readonly string section;

    protected BaseSettings(IniConfig config, string section)
    {
        this.config = config;
        this.section = section;
    }

    protected bool WriteValue(string key, string value) =>
    config.WriteValue(section, key, value);

    protected string ReadValue(string key, string defaultValue = "") =>
        config.ReadValue(section, key, defaultValue);

    protected float ReadFloat(string key, float defaultValue = 0f) =>
        config.ReadFloat(section, key, defaultValue);

    protected int ReadInt(string key, int defaultValue = 0) =>
        config.ReadInt(section, key, defaultValue);

    protected bool ReadBool(string key, bool defaultValue = false) =>
        config.ReadBool(section, key, defaultValue);
}