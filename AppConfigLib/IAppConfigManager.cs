public interface IAppConfigManager
{
    void CreateConfigFile(string appName);
    void LogActivity(string appName, string activity);
    void UpdateSettings(string appName, object settings);
    T ReadSettings<T>(string appName);
}
