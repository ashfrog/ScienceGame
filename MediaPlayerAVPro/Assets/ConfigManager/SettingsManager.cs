/// <summary>
/// 设置管理器
/// </summary>
public class SettingsManager
{
    public MGraphicsSettings Graphics { get; }
    public MGameSettings Game { get; }
    public MIPSettings IPHost { get; }
    public MPathSettings Path { get; }

    public SettingsManager(IniConfig config)
    {
        Graphics = new MGraphicsSettings(config);
        Game = new MGameSettings(config);
        IPHost = new MIPSettings(config);
        Path = new MPathSettings(config);
    }
}