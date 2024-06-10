using TextCopy;
using System.Text.RegularExpressions;
using System.Net.Sockets;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        var configManager = new AppConfigManager();
        string appName = "FlattenDockerRun";
        configManager.CreateConfigFile(appName);
        try
        {
            // Read text from clipboard
            string dockerRunCommand = ClipboardService.GetTextAsync().Result ?? string.Empty;

            // Remove trailing backslashes and join lines
            string flattenedText = Regex.Replace(dockerRunCommand, @"\\\s*\r?\n\s*", " ");

            // Remove extra spaces around equal signs
            flattenedText = Regex.Replace(dockerRunCommand, @"\s*=\s*", "=");

            // Remove trailing backslashes and join lines   
            ClipboardService.SetTextAsync(flattenedText).Wait();

            // Log activity
            Console.WriteLine("Flattened Docker run command copied to clipboard:");

            // Write flattened text to log
            configManager.LogActivity(appName, $"Flattened Docker run command: {flattenedText}" + $"Time: { DateTime.Now }");
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine("Error: " + ex.Message);

            // Log activity
            configManager.LogActivity(appName, $"Error: {ex.Message}");
        }
    }
}
