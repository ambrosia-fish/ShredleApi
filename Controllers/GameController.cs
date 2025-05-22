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
        private readonly IConfiguration _configuration;

        public GameController(GameService gameService, IConfiguration configuration)
        {
            _gameService = gameService;
            _configuration = configuration;
        }

        [HttpGet("daily")]
        public async Task<ActionResult<GameResponse>> GetDailyGame()
        {
            var game = await _gameService.GetDailyGameAsync();
            
            if (game == null)
            {
                return NotFound("No game available for today");
            }

            var response = new GameResponse
            {
                Id = game.Id,
                Date = game.Date,
                SoloId = game.SoloId
            };

            return Ok(response);
        }

        [HttpGet("daily-test")]
        public async Task<ActionResult<GameResponse>> GetDailyTestGame([FromQuery] string passcode)
        {
            var adminKey = _configuration["ADMIN_KEY"];
            
            // DEBUG: Log the values
            Console.WriteLine($"Received passcode: '{passcode}'");
            Console.WriteLine($"Admin key from config: '{adminKey}'");
            Console.WriteLine($"Are they equal? {passcode == adminKey}");
            
            if (string.IsNullOrEmpty(passcode) || passcode != adminKey)
            {
                return Unauthorized("Invalid admin key");
            }

            var game = await _gameService.GetDailyTestGameAsync();
            
            if (game == null)
            {
                return NotFound("No test game available for today");
            }

            var response = new GameResponse
            {
                Id = game.Id,
                Date = game.Date,
                SoloId = game.SoloId
            };

            return Ok(response);
        }
    }
}