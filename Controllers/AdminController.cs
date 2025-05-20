using Microsoft.AspNetCore.Mvc;
using ShredleApi.Models;
using ShredleApi.Services;

namespace ShredleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly SupabaseService _supabaseService;
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;
        private readonly OpenAiService? _openAiService;
        
        public AdminController(
            SupabaseService supabaseService, 
            ILogger<AdminController> logger,
            IConfiguration configuration,
            OpenAiService? openAiService = null)  // Optional to avoid breaking when OpenAI service isn't registered
        {
            _supabaseService = supabaseService;
            _logger = logger;
            _configuration = configuration;
            _openAiService = openAiService;
        }
        
        private bool ValidateAdminKey(string? providedKey)
        {
            // First check environment variable (for Heroku)
            string? environmentAdminKey = Environment.GetEnvironmentVariable("ADMIN_KEY");
            
            // If not found in environment, fall back to configuration (for local development)
            string? configAdminKey = environmentAdminKey ?? _configuration["AdminKey"];
            
            // Log for debugging (don't log the full key in production)
            _logger.LogInformation($"Admin key source: {(environmentAdminKey != null ? "Environment" : "Configuration")}");
            
            // Check if the key is valid
            bool isValid = !string.IsNullOrEmpty(configAdminKey) && providedKey == configAdminKey;
            
            // Log validation result (without revealing the key)
            _logger.LogInformation($"Admin key validation result: {isValid}");
            
            return isValid;
        }

        // New simple endpoint to set the daily solo by ID
        [HttpPost("set-daily-solo")]
        public async Task<IActionResult> SetDailySolo([FromQuery] string adminKey, [FromBody] SetDailySoloRequest request)
        {
            if (!ValidateAdminKey(adminKey))
            {
                return Unauthorized("Invalid admin key");
            }
            
            try
            {
                // Debug log to help diagnose issues
                _logger.LogInformation($"Received request to set daily solo with ID: {request?.soloId}");
                
                // Validate request
                if (request == null || request.soloId <= 0)
                {
                    _logger.LogWarning($"Invalid solo ID: {request?.soloId}");
                    return BadRequest("Valid soloId is required");
                }

                // Check if the solo exists
                var solo = await _supabaseService.GetSoloByIdAsync(request.soloId);
                if (solo == null)
                {
                    _logger.LogWarning($"Solo with ID {request.soloId} not found");
                    return NotFound($"Solo with ID {request.soloId} not found");
                }
                
                // Check if there's already a daily game for today
                var today = DateTime.UtcNow.Date;
                var existingDailyGame = await _supabaseService.GetDailyGameAsync(today);
                
                if (existingDailyGame != null)
                {
                    // Update the existing daily game
                    existingDailyGame.SoloId = request.soloId;
                    var updateSuccess = await _supabaseService.UpdateDailyGameAsync(existingDailyGame);
                    
                    if (!updateSuccess)
                    {
                        _logger.LogError($"Failed to update daily game to solo ID {request.soloId}");
                        return StatusCode(500, "Failed to update daily game");
                    }
                    
                    _logger.LogInformation($"Successfully updated daily solo for {today.ToShortDateString()} to '{solo.Title}' by {solo.Artist}");
                    return Ok($"Successfully updated daily solo for {today.ToShortDateString()} to '{solo.Title}' by {solo.Artist}");
                }
                else
                {
                    // Create a new daily game
                    var newDailyGame = new DailyGame
                    {
                        Date = today,
                        SoloId = request.soloId
                    };
                    
                    var result = await _supabaseService.CreateDailyGameAsync(newDailyGame);
                    
                    if (result == null)
                    {
                        _logger.LogError($"Failed to create daily game with solo ID {request.soloId}");
                        return StatusCode(500, "Failed to create daily game");
                    }
                    
                    _logger.LogInformation($"Successfully set daily solo for {today.ToShortDateString()} to '{solo.Title}' by {solo.Artist}");
                    return Ok($"Successfully set daily solo for {today.ToShortDateString()} to '{solo.Title}' by {solo.Artist}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting daily solo ID {request?.soloId}");
                return StatusCode(500, $"An error occurred while setting the daily solo: {ex.Message}");
            }
        }
    
    }

    // Simple request object for the set-daily-solo endpoint
    public class SetDailySoloRequest
    {
        // Using lowercase property name to match JSON convention for JavaScript
        public int soloId { get; set; }
    }
}