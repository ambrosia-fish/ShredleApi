using Microsoft.AspNetCore.Mvc;
using ShredleApi.DTOs;
using ShredleApi.Models;
using ShredleApi.Services;
using System.Text.RegularExpressions;

namespace ShredleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly SupabaseService _supabaseService;
        private readonly ILogger<GameController> _logger;
        private const int MaxGuesses = 4;

        public GameController(SupabaseService supabaseService, ILogger<GameController> logger)
        {
            _supabaseService = supabaseService;
            _logger = logger;
        }

        [HttpGet("daily")]
        public async Task<ActionResult<GameStateResponse>> GetDailySolo([FromQuery] int guessCount = 0)
        {
            try
            {
                var dailyGame = await _supabaseService.GetTodaysDailyGameAsync();
                
                if (dailyGame == null)
                {
                    return NotFound("No daily solo is available for today");
                }

                var solo = await _supabaseService.GetSoloByIdAsync(dailyGame.SoloId);
                
                if (solo == null)
                {
                    return NotFound("Solo not found");
                }

                return Ok(CreateGameStateResponse(dailyGame, solo, guessCount, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily solo");
                return StatusCode(500, "An error occurred while retrieving the daily solo");
            }
        }

        [HttpPost("guess")]
        public async Task<ActionResult<GameStateResponse>> MakeGuess([FromBody] GuessRequest request, [FromQuery] int previousGuessCount = 0)
        {
            if (string.IsNullOrWhiteSpace(request.SongGuess))
            {
                return BadRequest("Guess cannot be empty");
            }

            try
            {
                var dailyGame = await _supabaseService.GetTodaysDailyGameAsync();
                
                if (dailyGame == null)
                {
                    return NotFound("No daily solo is available for today");
                }

                var solo = await _supabaseService.GetSoloByIdAsync(dailyGame.SoloId);
                
                if (solo == null)
                {
                    return NotFound("Solo not found");
                }

                int currentGuessCount = previousGuessCount + 1;
                
                // Check if the guess is correct (case insensitive, ignore punctuation)
                bool isCorrect = NormalizeForComparison(request.SongGuess) == 
                                 NormalizeForComparison(solo.Title);

                return Ok(CreateGameStateResponse(dailyGame, solo, currentGuessCount, isCorrect));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing guess");
                return StatusCode(500, "An error occurred while processing your guess");
            }
        }

        private GameStateResponse CreateGameStateResponse(DailyGame dailyGame, Solo solo, int guessCount, bool isCorrect)
        {
            var soloResponse = new SoloResponse
            {
                Id = solo.Id,
                SpotifyId = solo.SpotifyId,
                SoloStartTimeMs = solo.SoloStartTimeMs,
                SoloEndTimeMs = solo.SoloEndTimeMs,
                IsCorrect = isCorrect,
                GuessCount = guessCount
            };

            // Determine what to reveal based on guess count
            if (isCorrect || guessCount >= MaxGuesses)
            {
                // Reveal everything on correct guess or max attempts reached
                soloResponse.Title = solo.Title;
                soloResponse.Artist = solo.Artist;
                soloResponse.Guitarist = solo.Guitarist;
                soloResponse.AiHint = solo.AiHint;
                soloResponse.ClipDurationMs = solo.SoloEndTimeMs - solo.SoloStartTimeMs;
                soloResponse.RevealGuitarist = true;
                soloResponse.RevealHint = true;
            }
            else
            {
                // Progressive reveal based on guess count
                switch (guessCount)
                {
                    case 0:
                        // Initial view
                        soloResponse.ClipDurationMs = 3000; // 3 seconds
                        break;
                    case 1:
                        // First incorrect guess
                        soloResponse.ClipDurationMs = (int)((solo.SoloEndTimeMs - solo.SoloStartTimeMs) * 0.25); // 25%
                        break;
                    case 2:
                        // Second incorrect guess
                        soloResponse.ClipDurationMs = (int)((solo.SoloEndTimeMs - solo.SoloStartTimeMs) * 0.66); // 66%
                        soloResponse.Guitarist = solo.Guitarist;
                        soloResponse.RevealGuitarist = true;
                        break;
                    case 3:
                        // Third incorrect guess
                        soloResponse.ClipDurationMs = solo.SoloEndTimeMs - solo.SoloStartTimeMs; // 100%
                        soloResponse.Guitarist = solo.Guitarist;
                        soloResponse.AiHint = solo.AiHint;
                        soloResponse.RevealGuitarist = true;
                        soloResponse.RevealHint = true;
                        break;
                }
            }

            return new GameStateResponse
            {
                Date = dailyGame.Date,
                CurrentSolo = soloResponse,
                IsComplete = isCorrect || guessCount >= MaxGuesses,
                AttemptsRemaining = Math.Max(0, MaxGuesses - guessCount)
            };
        }

        private string NormalizeForComparison(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
                
            // Remove punctuation, convert to lowercase
            string normalized = Regex.Replace(input, @"[^\w\s]", "").ToLower();
            // Remove extra spaces
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
            return normalized;
        }
    }
}