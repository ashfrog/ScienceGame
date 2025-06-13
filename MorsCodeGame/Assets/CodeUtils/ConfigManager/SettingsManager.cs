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

    public int Speed
    {
        get => ReadInt("Speed", 250);
        set => WriteValue("Speed", value.ToString());
    }

    public float Interval
    {
        get => ReadFloat("Interval", 0.5f);
        set => WriteValue("Interval", value.ToString());
    }
    public float WaitResultTime
    {
        get => ReadFloat("WaitResultTime", 6f);
        set => WriteValue("WaitResultTime", value.ToString());
    }
    public float DotTime
    {
        get => ReadFloat("DotTime", 0.12f);
        set => WriteValue("DotTime", value.ToString());
    }

    public bool EazyMode
    {
        get => ReadBool("EazyMode", true);
        set => WriteValue("EazyMode", value.ToString());
    }
    public float ScaleValue
    {
        get => ReadFloat("ScaleValue", 1.5f);
        set => WriteValue("ScaleValue", value.ToString());
    }
    public float ToleranceTimeStart
    {
        get => ReadFloat("ToleranceTimeStart", 0.1f);
        set => WriteValue("ToleranceTimeStart", value.ToString());
    }

    public float ToleranceTimeEnd
    {
        get => ReadFloat("ToleranceTimeEnd", 0.25f);
        set => WriteValue("ToleranceTimeEnd", value.ToString());
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

    public string TextTooltip
    {
        get => ReadValue("TextTooltip", "发报机玩法：按K键开始游戏，长按/短按K键发送不同信号（长按=长信号，短按=短信号）。通过组合长短信号传递信息，模拟真实电报操作。注意节奏间隔，准确还原莫尔斯电码。");
        set => WriteValue("TextTooltip", value);
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