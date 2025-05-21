using System.Text;
using System.Text.Json;

namespace ShredleApi.Services
{
    public class OpenAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;
        private readonly ILogger<OpenAiService> _logger;
        private readonly bool _isDevelopment;

        public OpenAiService(IConfiguration configuration, ILogger<OpenAiService> logger, IWebHostEnvironment env)
        {
            _isDevelopment = env.IsDevelopment();
            _logger = logger;
            
            // Get API key from configuration (User Secrets in dev, Heroku Config in prod)
            _apiKey = configuration["OpenAI:ApiKey"];
            
            // Validate API key availability
            if (string.IsNullOrEmpty(_apiKey))
            {
                if (_isDevelopment)
                {
                    _logger.LogWarning("Development mode: No OpenAI API key found in User Secrets. Add it with: dotnet user-secrets set \"OpenAI:ApiKey\" \"your-key\"");
                    _logger.LogWarning("Will use fallback responses for development");
                }
                else
                {
                    _logger.LogError("Production mode: No OpenAI API key found in environment variables! Add it with: heroku config:set OPENAI__APIKEY=your-key");
                    _logger.LogError("Real-time hints and guess validation will be unavailable");
                }
            }
            
            // Initialize HttpClient
            _httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
                _logger.LogInformation("OpenAI service initialized with API key");
            }
        }

        /// <summary>
        /// Generate a hint for a song
        /// </summary>
        public async Task<string> GenerateHint(string songTitle, string artistName, string guitarist)
        {
            // For development without API key, return hardcoded hints
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogInformation("No OpenAI API key available, returning hardcoded hint");
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
                var responseObject = JsonSerializer.Deserialize<JsonDocument>(responseContent);
                
                return responseObject?
                    .RootElement
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
        
        /// <summary>
        /// Check if a user's guess matches the correct song title using AI
        /// </summary>
        public async Task<bool> CheckGuessCorrectness(string userGuess, string correctTitle, string artist)
        {
            // For development without API key, use basic comparison
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogInformation("No OpenAI API key available, using hardcoded guess checking");
                return IsHardcodedGuessCorrect(userGuess, correctTitle);
            }
            
            try
            {
                var prompt = $"This is a song guessing game. The user guessed: \"{userGuess}\"\n\n" +
                             $"The correct song title is: \"{correctTitle}\" by {artist}\n\n" +
                             "Would you consider the user's guess to be correct?\n\n" +
                             "Rules:\n" +
                             "1. Accept the guess if it's the same title translated to in any language\n" +
                             "2. Accept common variations or abbreviations of the title\n" +
                             "3. Accept if there are minor typos or misspellings\n" +
                             "4. Reject if it's clearly a different song\n\n" +
                             "e.g. escalera al cielo is acceptable for Stairway to Heaven \n\n" +
                             "Answer with ONLY 'yes' or 'no'.";

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = "You are a music expert helping with a song guessing game. Give only 'yes' or 'no' answers." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 10,
                    temperature = 0.1
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"OpenAI API error when checking guess: {errorContent}");
                    // Fall back to simple string comparison
                    return userGuess.Trim().Equals(correctTitle.Trim(), StringComparison.OrdinalIgnoreCase);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonDocument>(responseContent);
                
                var aiResponse = responseObject?
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "no";
                
                // Log the response for debugging
                _logger.LogInformation($"OpenAI guess check - User: \"{userGuess}\", Correct: \"{correctTitle}\", AI says: \"{aiResponse}\"");
                
                return aiResponse.Trim().ToLower().Contains("yes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking guess with OpenAI");
                // Fall back to simple string comparison
                return userGuess.Trim().Equals(correctTitle.Trim(), StringComparison.OrdinalIgnoreCase);
            }
        }
        
        private string GetHardcodedHint(string songTitle)
        {
            // Return hardcoded hints for our test songs
            if (_isDevelopment)
            {
                _logger.LogInformation($"Development mode: Providing hardcoded hint for '{songTitle}'");
            }
            
            return songTitle.ToLower() switch
            {
                "stairway to heaven" => "Broken Escalator to Hell",
                "while my guitar gently weeps" => "During the Crying of my Axe",
                _ => "This iconic track features one of the most recognizable guitar solos in rock history."
            };
        }
        
        private bool IsHardcodedGuessCorrect(string userGuess, string correctTitle)
        {
            if (_isDevelopment)
            {
                _logger.LogInformation($"Development mode: Comparing guess '{userGuess}' with '{correctTitle}'");
            }
            
            // Simple normalization for hardcoded checking
            var normalizedGuess = userGuess.Trim().ToLower().Replace(" ", "").Replace("'", "").Replace("-", "");
            var normalizedCorrect = correctTitle.Trim().ToLower().Replace(" ", "").Replace("'", "").Replace("-", "");
            
            // Add some known variations
            if (normalizedCorrect == "stairwaytoheaven")
            {
                return normalizedGuess == "stairwaytoheaven" || 
                       normalizedGuess == "stairway" || 
                       normalizedGuess == "escaleracielo" || 
                       normalizedGuess == "escalerainferno" ||
                       normalizedGuess == "stairway2heaven";
            }
            
            if (normalizedCorrect == "whilemyguitargentlyweeps")
            {
                return normalizedGuess == "whilemyguitargentlyweeps" || 
                       normalizedGuess == "myguitargentlyweeps" || 
                       normalizedGuess == "guitargentlyweeps" ||
                       normalizedGuess == "gentlyweeps";
            }
            
            // Default comparison
            return normalizedGuess == normalizedCorrect;
        }
    }
}
