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
            // First check environment variables (Heroku)
            _supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") 
                ?? configuration["Supabase:Url"]!;
                
            _supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY") 
                ?? configuration["Supabase:Key"]!;
                
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");
            _logger = logger;
            
            _logger.LogInformation("Supabase service initialized with URL: {Url}", _supabaseUrl);
            _logger.LogInformation("Supabase key length: {KeyLength}", _supabaseKey?.Length ?? 0);
            
            // Log additional diagnostic information about the URL
            if (!string.IsNullOrEmpty(_supabaseUrl))
            {
                // Check if URL ends with a slash
                if (_supabaseUrl.EndsWith("/"))
                {
                    _logger.LogWarning("Supabase URL ends with a slash, which may cause issues with API calls");
                }
            }
            
            // For debugging - log the first few characters of the key (never log full keys)
            if (!string.IsNullOrEmpty(_supabaseKey) && _supabaseKey.Length > 10)
            {
                _logger.LogInformation("Supabase key starts with: {KeyStart}...", _supabaseKey.Substring(0, 10));
            }
        }

        /// <summary>
        /// Get all solos from the database
        /// </summary>
        public async Task<List<Solo>> GetSolosAsync()
        {
            try 
            {
                _logger.LogInformation("Fetching solos from Supabase");
                // Use proper PascalCase table name to match database
                string requestUrl = $"{_supabaseUrl}/rest/v1/Solos?select=*";
                _logger.LogInformation("Request URL: {Url}", requestUrl);
                
                var response = await _httpClient.GetAsync(requestUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Solos response: {Content}", content);
                    var solos = JsonSerializer.Deserialize<List<Solo>>(content, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                    
                    return solos ?? new List<Solo>();
                }
                
                _logger.LogWarning("Failed to fetch solos from Supabase, status code: {StatusCode}, response: {Response}", 
                    response.StatusCode, await response.Content.ReadAsStringAsync());
                return new List<Solo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving solos");
                return new List<Solo>();
            }
        }

        /// <summary>
        /// Get a solo by ID from the database
        /// </summary>
        public async Task<Solo?> GetSoloByIdAsync(int? id)
        {
            // Handle null or invalid ID
            if (!id.HasValue || id.Value <= 0)
            {
                _logger.LogWarning("Invalid or null solo ID: {Id}", id);
                return null;
            }

            try
            {
                _logger.LogInformation("Fetching solo with ID {Id} from Supabase", id.Value);
                
                // Use PascalCase for table and column names to match database
                string requestUrl = $"{_supabaseUrl}/rest/v1/Solos?Id=eq.{id.Value}&select=*";
                _logger.LogInformation("Request URL: {Url}", requestUrl);
                
                var response = await _httpClient.GetAsync(requestUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Solo response: {Content}", content);
                    var solos = JsonSerializer.Deserialize<List<Solo>>(content, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                    
                    return solos?.FirstOrDefault();
                }
                
                _logger.LogWarning("Failed to fetch solo with ID {Id} from Supabase, status code: {StatusCode}, response: {Response}", 
                    id.Value, response.StatusCode, await response.Content.ReadAsStringAsync());
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving solo with ID {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Get the daily game for a specific date
        /// </summary>
        public async Task<DailyGame?> GetDailyGameAsync(DateTime date)
        {
            try
            {
                string formattedDate = date.ToString("yyyy-MM-dd");
                _logger.LogInformation("Fetching daily game for date {Date} from Supabase", formattedDate);
                
                // Use PascalCase for table and column names to match database
                string requestUrl = $"{_supabaseUrl}/rest/v1/DailyGames?Date=eq.{formattedDate}&select=*";
                _logger.LogInformation("Request URL: {Url}", requestUrl);
                
                var response = await _httpClient.GetAsync(requestUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Daily game response: {Content}", content);
                    var games = JsonSerializer.Deserialize<List<DailyGame>>(content, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                    
                    var game = games?.FirstOrDefault();
                    
                    // Load the associated solo if available
                    if (game != null && game.SoloId.HasValue)
                    {
                        _logger.LogInformation("Loading solo with ID {SoloId} for daily game", game.SoloId.Value);
                        game.Solo = await GetSoloByIdAsync(game.SoloId);
                        
                        if (game.Solo == null)
                        {
                            _logger.LogWarning("Solo with ID {SoloId} not found for daily game", game.SoloId.Value);
                        }
                    }
                    
                    return game;
                }
                
                _logger.LogWarning("Failed to fetch daily game for date {Date}, status code: {StatusCode}, response: {Response}", 
                    formattedDate, response.StatusCode, await response.Content.ReadAsStringAsync());
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily game for date {Date}", date.ToShortDateString());
                return null;
            }
        }

        /// <summary>
        /// Get today's daily game
        /// </summary>
        public async Task<DailyGame?> GetTodaysDailyGameAsync()
        {
            return await GetDailyGameAsync(DateTime.UtcNow.Date);
        }

        /// <summary>
        /// Get recent daily games
        /// </summary>
        public async Task<List<DailyGame>> GetRecentDailyGamesAsync(int days)
        {
            try
            {
                var startDate = DateTime.UtcNow.Date.AddDays(-days);
                string formattedDate = startDate.ToString("yyyy-MM-dd");
                
                _logger.LogInformation("Fetching daily games since {StartDate} from Supabase", formattedDate);
                
                // Use PascalCase for table and column names to match database
                string requestUrl = $"{_supabaseUrl}/rest/v1/DailyGames?Date=gte.{formattedDate}&select=*&order=Date.desc";
                _logger.LogInformation("Request URL: {Url}", requestUrl);
                
                var response = await _httpClient.GetAsync(requestUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Recent daily games response: {Content}", content);
                    var games = JsonSerializer.Deserialize<List<DailyGame>>(content, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                    
                    games = games ?? new List<DailyGame>();
                    
                    // Load the associated solos
                    foreach (var game in games.Where(g => g.SoloId.HasValue))
                    {
                        game.Solo = await GetSoloByIdAsync(game.SoloId);
                    }
                    
                    return games;
                }
                
                _logger.LogWarning("Failed to fetch daily games, status code: {StatusCode}, response: {Response}", 
                    response.StatusCode, await response.Content.ReadAsStringAsync());
                return new List<DailyGame>();
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
                _logger.LogInformation("Creating daily game for date {Date} with solo ID {SoloId}", 
                    dailyGame.Date.ToShortDateString(), dailyGame.SoloId);
                
                // Serialize using PascalCase property names to match database columns
                var jsonData = new
                {
                    Date = dailyGame.Date.ToString("yyyy-MM-dd"),
                    SoloId = dailyGame.SoloId
                };
                
                var jsonContent = JsonSerializer.Serialize(jsonData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                _logger.LogInformation("Create daily game JSON payload: {Content}", jsonContent);
                
                // Add the prefer header to return the created record
                _httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");
                
                // Send POST request to Supabase with correct table name (PascalCase)
                string requestUrl = $"{_supabaseUrl}/rest/v1/DailyGames";
                _logger.LogInformation("Request URL: {Url}", requestUrl);
                
                var response = await _httpClient.PostAsync(requestUrl, content);
                
                // Remove the prefer header after use
                _httpClient.DefaultRequestHeaders.Remove("Prefer");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Create daily game response: {Content}", responseContent);
                    var createdGames = JsonSerializer.Deserialize<List<DailyGame>>(responseContent, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                    
                    var createdGame = createdGames?.FirstOrDefault();
                    
                    if (createdGame != null && createdGame.SoloId.HasValue)
                    {
                        createdGame.Solo = await GetSoloByIdAsync(createdGame.SoloId);
                    }
                    
                    return createdGame;
                }
                
                _logger.LogWarning("Failed to create daily game, status code: {StatusCode}, response: {Response}", 
                    response.StatusCode, await response.Content.ReadAsStringAsync());
                return null;
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
                _logger.LogInformation("Updating daily game ID {Id} to solo ID {SoloId}", 
                    dailyGame.Id, dailyGame.SoloId);
                
                // Create JSON payload with PascalCase property names to match database
                var jsonData = new
                {
                    Date = dailyGame.Date.ToString("yyyy-MM-dd"),
                    SoloId = dailyGame.SoloId
                };
                
                var jsonContent = JsonSerializer.Serialize(jsonData);
                _logger.LogInformation("Update daily game JSON payload: {Content}", jsonContent);
                
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                // Send PATCH request to Supabase with PascalCase table and column names
                string requestUrl = $"{_supabaseUrl}/rest/v1/DailyGames?Id=eq.{dailyGame.Id}";
                _logger.LogInformation("Request URL: {Url}", requestUrl);
                
                var response = await _httpClient.PatchAsync(requestUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Update daily game response: {Content}", responseContent);
                    return true;
                }
                
                _logger.LogWarning("Failed to update daily game, status code: {StatusCode}, response: {Response}", 
                    response.StatusCode, await response.Content.ReadAsStringAsync());
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating daily game");
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
                _logger.LogInformation("Creating new solo '{Title}' by {Artist}", solo.Title, solo.Artist);
                
                // Create JSON payload with PascalCase property names to match database
                var jsonData = new
                {
                    Title = solo.Title,
                    Artist = solo.Artist,
                    SpotifyId = solo.SpotifyId,
                    SoloStartTimeMs = solo.SoloStartTimeMs,
                    SoloEndTimeMs = solo.SoloEndTimeMs,
                    Guitarist = solo.Guitarist,
                    AiHint = solo.AiHint
                };
                
                var jsonContent = JsonSerializer.Serialize(jsonData);
                _logger.LogInformation("Create solo JSON payload: {Content}", jsonContent);
                
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                // Add the prefer header to return the created record
                _httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");
                
                // Send POST request to Supabase with PascalCase table name
                string requestUrl = $"{_supabaseUrl}/rest/v1/Solos";
                _logger.LogInformation("Request URL: {Url}", requestUrl);
                
                var response = await _httpClient.PostAsync(requestUrl, content);
                
                // Remove the prefer header after use
                _httpClient.DefaultRequestHeaders.Remove("Prefer");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Create solo response: {Content}", responseContent);
                    var createdSolos = JsonSerializer.Deserialize<List<Solo>>(responseContent, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                    
                    return createdSolos?.FirstOrDefault();
                }
                
                _logger.LogWarning("Failed to create solo, status code: {StatusCode}, response: {Response}", 
                    response.StatusCode, await response.Content.ReadAsStringAsync());
                return null;
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
                _logger.LogInformation("Updating solo {Id}: '{Title}' by {Artist}", solo.Id, solo.Title, solo.Artist);
                
                // Create JSON payload with PascalCase property names
                var jsonData = new
                {
                    Title = solo.Title,
                    Artist = solo.Artist,
                    SpotifyId = solo.SpotifyId,
                    SoloStartTimeMs = solo.SoloStartTimeMs,
                    SoloEndTimeMs = solo.SoloEndTimeMs,
                    Guitarist = solo.Guitarist,
                    AiHint = solo.AiHint
                };
                
                var jsonContent = JsonSerializer.Serialize(jsonData);
                _logger.LogInformation("Update solo JSON payload: {Content}", jsonContent);
                
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                // Send PATCH request to Supabase with PascalCase table and column names
                string requestUrl = $"{_supabaseUrl}/rest/v1/Solos?Id=eq.{solo.Id}";
                _logger.LogInformation("Request URL: {Url}", requestUrl);
                
                var response = await _httpClient.PatchAsync(requestUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Update solo response: {Content}", responseContent);
                    return true;
                }
                
                _logger.LogWarning("Failed to update solo, status code: {StatusCode}, response: {Response}", 
                    response.StatusCode, await response.Content.ReadAsStringAsync());
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating solo");
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
                _logger.LogInformation("Deleting solo {Id}", id);
                
                // Send DELETE request to Supabase with PascalCase table and column names
                string requestUrl = $"{_supabaseUrl}/rest/v1/Solos?Id=eq.{id}";
                _logger.LogInformation("Request URL: {Url}", requestUrl);
                
                var response = await _httpClient.DeleteAsync(requestUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Delete solo response: {Content}", responseContent);
                    return true;
                }
                
                _logger.LogWarning("Failed to delete solo, status code: {StatusCode}, response: {Response}", 
                    response.StatusCode, await response.Content.ReadAsStringAsync());
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting solo");
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
                _logger.LogInformation("Deleting daily game {Id}", id);
                
                // Send DELETE request to Supabase with PascalCase table and column names
                string requestUrl = $"{_supabaseUrl}/rest/v1/DailyGames?Id=eq.{id}";
                _logger.LogInformation("Request URL: {Url}", requestUrl);
                
                var response = await _httpClient.DeleteAsync(requestUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Delete daily game response: {Content}", responseContent);
                    return true;
                }
                
                _logger.LogWarning("Failed to delete daily game, status code: {StatusCode}, response: {Response}", 
                    response.StatusCode, await response.Content.ReadAsStringAsync());
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting daily game");
                return false;
            }
        }
    }
}