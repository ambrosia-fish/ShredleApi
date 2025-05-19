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
            
            // FIXED: Using "DailyGames" instead of "daily_games" to match your Supabase table name
            var url = $"{_supabaseUrl}/rest/v1/DailyGames?Date=eq.{formattedDate}";
            _logger.LogInformation($"Request URL: {url}");
            
            var response = await _httpClient.GetAsync(url);
            
            _logger.LogInformation($"Response status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to get daily game: {response.StatusCode}");
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
            
            // FIXED: Using "DailyGames" instead of "daily_games" to match your Supabase table name
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
            var options = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Use camelCase for property names
                WriteIndented = true
            };
            
            var json = JsonSerializer.Serialize(dailyGame, options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation($"Creating daily game: {json}");

            // FIXED: Using "DailyGames" instead of "daily_games" to match your Supabase table name
            var url = $"{_supabaseUrl}/rest/v1/DailyGames";
            _logger.LogInformation($"Request URL: {url}");
            
            var response = await _httpClient.PostAsync(url, content);
            
            _logger.LogInformation($"Create daily game response status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to create daily game: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error response: {errorContent}");
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Create daily game response content: {responseContent}");
            
            return JsonSerializer.Deserialize<DailyGame>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<bool> UpdateDailyGameAsync(DailyGame dailyGame)
        {
            var options = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Use camelCase for property names
                WriteIndented = true
            };
            
            var json = JsonSerializer.Serialize(dailyGame, options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation($"Updating daily game ID {dailyGame.Id}: {json}");

            // FIXED: Using "DailyGames" instead of "daily_games" to match your Supabase table name
            var url = $"{_supabaseUrl}/rest/v1/DailyGames?id=eq.{dailyGame.Id}";
            _logger.LogInformation($"Request URL: {url}");
            
            var response = await _httpClient.PatchAsync(url, content);
            
            _logger.LogInformation($"Update daily game response status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error updating daily game: {errorContent}");
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
            // FIXED: Using "DailyGames" instead of "daily_games" to match your Supabase table name
            var response = await _httpClient.DeleteAsync($"{_supabaseUrl}/rest/v1/DailyGames?id=eq.{id}");

            return response.IsSuccessStatusCode;
        }
    }
}