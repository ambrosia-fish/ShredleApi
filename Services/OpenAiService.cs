using System.Text;
using System.Text.Json;

namespace ShredleApi.Services
{
    public class OpenAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;
        private readonly ILogger<OpenAiService> _logger;

        public OpenAiService(IConfiguration configuration, ILogger<OpenAiService> logger)
        {
            // Make API key optional for development
            _apiKey = configuration["OpenAI:ApiKey"];
            _httpClient = new HttpClient();
            
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }
            
            _logger = logger;
        }

        public async Task<string> GenerateHint(string songTitle, string artistName, string guitarist)
        {
            // For development, return hardcoded hints if no API key
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogInformation("No OpenAI API key provided, returning hardcoded hint");
                return GetHardcodedHint(songTitle);
            }
            
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
        
        private string GetHardcodedHint(string songTitle)
        {
            // Return hardcoded hints for our test songs
            return songTitle.ToLower() switch
            {
                "stairway to heaven" => "Broken Escalator to Hell",
                "while my guitar gently weeps" => "During the Crying of my Axe",
                _ => "This iconic track features one of the most recognizable guitar solos in rock history."
            };
        }
    }
}