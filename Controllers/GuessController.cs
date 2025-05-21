// Controllers/GuessController.cs
using Microsoft.AspNetCore.Mvc;
using ShredleApi.Models;
using ShredleApi.Services;

namespace ShredleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuessController : ControllerBase
    {
        private readonly GuessValidationService _guessValidationService;

        public GuessController(GuessValidationService guessValidationService)
        {
            _guessValidationService = guessValidationService;
        }

        [HttpPost]
        public async Task<ActionResult<GuessResponse>> ValidateGuess([FromBody] GuessRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _guessValidationService.ValidateGuessAsync(request);
            return Ok(response);
        }
    }
}