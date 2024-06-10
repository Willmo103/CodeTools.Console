using Newtonsoft.Json;
using System;
using System.IO;

public class AppConfigManager : IAppConfigManager
{
    private readonly string _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".script_settings");

    public AppConfigManager()
    {
        if (!Directory.Exists(_configPath))
        {
            Directory.CreateDirectory(_configPath);
        }
    }

    public void CreateConfigFile(string appName)
    {
        string settingsPath = Path.Combine(_configPath, $"{appName}.settings.json");
        string logPath = Path.Combine(_configPath, $"{appName}.log.json");

        if (!File.Exists(settingsPath))
        {
            File.WriteAllText(settingsPath, "{}");
        }

        if (!File.Exists(logPath))
        {
            File.WriteAllText(logPath, "[]");
        }
    }

    public void LogActivity(string appName, string activity)
    {
        string logPath = Path.Combine(_configPath, $"{appName}.log.json");
        var logs = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(logPath)) ?? new List<string>();
        logs.Add($"{DateTime.UtcNow}: {activity}");
        File.WriteAllText(logPath, JsonConvert.SerializeObject(logs, Formatting.Indented));
    }

    public void UpdateSettings(string appName, object settings)
    {
        string settingsPath = Path.Combine(_configPath, $"{appName}.settings.json");
        string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText(settingsPath, json);
    }

    public T ReadSettings<T>(string appName)
    {
        string settingsPath = Path.Combine(_configPath, $"{appName}.settings.json");
        string json = File.ReadAllText(settingsPath);
        return JsonConvert.DeserializeObject<T>(json);
    }
}
