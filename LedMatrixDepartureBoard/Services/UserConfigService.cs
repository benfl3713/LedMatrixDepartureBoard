using System.Text.Json;

namespace LedMatrixDepartureBoard.Services;

public class UserConfigService
{
    private const string ConfigFileName = "./user.config.json";
    public event EventHandler<UserConfig> ConfigChange;
    public UserConfig Get()
    {
        if (File.Exists(ConfigFileName))
            return JsonSerializer.Deserialize<UserConfig>(File.ReadAllText(ConfigFileName))!;
        
        return new UserConfig
        {
            Enabled = true,
            StationCode = "EUS"
        };
    }

    public void Save(UserConfig config)
    {
        var contents = JsonSerializer.Serialize(config);
        File.WriteAllText(ConfigFileName, contents);
        ConfigChange.Invoke(this, Get());
    }
}