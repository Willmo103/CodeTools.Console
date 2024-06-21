using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace GroqApi
{
    public interface IGroqCommand
    {
        string Template { get; }
        string[] InputItems { get; set; }
        string OutputFilePath { get; set; }
        Task ExecuteAsync();
    }

    public abstract class AbstractGroqCommand : IGroqCommand
    {
        protected readonly GroqApiClient ApiClient;
        protected readonly AppConfigManager ConfigManager;

        public abstract string Template { get; }
        public string[] InputItems { get; set; }
        public string OutputFilePath { get; set; }

        protected AbstractGroqCommand(GroqApiClient apiClient, AppConfigManager configManager)
        {
            ApiClient = apiClient;
            ConfigManager = configManager;
            InputItems = new string[1]; // Default to one input item
        }

        public abstract Task ExecuteAsync();

        protected async Task SaveResponseToFileAsync(string content)
        {
            if (!string.IsNullOrEmpty(OutputFilePath))
            {
                string directory = Path.GetDirectoryName(OutputFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string extension = Path.GetExtension(OutputFilePath).ToLower();
                if (extension != ".txt" && extension != ".json")
                {
                    throw new ArgumentException("Output file must have a .txt or .json extension.");
                }

                await File.WriteAllTextAsync(OutputFilePath, content);
            }
        }

        protected void LogActivity(string activity)
        {
            ConfigManager.LogActivity(GetType().Name, activity);
        }
    }

    public class GroqApiClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.groq.com/openai/v1/";

        public GroqApiClient(string apiKey)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<JObject> GetModelsAsync()
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}models");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JObject.Parse(content);
        }

        public async Task<JObject> CreateChatCompletionAsync(string model, string template, string[] inputItems)
        {
            string formattedMessage = string.Format(template, inputItems);
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "user", content = formattedMessage }
                },
                model = model
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BaseUrl}chat/completions", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseContent);
        }

        public async Task<JObject> SendCustomRequestAsync(HttpMethod method, string endpoint, object requestBody = null)
        {
            var request = new HttpRequestMessage(method, $"{BaseUrl}{endpoint}");

            if (requestBody != null)
            {
                var json = JsonConvert.SerializeObject(requestBody);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JObject.Parse(content);
        }
    }
}