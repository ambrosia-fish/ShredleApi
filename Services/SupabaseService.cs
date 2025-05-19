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
        }

        /// <summary>
        /// Get all solos from the database
        /// </summary>
        public async Task<List<Solo>> GetSolosAsync()
        {
            try 
            {
                var response = await _httpClient.GetAsync($"{_supabaseUrl}/rest/v1/solos?select=*");
                
                if (!response.IsSuccessStatusCode)
                    return new List<Solo>();

                var content = await response.Content.ReadAsStringAsync();
                
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<List<Solo>>(content, options) ?? new List<Solo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving solos");
                return new List<Solo>();
            }
        }

        /// <summary>
        /// Get a solo by ID, with fallback to hardcoded data for testing
        /// </summary>
        public async Task<Solo?> GetSoloByIdAsync(int id)
        {
            try {
                var response = await _httpClient.GetAsync($"{_supabaseUrl}/rest/v1/solos?id=eq.{id}");
                
                if (!response.IsSuccessStatusCode)
                    return GetHardcodedSolo(id);
                
                var content = await response.Content.ReadAsStringAsync();
                
                if (string.IsNullOrEmpty(content) || content == "[]")
                    return GetHardcodedSolo(id);
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var solos = JsonSerializer.Deserialize<List<Solo>>(content, options);
                
                return solos?.FirstOrDefault() ?? GetHardcodedSolo(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error finding solo with ID {id}");
                return GetHardcodedSolo(id);
            }
        }
        
        /// <summary>
        /// Provides hardcoded solo data for testing
        /// </summary>
        private Solo? GetHardcodedSolo(int id)
        {
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

        /// <summary>
        /// Get the daily game for a specific date
        /// </summary>
        public async Task<DailyGame?> GetDailyGameAsync(DateTime date)
        {
            // Get the start of the day in ISO format
            var startOfDay = date.ToString("yyyy-MM-dd");
            
            _logger.LogInformation($"Looking for daily game on date: {startOfDay}");
            _logger.LogInformation($"Original date passed: {date}");
            
            try
            {
                // Use date portion only, with more lenient matching
                var url = $"{_supabaseUrl}/rest/v1/DailyGames?Date=like.{startOfDay}%";
                _logger.LogInformation($"Making request to URL: {url}");
                
                var response = await _httpClient.GetAsync(url);
                
                _logger.LogInformation($"Response status code: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to get daily game: {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response content: {content}");
                
                if (string.IsNullOrEmpty(content) || content == "[]")
                {
                    _logger.LogWarning("Response was empty or an empty array");
                    return null;
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var games = JsonSerializer.Deserialize<List<DailyGame>>(content, options);
                
                _logger.LogInformation($"Deserialized {games?.Count ?? 0} game(s)");
                
                return games?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting daily game for date {startOfDay}");
                return null;
            }
        }

        /// <summary>
        /// Get today's daily game
        /// </summary>
        public async Task<DailyGame?> GetTodaysDailyGameAsync()
        {
            var todayDate = DateTime.UtcNow.Date;
            return await GetDailyGameAsync(todayDate);
        }

        /// <summary>
        /// Get recent daily games
        /// </summary>
        public async Task<List<DailyGame>> GetRecentDailyGamesAsync(int days)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var formattedDate = startDate.ToString("yyyy-MM-dd");
            
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_supabaseUrl}/rest/v1/DailyGames?Date=gte.{formattedDate}&order=Date.desc");
                
                if (!response.IsSuccessStatusCode)
                    return new List<DailyGame>();

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                
                return JsonSerializer.Deserialize<List<DailyGame>>(content, options) ?? new List<DailyGame>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent daily games");
                return new List<DailyGame>();
            }
        }

        /// <summary>
        /// Create a new daily game
        /// </summary>
        public async Task<DailyGame?> CreateDailyGameAsync(DailyGame dailyGame)
        {
            try
            {
                // Based on our testing, this format works with Supabase
                var data = new Dictionary<string, object> 
                { 
                    { "Date", dailyGame.Date.ToString("yyyy-MM-dd") },
                    { "SoloId", dailyGame.SoloId }
                };
                
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_supabaseUrl}/rest/v1/DailyGames", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to create daily game: {response.StatusCode}, error: {errorContent}");
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                
                return JsonSerializer.Deserialize<DailyGame>(responseContent, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating daily game");
                return null;
            }
        }

        /// <summary>
        /// Update an existing daily game
        /// </summary>
        public async Task<bool> UpdateDailyGameAsync(DailyGame dailyGame)
        {
            try
            {
                var data = new Dictionary<string, object> 
                { 
                    { "Date", dailyGame.Date.ToString("yyyy-MM-dd") },
                    { "SoloId", dailyGame.SoloId }
                };
                
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PatchAsync($"{_supabaseUrl}/rest/v1/DailyGames?Id=eq.{dailyGame.Id}", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error updating daily game: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating daily game ID {dailyGame.Id}");
                return false;
            }
        }

        /// <summary>
        /// Create a new solo
        /// </summary>
        public async Task<Solo?> CreateSoloAsync(Solo solo)
        {
            try
            {
                var json = JsonSerializer.Serialize(solo);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_supabaseUrl}/rest/v1/solos", content);

                if (!response.IsSuccessStatusCode)
                    return null;

                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                
                return JsonSerializer.Deserialize<Solo>(responseContent, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating solo");
                return null;
            }
        }

        /// <summary>
        /// Update an existing solo
        /// </summary>
        public async Task<bool> UpdateSoloAsync(Solo solo)
        {
            try
            {
                var json = JsonSerializer.Serialize(solo);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync($"{_supabaseUrl}/rest/v1/solos?id=eq.{solo.Id}", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating solo ID {solo.Id}");
                return false;
            }
        }

        /// <summary>
        /// Delete a solo
        /// </summary>
        public async Task<bool> DeleteSoloAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_supabaseUrl}/rest/v1/solos?id=eq.{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting solo ID {id}");
                return false;
            }
        }

        /// <summary>
        /// Delete a daily game
        /// </summary>
        public async Task<bool> DeleteDailyGameAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_supabaseUrl}/rest/v1/DailyGames?Id=eq.{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting daily game ID {id}");
                return false;
            }
        }
    }
}