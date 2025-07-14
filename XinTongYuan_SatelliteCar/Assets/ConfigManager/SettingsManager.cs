/// <summary>
/// 设置管理器
/// </summary>
public class SettingsManager
{
    public DisplaySettings Graphics { get; }
    public GameSettings Game { get; }
    public IPSettings IPHost { get; }
    public PathSettings Path { get; }

    public SettingsManager(IniConfig config)
    {
        Graphics = new DisplaySettings(config);
        Game = new GameSettings(config);
        IPHost = new IPSettings(config);
        Path = new PathSettings(config);
    }
}