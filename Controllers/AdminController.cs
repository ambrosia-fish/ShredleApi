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
            string? configAdminKey = _configuration["AdminKey"];
            return !string.IsNullOrEmpty(configAdminKey) && providedKey == configAdminKey;
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
        
        [HttpPost("update-daily-solo")]
        public async Task<IActionResult> UpdateDailySolo([FromQuery] string adminKey, [FromQuery] int? specificSoloId = null)
        {
            if (!ValidateAdminKey(adminKey))
            {
                return Unauthorized("Invalid admin key");
            }
            
            try
            {
                // Check if there's already a solo for today
                var today = DateTime.UtcNow.Date;
                var existingDailyGame = await _supabaseService.GetDailyGameAsync(today);
                
                if (existingDailyGame != null)
                {
                    // If a specific solo ID is provided and different from the current one, update
                    if (specificSoloId.HasValue && specificSoloId.Value != existingDailyGame.SoloId)
                    {
                        var solo = await _supabaseService.GetSoloByIdAsync(specificSoloId.Value);
                        if (solo == null)
                        {
                            return NotFound($"Solo with ID {specificSoloId.Value} not found");
                        }
                        
                        existingDailyGame.SoloId = specificSoloId.Value;
                        var updateSuccess = await _supabaseService.UpdateDailyGameAsync(existingDailyGame);
                        
                        if (!updateSuccess)
                        {
                            return StatusCode(500, "Failed to update daily game");
                        }
                        
                        return Ok($"Successfully updated daily solo for {today.ToShortDateString()} to '{solo.Title}' by {solo.Artist}");
                    }
                    else
                    {
                        return Ok($"Daily solo already exists for {today.ToShortDateString()} and no change was requested");
                    }
                }
                
                // Get the solo to use (either specific or random)
                Solo? selectedSolo = null;
                
                if (specificSoloId.HasValue)
                {
                    selectedSolo = await _supabaseService.GetSoloByIdAsync(specificSoloId.Value);
                    if (selectedSolo == null)
                    {
                        return NotFound($"Solo with ID {specificSoloId.Value} not found");
                    }
                }
                else
                {
                    // Get all solos
                    var allSolos = await _supabaseService.GetSolosAsync();
                    if (allSolos.Count == 0)
                    {
                        return NotFound("No solos found in the database");
                    }
                    
                    // Get all previous daily games to avoid duplicates
                    var recentDailyGames = await _supabaseService.GetRecentDailyGamesAsync(30); // Last 30 days
                    var recentSoloIds = recentDailyGames.Select(g => g.SoloId).ToHashSet();
                    
                    // Filter out recently used solos
                    var availableSolos = allSolos.Where(s => !recentSoloIds.Contains(s.Id)).ToList();
                    
                    if (availableSolos.Count == 0)
                    {
                        // If all solos were recently used, just use all solos
                        availableSolos = allSolos;
                    }
                    
                    // Select a random solo
                    Random random = new Random();
                    selectedSolo = availableSolos[random.Next(availableSolos.Count)];
                }
                
                // Create new daily game
                var newDailyGame = new DailyGame
                {
                    Date = today,
                    SoloId = selectedSolo.Id
                };
                
                // Save to database
                var result = await _supabaseService.CreateDailyGameAsync(newDailyGame);
                
                if (result == null)
                {
                    return StatusCode(500, "Failed to create daily game");
                }
                
                // Generate hint using OpenAI if available and solo doesn't have a hint
                if (_openAiService != null && string.IsNullOrEmpty(selectedSolo.AiHint))
                {
                    try
                    {
                        var hint = await _openAiService.GenerateHint(selectedSolo.Title, selectedSolo.Artist, selectedSolo.Guitarist);
                        selectedSolo.AiHint = hint;
                        await _supabaseService.UpdateSoloAsync(selectedSolo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to generate hint for solo {selectedSolo.Id}, but daily solo was set successfully");
                    }
                }
                
                return Ok($"Successfully set daily solo for {today.ToShortDateString()} to '{selectedSolo.Title}' by {selectedSolo.Artist}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating daily solo");
                return StatusCode(500, $"An error occurred while updating the daily solo: {ex.Message}");
            }
        }
        
        [HttpGet("solos")]
        public async Task<ActionResult<List<Solo>>> GetAllSolos([FromQuery] string adminKey)
        {
            if (!ValidateAdminKey(adminKey))
            {
                return Unauthorized("Invalid admin key");
            }
            
            try
            {
                var solos = await _supabaseService.GetSolosAsync();
                return Ok(solos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving solos");
                return StatusCode(500, "An error occurred while retrieving solos");
            }
        }
        
        [HttpGet("solos/{id}")]
        public async Task<ActionResult<Solo>> GetSoloById([FromRoute] int id, [FromQuery] string adminKey)
        {
            if (!ValidateAdminKey(adminKey))
            {
                return Unauthorized("Invalid admin key");
            }
            
            try
            {
                var solo = await _supabaseService.GetSoloByIdAsync(id);
                
                if (solo == null)
                {
                    return NotFound($"Solo with ID {id} not found");
                }
                
                return Ok(solo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving solo {id}");
                return StatusCode(500, "An error occurred while retrieving the solo");
            }
        }
        
        [HttpPost("solos")]
        public async Task<ActionResult<Solo>> CreateSolo([FromBody] Solo solo, [FromQuery] string adminKey)
        {
            if (!ValidateAdminKey(adminKey))
            {
                return Unauthorized("Invalid admin key");
            }
            
            if (string.IsNullOrEmpty(solo.Title) || string.IsNullOrEmpty(solo.Artist) || string.IsNullOrEmpty(solo.SpotifyId))
            {
                return BadRequest("Title, Artist, and SpotifyId are required");
            }
            
            try
            {
                // Generate hint automatically if OpenAI service is available
                if (_openAiService != null && string.IsNullOrEmpty(solo.AiHint) && !string.IsNullOrEmpty(solo.Guitarist))
                {
                    try
                    {
                        solo.AiHint = await _openAiService.GenerateHint(solo.Title, solo.Artist, solo.Guitarist);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to generate hint for new solo, continuing with creation");
                    }
                }
                
                var result = await _supabaseService.CreateSoloAsync(solo);
                
                if (result == null)
                {
                    return StatusCode(500, "Failed to create solo");
                }
                
                return CreatedAtAction(nameof(GetSoloById), new { id = result.Id, adminKey }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating solo");
                return StatusCode(500, "An error occurred while creating the solo");
            }
        }
        
        [HttpPut("solos/{id}")]
        public async Task<IActionResult> UpdateSolo([FromRoute] int id, [FromBody] Solo solo, [FromQuery] string adminKey)
        {
            if (!ValidateAdminKey(adminKey))
            {
                return Unauthorized("Invalid admin key");
            }
            
            if (id != solo.Id)
            {
                return BadRequest("ID in URL must match ID in request body");
            }
            
            try
            {
                var existingSolo = await _supabaseService.GetSoloByIdAsync(id);
                
                if (existingSolo == null)
                {
                    return NotFound($"Solo with ID {id} not found");
                }
                
                var success = await _supabaseService.UpdateSoloAsync(solo);
                
                if (!success)
                {
                    return StatusCode(500, "Failed to update solo");
                }
                
                return Ok(solo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating solo {id}");
                return StatusCode(500, "An error occurred while updating the solo");
            }
        }
        
        [HttpDelete("solos/{id}")]
        public async Task<IActionResult> DeleteSolo([FromRoute] int id, [FromQuery] string adminKey)
        {
            if (!ValidateAdminKey(adminKey))
            {
                return Unauthorized("Invalid admin key");
            }
            
            try
            {
                var existingSolo = await _supabaseService.GetSoloByIdAsync(id);
                
                if (existingSolo == null)
                {
                    return NotFound($"Solo with ID {id} not found");
                }
                
                var success = await _supabaseService.DeleteSoloAsync(id);
                
                if (!success)
                {
                    return StatusCode(500, "Failed to delete solo");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting solo {id}");
                return StatusCode(500, "An error occurred while deleting the solo");
            }
        }
        
        [HttpGet("daily-games")]
        public async Task<ActionResult<List<DailyGame>>> GetRecentDailyGames([FromQuery] int days = 7, [FromQuery] string adminKey)
        {
            if (!ValidateAdminKey(adminKey))
            {
                return Unauthorized("Invalid admin key");
            }
            
            try
            {
                var dailyGames = await _supabaseService.GetRecentDailyGamesAsync(days);
                return Ok(dailyGames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily games");
                return StatusCode(500, "An error occurred while retrieving daily games");
            }
        }
        
        [HttpDelete("daily-games/{id}")]
        public async Task<IActionResult> DeleteDailyGame([FromRoute] int id, [FromQuery] string adminKey)
        {
            if (!ValidateAdminKey(adminKey))
            {
                return Unauthorized("Invalid admin key");
            }
            
            try
            {
                var success = await _supabaseService.DeleteDailyGameAsync(id);
                
                if (!success)
                {
                    return StatusCode(500, "Failed to delete daily game");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting daily game {id}");
                return StatusCode(500, "An error occurred while deleting the daily game");
            }
        }
        
        [HttpPost("generate-hint")]
        public async Task<IActionResult> GenerateHint([FromQuery] int soloId, [FromQuery] string adminKey)
        {
            if (!ValidateAdminKey(adminKey))
            {
                return Unauthorized("Invalid admin key");
            }
            
            if (_openAiService == null)
            {
                return BadRequest("OpenAI service is not configured");
            }
            
            try
            {
                var solo = await _supabaseService.GetSoloByIdAsync(soloId);
                
                if (solo == null)
                {
                    return NotFound($"Solo with ID {soloId} not found");
                }
                
                var hint = await _openAiService.GenerateHint(solo.Title, solo.Artist, solo.Guitarist);
                
                // Update the solo with the new hint
                solo.AiHint = hint;
                var success = await _supabaseService.UpdateSoloAsync(solo);
                
                if (!success)
                {
                    return StatusCode(500, "Failed to update solo with hint");
                }
                
                return Ok(new { hint, solo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating hint for solo {soloId}");
                return StatusCode(500, "An error occurred while generating hint");
            }
        }
        
        [HttpPost("bulk-generate-hints")]
        public async Task<IActionResult> BulkGenerateHints([FromQuery] string adminKey)
        {
            if (!ValidateAdminKey(adminKey))
            {
                return Unauthorized("Invalid admin key");
            }
            
            if (_openAiService == null)
            {
                return BadRequest("OpenAI service is not configured");
            }
            
            try
            {
                var solos = await _supabaseService.GetSolosAsync();
                var solosWithoutHints = solos.Where(s => string.IsNullOrEmpty(s.AiHint) && !string.IsNullOrEmpty(s.Guitarist)).ToList();
                
                if (solosWithoutHints.Count == 0)
                {
                    return Ok("All solos already have hints");
                }
                
                var results = new List<object>();
                
                foreach (var solo in solosWithoutHints)
                {
                    try
                    {
                        var hint = await _openAiService.GenerateHint(solo.Title, solo.Artist, solo.Guitarist);
                        
                        solo.AiHint = hint;
                        var success = await _supabaseService.UpdateSoloAsync(solo);
                        
                        results.Add(new 
                        { 
                            id = solo.Id, 
                            title = solo.Title, 
                            hint, 
                            success 
                        });
                        
                        // Add a small delay to avoid hitting rate limits
                        await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error generating hint for solo {solo.Id}");
                        results.Add(new 
                        { 
                            id = solo.Id, 
                            title = solo.Title, 
                            error = ex.Message, 
                            success = false 
                        });
                    }
                }
                
                return Ok(new 
                { 
                    total = solosWithoutHints.Count, 
                    processed = results.Count, 
                    results 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk generating hints");
                return StatusCode(500, "An error occurred while bulk generating hints");
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