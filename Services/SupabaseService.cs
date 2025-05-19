using System.Text.Json;
using System.Text;
using ShredleApi.Models;

namespace ShredleApi.Services
{
    public class SupabaseService
    {
        private readonly HttpClient _httpClient;
        private readonly string _supabaseUrl;
        private readonly string _supabaseKey;
        private readonly ILogger<SupabaseService> _logger;

        public SupabaseService(IConfiguration configuration, ILogger<SupabaseService> logger)
        {
            _supabaseUrl = configuration["Supabase:Url"]!;
            _supabaseKey = configuration["Supabase:Key"]!;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");
            _logger = logger;
            
            _logger.LogInformation($"SupabaseService initialized with URL: {_supabaseUrl}");
        }

        public async Task<List<Solo>> GetSolosAsync()
        {
            try 
            {
                _logger.LogInformation("Attempting to fetch all solos...");
                var url = $"{_supabaseUrl}/rest/v1/solos?select=*";
                _logger.LogInformation($"Request URL: {url}");
                
                var response = await _httpClient.GetAsync(url);

                _logger.LogInformation($"Response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to get solos: {response.StatusCode}");
                    _logger.LogError($"Response content: {await response.Content.ReadAsStringAsync()}");
                    return new List<Solo>();
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response content: {content}");
                
                // For debugging, print the exact content
                if (string.IsNullOrEmpty(content) || content == "[]")
                {
                    _logger.LogWarning("No solos found in database - empty response");
                    return new List<Solo>();
                }
                
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var solos = JsonSerializer.Deserialize<List<Solo>>(content, options) ?? new List<Solo>();
                _logger.LogInformation($"Successfully deserialized {solos.Count} solos");
                
                return solos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving solos");
                return new List<Solo>();
            }
        }

        public async Task<Solo?> GetSoloByIdAsync(int id)
        {
            try {
                _logger.LogInformation($"Fetching solo with ID {id}");
                var url = $"{_supabaseUrl}/rest/v1/solos?id=eq.{id}";
                _logger.LogInformation($"Request URL: {url}");
                
                var response = await _httpClient.GetAsync(url);
                
                _logger.LogInformation($"Response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to get solo: {response.StatusCode}");
                    // Fallback to hardcoded solos for testing
                    return GetHardcodedSolo(id);
                }
                
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response content: {content}");
                
                if (string.IsNullOrEmpty(content) || content == "[]")
                {
                    _logger.LogWarning($"No solo found with ID {id} - empty response");
                    // Fallback to hardcoded solos for testing
                    return GetHardcodedSolo(id);
                }
                
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var solos = JsonSerializer.Deserialize<List<Solo>>(content, options);
                return solos?.FirstOrDefault() ?? GetHardcodedSolo(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error finding solo with ID {id}");
                // Fallback to hardcoded solos for testing
                return GetHardcodedSolo(id);
            }
        }
        
        private Solo? GetHardcodedSolo(int id)
        {
            _logger.LogWarning($"Using hardcoded solo data for ID {id}");
            
            if (id == 1)
            {
                return new Solo
                {
                    Id = 1,
                    Title = "Stairway to Heaven",
                    Artist = "Led Zeppelin",
                    SpotifyId = "5CQ30WqJwcep0pYcV4AMNc", 
                    SoloStartTimeMs = 30000,
                    SoloEndTimeMs = 90000,
                    Guitarist = "Jimmy Page"
                };
            }
            else if (id == 0)
            {
                return new Solo
                {
                    Id = 0,
                    Title = "While My Guitar Gently Weeps",
                    Artist = "The Beatles",
                    SpotifyId = "2EEaNpFdykm5yYlkR3izeE",
                    SoloStartTimeMs = 30000,
                    SoloEndTimeMs = 90000,
                    Guitarist = "Eric Clapton"
                };
            }
            
            return null;
        }

