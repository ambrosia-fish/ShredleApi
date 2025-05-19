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
                _logger.LogInformation("Getting hardcoded solos list (skipping API call)");
                return GetHardcodedSolos(); 
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
            _logger.LogInformation($"Getting hardcoded solo {id} (skipping API call)");
            return GetHardcodedSolo(id);
        }
        
        /// <summary>
        /// Provides hardcoded solo data list for testing
        /// </summary>
        private List<Solo> GetHardcodedSolos()
        {
            return new List<Solo> 
            {
                new Solo
                {
                    Id = 0,
                    Title = "While My Guitar Gently Weeps",
                    Artist = "The Beatles",
                    SpotifyId = "2EEaNpFdykm5yYlkR3izeE",
                    SoloStartTimeMs = 220000,
                    SoloEndTimeMs = 270000,
                    Guitarist = "Eric Clapton",
                    AiHint = "During the Crying of my Axe"
                },
                new Solo
                {
                    Id = 1,
                    Title = "Stairway to Heaven",
                    Artist = "Led Zeppelin",
                    SpotifyId = "5CQ30WqJwcep0pYcV4AMNc", 
                    SoloStartTimeMs = 334000,
                    SoloEndTimeMs = 404000,
                    Guitarist = "Jimmy Page",
                    AiHint = "Broken Escalator to Hell"
                }
            };
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
                    SoloStartTimeMs = 334000,
                    SoloEndTimeMs = 404000,
                    Guitarist = "Jimmy Page",
                    AiHint = "Broken Escalator to Hell"
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
                    SoloStartTimeMs = 220000,
                    SoloEndTimeMs = 270000,
                    Guitarist = "Eric Clapton",
                    AiHint = "During the Crying of my Axe"
                };
            }
            
            return null;
        }

        /// <summary>
        /// Get the daily game for a specific date - hardcoded for now
        /// </summary>
        public async Task<DailyGame?> GetDailyGameAsync(DateTime date)
        {
            _logger.LogInformation($"Getting hardcoded daily game (skipping API call)");
            return GetHardcodedDailyGame();
        }

        /// <summary>
        /// Get today's daily game
        /// </summary>
        public async Task<DailyGame?> GetTodaysDailyGameAsync()
        {
            _logger.LogInformation("Getting hardcoded daily game for today (skipping API call)");
            return GetHardcodedDailyGame();
        }

        /// <summary>
        /// Get a hardcoded daily game for testing
        /// </summary>
        private DailyGame GetHardcodedDailyGame()
        {
            return new DailyGame
            {
                Id = 1,
                Date = DateTime.UtcNow.Date,
                SoloId = 1,
                Solo = GetHardcodedSolo(1)
            };
        }

        /// <summary>
        /// Get recent daily games
        /// </summary>
        public async Task<List<DailyGame>> GetRecentDailyGamesAsync(int days)
        {
            _logger.LogInformation("Getting hardcoded recent daily games (skipping API call)");
            return new List<DailyGame> { GetHardcodedDailyGame() };
        }

        /// <summary>
        /// Create a new daily game
        /// </summary>
        public async Task<DailyGame?> CreateDailyGameAsync(DailyGame dailyGame)
        {
            _logger.LogInformation("Creating hardcoded daily game (skipping API call)");
            return GetHardcodedDailyGame();
        }

        /// <summary>
        /// Update an existing daily game
        /// </summary>
        public async Task<bool> UpdateDailyGameAsync(DailyGame dailyGame)
        {
            _logger.LogInformation("Updating hardcoded daily game (skipping API call)");
            return true;
        }

        /// <summary>
        /// Create a new solo
        /// </summary>
        public async Task<Solo?> CreateSoloAsync(Solo solo)
        {
            _logger.LogInformation("Creating hardcoded solo (skipping API call)");
            return GetHardcodedSolo(1);
        }

        /// <summary>
        /// Update an existing solo
        /// </summary>
        public async Task<bool> UpdateSoloAsync(Solo solo)
        {
            _logger.LogInformation("Updating hardcoded solo (skipping API call)");
            return true;
        }

        /// <summary>
        /// Delete a solo
        /// </summary>
        public async Task<bool> DeleteSoloAsync(int id)
        {
            _logger.LogInformation("Deleting hardcoded solo (skipping API call)");
            return true;
        }

        /// <summary>
        /// Delete a daily game
        /// </summary>
        public async Task<bool> DeleteDailyGameAsync(int id)
        {
            _logger.LogInformation("Deleting hardcoded daily game (skipping API call)");
            return true;
        }
    }
}