using UnityEngine;

/// <summary>
/// 图形设置
/// </summary>
public class SGraphicsSettings : BaseSettings
{
    public SGraphicsSettings(IniConfig config) : base(config, "Graphics")
    {
    }

    public float WebResolution
    {
        get => ReadFloat("WebResolution", 0.5f);
        set => WriteValue("WebResolution", value.ToString());
    }

    public string Quality
    {
        get => ReadValue("Quality", "Medium");
        set => WriteValue("Quality", value);
    }

    //多个字体大小设置 用逗号分割
    public string FontSize
    {
        get => ReadValue("FontSize", "1.67,2,5,3,3");
        set => WriteValue("FontSize", value);
    }

    //多个字体颜色设置 用逗号分割
    public string FontColor
    {
        get => ReadValue("FontColor", "#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF");
        set => WriteValue("FontColor", value);
    }

    public bool FullScreen
    {
        get => ReadBool("FullScreen", true);
        set => WriteValue("FullScreen", value.ToString());
    }

    public float ScrollSpeed
    {
        get => ReadFloat("ScrollSpeed", 1.45f);
        set => WriteValue("ScrollSpeed", value.ToString());
    }

    public (int width, int height) Resolution
    {
        get
        {
            string resolution = ReadValue("Resolution", "1920*1080");
            string[] parts = resolution.Split('*');
            if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
            {
                return (width, height);
            }
            return (1920, 1080);
        }
        set => WriteValue("Resolution", $"{value.width}*{value.height}");
    }
}

/// <summary>
/// 游戏设置
/// </summary>
public class GameSettings : BaseSettings
{
    private static readonly string[] validDifficulties = { "Easy", "Normal", "Hard" };

    public GameSettings(IniConfig config) : base(config, "Game")
    {
    }

    public bool SkipVerify
    {
        get => ReadBool("SkipVerify", false);
        set => WriteValue("SkipVerify", value.ToString());
    }

    public string Difficulty
    {
        get => ReadValue("Difficulty", "Normal");
        set
        {
            if (System.Array.Exists(validDifficulties, d => d == value))
            {
                WriteValue("Difficulty", value);
            }
        }
    }

    public string Language
    {
        get => ReadValue("Language", "en");
        set => WriteValue("Language", value);
    }
}

public class IPSettings : BaseSettings
{
    public IPSettings(IniConfig config) : base(config, "IPHost")
    {
    }

    public string QueServer
    {
        get => ReadValue("QueServer", "127.0.0.1");
        set => WriteValue("QueServer", value.ToString());
    }

    public string FHServerIP
    {
        get => ReadValue("FHServerIP", "127.0.0.1");
        set => WriteValue("FHServerIP", value.ToString());
    }

    public int FHServerPort
    {
        get => ReadInt("FHServerPort", 4849);
        set => WriteValue("FHServerPort", value.ToString());
    }
}

/// <summary>
/// 设置管理器
/// </summary>
public class SettingsManager
{
    public SGraphicsSettings Graphics { get; }
    public AudioSettings Audio { get; }
    public GameSettings Game { get; }
    public IPSettings IPHost { get; }

    public SettingsManager(IniConfig config)
    {
        Graphics = new SGraphicsSettings(config);
        Game = new GameSettings(config);
        IPHost = new IPSettings(config);
    }
}