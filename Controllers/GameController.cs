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
    }
}