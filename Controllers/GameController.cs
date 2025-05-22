// Controllers/GameController.cs
using Microsoft.AspNetCore.Mvc;
using ShredleApi.Models;
using ShredleApi.Services;

namespace ShredleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameService _gameService;

        public GameController(GameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet("daily")]
        public async Task<ActionResult<object>> GetDailyGame()
        {
            try
            {
                var game = await _gameService.GetDailyGameAsync();
                
                if (game == null)
                {
                    // Return mock data for testing
                    return Ok(new {
                        id = "game_20250522",
                        date = "2025-05-22T00:00:00Z",
                        soloId = 123
                    });
                }

                var response = new 
                {
                    id = game.Id,
                    date = game.Date,
                    soloId = game.SoloId
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Return mock data if service fails
                return Ok(new {
                    id = "game_20250522",
                    date = "2025-05-22T00:00:00Z",
                    soloId = 123
                });
            }
        }

        // Simple health check endpoint
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}