        public async Task<DailyGame?> GetDailyGameAsync(DateTime date)
        {
            var formattedDate = date.ToString("yyyy-MM-dd");
            _logger.LogInformation($"Getting daily game for date: {formattedDate}");
            
            // Try different approaches to access the table to figure out what works
            try
            {
                // First, let's try to get table structure
                var metaUrl = $"{_supabaseUrl}/rest/v1/";
                _logger.LogInformation($"Getting table info: {metaUrl}");
                var metaResponse = await _httpClient.GetAsync(metaUrl);
                _logger.LogInformation($"Meta response status: {metaResponse.StatusCode}");
                if (metaResponse.IsSuccessStatusCode)
                {
                    var metaContent = await metaResponse.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Available tables: {metaContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying table structure");
            }
            
            // Based on the screenshot, the actual column name should be "Date" with capital D
            var url = $"{_supabaseUrl}/rest/v1/DailyGames?Date=eq.{formattedDate}";
            _logger.LogInformation($"Request URL: {url}");
            
            var response = await _httpClient.GetAsync(url);
            
            _logger.LogInformation($"Response status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to get daily game: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error content: {errorContent}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Daily game response content: {content}");
            
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var games = JsonSerializer.Deserialize<List<DailyGame>>(content, options);
            return games?.FirstOrDefault();
        }

        public async Task<DailyGame?> GetTodaysDailyGameAsync()
        {
            var todayDate = DateTime.UtcNow.Date;
            return await GetDailyGameAsync(todayDate);
        }

        public async Task<List<DailyGame>> GetRecentDailyGamesAsync(int days)
        {
            // Get daily games from the last 'days' days
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var formattedDate = startDate.ToString("yyyy-MM-dd");
            
            var url = $"{_supabaseUrl}/rest/v1/DailyGames?Date=gte.{formattedDate}&order=Date.desc";
            _logger.LogInformation($"Request URL: {url}");
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to get recent daily games: {response.StatusCode}");
                return new List<DailyGame>();
            }

            var content = await response.Content.ReadAsStringAsync();
            
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            return JsonSerializer.Deserialize<List<DailyGame>>(content, options) ?? new List<DailyGame>();
        }

        public async Task<DailyGame?> CreateDailyGameAsync(DailyGame dailyGame)
        {
            // Let's try using the PascalCase column names to match what we see in the screenshot
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true
            };
            
            // Let's try to create a game with several variants to see what works
            _logger.LogInformation("Trying multiple approaches to create a daily game");
            
            // Define the different variants to try
            var attempt1 = new Dictionary<string, object> 
            { 
                { "Date", dailyGame.Date.ToString("yyyy-MM-dd") },
                { "SoloId", dailyGame.SoloId }
            };
            
            var attempt2 = new Dictionary<string, object> 
            { 
                { "date", dailyGame.Date.ToString("yyyy-MM-dd") },
                { "soloid", dailyGame.SoloId }
            };
            
            var attempt3 = new Dictionary<string, object> 
            { 
                { "Date", dailyGame.Date.ToString("yyyy-MM-dd") },
                { "soloid", dailyGame.SoloId }
            };
            
            var attempt4 = new Dictionary<string, object> 
            { 
                { "date", dailyGame.Date.ToString("yyyy-MM-dd") },
                { "SoloId", dailyGame.SoloId }
            };
            
            var attempt5 = new Dictionary<string, object> 
            { 
                { "DATE", dailyGame.Date.ToString("yyyy-MM-dd") },
                { "SOLOID", dailyGame.SoloId }
            };
            
            var attempts = new[] { attempt1, attempt2, attempt3, attempt4, attempt5 };
            
            var url = $"{_supabaseUrl}/rest/v1/DailyGames";
            
            // First check if the table exists
            var tableCheckUrl = $"{_supabaseUrl}/rest/v1/DailyGames?limit=1";
            var tableCheckResponse = await _httpClient.GetAsync(tableCheckUrl);
            _logger.LogInformation($"Table check response status: {tableCheckResponse.StatusCode}");
            
            if (!tableCheckResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Table DailyGames may not exist or is not accessible");
                var tableCheckContent = await tableCheckResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Table check error: {tableCheckContent}");
            }
            
            // Try each attempt
            foreach (var attempt in attempts)
            {
                try
                {
                    var json = JsonSerializer.Serialize(attempt, options);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    _logger.LogInformation($"Trying with: {json}");
                    
                    var response = await _httpClient.PostAsync(url, content);
                    _logger.LogInformation($"Response: {response.StatusCode}");
                    
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Response content: {responseContent}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Success with: {json}");
                        return JsonSerializer.Deserialize<DailyGame>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during attempt");
                }
            }
            
            _logger.LogError("All attempts to create daily game failed");
            return null;
        }

        public async Task<bool> UpdateDailyGameAsync(DailyGame dailyGame)
        {
            // Try with PascalCase column names
            var rawJson = new
            {
                Date = dailyGame.Date.ToString("yyyy-MM-dd"),
                SoloId = dailyGame.SoloId
            };
            
            var json = JsonSerializer.Serialize(rawJson);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation($"Updating daily game ID {dailyGame.Id} with raw JSON: {json}");

            var url = $"{_supabaseUrl}/rest/v1/DailyGames?Id=eq.{dailyGame.Id}";
            _logger.LogInformation($"Request URL: {url}");
            
            var response = await _httpClient.PatchAsync(url, content);
            
            _logger.LogInformation($"Update daily game response status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error updating daily game: {errorContent}");
                
                // Try alternate casing
                var alternateJson = new
                {
                    date = dailyGame.Date.ToString("yyyy-MM-dd"),
                    soloid = dailyGame.SoloId
                };
                
                var alternateContent = new StringContent(
                    JsonSerializer.Serialize(alternateJson), 
                    Encoding.UTF8, 
                    "application/json");
                
                var alternateUrl = $"{_supabaseUrl}/rest/v1/DailyGames?id=eq.{dailyGame.Id}";
                _logger.LogInformation($"Alternate URL: {alternateUrl} with JSON: {JsonSerializer.Serialize(alternateJson)}");
                
                var alternateResponse = await _httpClient.PatchAsync(alternateUrl, alternateContent);
                _logger.LogInformation($"Alternate response status: {alternateResponse.StatusCode}");
                
                return alternateResponse.IsSuccessStatusCode;
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<Solo?> CreateSoloAsync(Solo solo)
        {
            var options = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            
            var json = JsonSerializer.Serialize(solo, options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_supabaseUrl}/rest/v1/solos", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Solo>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<bool> UpdateSoloAsync(Solo solo)
        {
            var options = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            
            var json = JsonSerializer.Serialize(solo, options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync($"{_supabaseUrl}/rest/v1/solos?id=eq.{solo.Id}", content);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSoloAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_supabaseUrl}/rest/v1/solos?id=eq.{id}");

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteDailyGameAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_supabaseUrl}/rest/v1/DailyGames?Id=eq.{id}");

            return response.IsSuccessStatusCode;
        }
    }
}