using TextCopy;

namespace FlattenCB
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var configManager = new AppConfigManager();
            string appName = "FlattenCB";
            configManager.CreateConfigFile(appName);

            try
            {
                string clipboardText = ClipboardService.GetTextAsync().Result ?? string.Empty;
                string flattenedText = clipboardText.Replace("\r", "").Replace("\n", " ").Replace("  ", " ");
                ClipboardService.SetTextAsync(flattenedText).Wait();
                Console.WriteLine("Text has been flattened and updated in the clipboard.");
                configManager.LogActivity(appName, $"Flattened text: {flattenedText}" + $"Time: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                configManager.LogActivity(appName, $"Error: {ex.Message}");
            }
        }
    }
}
