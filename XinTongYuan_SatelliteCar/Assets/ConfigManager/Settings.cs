using System.IO;
using UnityEngine;


/// <summary>
/// 静态访问入口
/// </summary>
public static class Settings
{
    public static SettingsManager ini => ConfigManager.Instance.Settings;
}

/// <summary>
/// 图形配置
/// </summary>
public class DisplaySettings : BaseSettings
{
    public DisplaySettings(IniConfig config) : base(config, "Graphics") { }

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

    public bool FullScreen
    {
        get => ReadBool("FullScreen", true);
        set => WriteValue("FullScreen", value.ToString());
    }

    public string ScreenSaver
    {
        get => ReadValue("ScreenSaver");
        set => WriteValue("ScreenSaver", value);
    }

    public (int width, int height) Resolution
    {
        get
        {
            string resolution = ReadValue("Resolution", "1920x1080");
            string[] parts = resolution.Split('x');
            if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
                return (width, height);
            return (1920, 1080);
        }
        set => WriteValue("Resolution", $"{value.width}x{value.height}");
    }
}

/// <summary>
/// 游戏配置
/// </summary>
public class GameSettings : BaseSettings
{
    private static readonly string[] ValidDifficulties = { "Easy", "Normal", "Hard" };

    public GameSettings(IniConfig config) : base(config, "Game") { }

    public float Volume
    {
        get => ReadFloat("Volume", 1f);
        set => WriteValue("Volume", value.ToString());
    }

    public float AutoResetTime
    {
        get => ReadFloat("AutoResetTime", 300f);
        set => WriteValue("AutoResetTime", value.ToString());
    }

    public float ZSpeed
    {
        get => ReadFloat("ZSpeed", 1f);
        set => WriteValue("ZSpeed", value.ToString());
    }

    public float ProjectorRX
    {
        get => ReadFloat("ProjectorRX", 1f);
        set => WriteValue("ProjectorRX", value.ToString());
    }

    public float ProjectorY
    {
        get => ReadFloat("ProjectorY", 1f);
        set => WriteValue("ProjectorY", value.ToString());
    }

    public float ProjectorZ
    {
        get => ReadFloat("ProjectorZ", 1f);
        set => WriteValue("ProjectorZ", value.ToString());
    }

    public float PanelY
    {
        get => ReadFloat("PanelY", 1f);
        set => WriteValue("PanelY", value.ToString());
    }

    public bool DebugProjector
    {
        get => ReadBool("DebugProjector", false);
        set => WriteValue("DebugProjector", value.ToString());
    }

    public string LoopMode
    {
        get => ReadValue("LoopMode", "all");
        set => WriteValue("LoopMode", value);
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
            if (System.Array.Exists(ValidDifficulties, d => d == value))
                WriteValue("Difficulty", value);
        }
    }

    public string Language
    {
        get => ReadValue("Language", "en");
        set => WriteValue("Language", value);
    }
}

/// <summary>
/// IP配置
/// </summary>
public class IPSettings : BaseSettings
{
    public IPSettings(IniConfig config) : base(config, "IPHost") { }

    public string QueServer
    {
        get => ReadValue("QueServer", "127.0.0.1");
        set => WriteValue("QueServer", value);
    }

    public string FHServerIP
    {
        get => ReadValue("FHServerIP", "127.0.0.1");
        set => WriteValue("FHServerIP", value);
    }

    public string ServerIPHost
    {
        get => ReadValue("ServerIPHost", "");
        set => WriteValue("ServerIPHost", value);
    }

    public string DoorIPHost
    {
        get => ReadValue("DoorIPHost", "");
        set => WriteValue("DoorIPHost", value);
    }

    public int IPNO
    {
        get => ReadInt("IPNO", -1);
        set => WriteValue("IPNO", value.ToString());
    }

    public int PadIPNO
    {
        get => ReadInt("PadIPNO", -1);
        set => WriteValue("PadIPNO", value.ToString());
    }
}

/// <summary>
/// 路径配置
/// </summary>
public class PathSettings : BaseSettings
{
    public PathSettings(IniConfig config) : base(config, "PathSettings") { }

    public string MediaPath
    {
        get => ReadValue("MediaPath", Path.Combine(Application.streamingAssetsPath, "媒体文件"));
        set => WriteValue("MediaPath", value);
    }
}

