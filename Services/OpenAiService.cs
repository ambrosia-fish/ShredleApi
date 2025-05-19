using System.Text;
using System.Text.Json;

namespace ShredleApi.Services
{
    public class OpenAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<OpenAiService> _logger;

        public OpenAiService(IConfiguration configuration, ILogger<OpenAiService> logger)
        {
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI API key not found in configuration");
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _logger = logger;
        }

        public async Task<string> GenerateHint(string songTitle, string artistName, string guitarist)
        {
            try
            {
                var prompt = $"Generate a clever hint about the song '{songTitle}' by {artistName}, featuring guitar work by {guitarist}. " +
                             "The hint should be intriguing but not too obvious, focusing on interesting facts about the song, " +
                             "its cultural impact, or distinctive elements of the guitar solo. Keep it to one short paragraph (2-3 sentences).";

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = "You are a music expert who creates clever hints about songs and their guitar solos without giving away the song title directly." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 100,
                    temperature = 0.7
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"OpenAI API error: {errorContent}");
                    return "This song features an iconic guitar solo that's instantly recognizable to rock fans.";
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                return responseObject
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "No hint available.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating hint with OpenAI");
                return "This track features a memorable guitar part that has influenced many musicians.";
            }
        }
    }
}