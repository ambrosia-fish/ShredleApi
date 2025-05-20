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
        private readonly OpenAiService _openAiService;
        private readonly ILogger<GameController> _logger;
        private const int MaxGuesses = 4;

        public GameController(
            SupabaseService supabaseService, 
            OpenAiService openAiService,
            ILogger<GameController> logger)
        {
            _supabaseService = supabaseService;
            _openAiService = openAiService;
            _logger = logger;
        }

        [HttpGet("daily")]
        public async Task<ActionResult<GameStateResponse>> GetDailySolo([FromQuery] int guessCount = 0)
        {
            try
            {
                _logger.LogInformation("Getting daily solo with guessCount: {GuessCount}", guessCount);
                var dailyGame = await _supabaseService.GetTodaysDailyGameAsync();
                
                // If no daily game exists for today, create one on the fly
                if (dailyGame == null)
                {
                    _logger.LogInformation("No daily game found for today, creating one");
                    dailyGame = await CreateDailySoloIfMissing();
                    
                    if (dailyGame == null)
                    {
                        _logger.LogWarning("Failed to create daily solo");
                        return NotFound("No daily solo is available for today");
                    }
                }

                // Check if Solo is already loaded
                if (dailyGame.Solo == null && dailyGame.SoloId.HasValue)
                {
                    // If not loaded, try to load it
                    _logger.LogInformation("Loading solo ID {SoloId} for daily game", dailyGame.SoloId.Value);
                    dailyGame.Solo = await _supabaseService.GetSoloByIdAsync(dailyGame.SoloId);
                }

                var solo = dailyGame.Solo;
                if (solo == null)
                {
                    _logger.LogWarning("Solo not found for daily game");
                    return NotFound("Solo not found");
                }

                _logger.LogInformation("Returning daily solo: '{Title}' by {Artist}", solo.Title, solo.Artist);
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
                
                // If no daily game exists, create one
                if (dailyGame == null)
                {
                    dailyGame = await CreateDailySoloIfMissing();
                    
                    if (dailyGame == null)
                    {
                        return NotFound("No daily solo is available for today");
                    }
                }

                // Ensure Solo is loaded
                if (dailyGame.Solo == null && dailyGame.SoloId.HasValue)
                {
                    dailyGame.Solo = await _supabaseService.GetSoloByIdAsync(dailyGame.SoloId);
                }

                var solo = dailyGame.Solo;
                if (solo == null)
                {
                    return NotFound("Solo not found");
                }

                int currentGuessCount = previousGuessCount + 1;
                
                // Check if the guess is correct using AI-powered comparison
                _logger.LogInformation($"Checking guess: '{request.SongGuess}' against correct title: '{solo.Title}'");
                bool isCorrect = await _openAiService.CheckGuessCorrectness(request.SongGuess, solo.Title, solo.Artist);
                _logger.LogInformation($"Guess result: {(isCorrect ? "Correct" : "Incorrect")}");

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

        // Helper for string normalization if needed
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

        private async Task<DailyGame?> CreateDailySoloIfMissing()
        {
            try
            {
                // Get all solos
                var allSolos = await _supabaseService.GetSolosAsync();
                if (allSolos.Count == 0)
                {
                    _logger.LogWarning("No solos found in the database to create a daily solo");
                    return null;
                }
                
                // Get all previous daily games to avoid duplicates
                var recentDailyGames = await _supabaseService.GetRecentDailyGamesAsync(30); // Last 30 days
                var recentSoloIds = recentDailyGames.Select(g => g.SoloId).ToHashSet();
                
                // Filter out recently used solos
                var availableSolos = allSolos.Where(s => !recentSoloIds.Contains(s.Id)).ToList();
                
                if (availableSolos.Count == 0)
                {
                    // If all solos were recently used, just use all solos
                    _logger.LogInformation("All solos have been used recently, selecting from all solos");
                    availableSolos = allSolos;
                }
                
                // Select a random solo
                Random random = new Random();
                var selectedSolo = availableSolos[random.Next(availableSolos.Count)];
                
                // Create new daily game
                var today = DateTime.UtcNow.Date;
                var newDailyGame = new DailyGame
                {
                    Date = today,
                    SoloId = selectedSolo.Id
                };
                
                _logger.LogInformation($"Creating new daily solo: {selectedSolo.Title} by {selectedSolo.Artist} for {today.ToShortDateString()}");
                
                // Save to database
                var createdGame = await _supabaseService.CreateDailyGameAsync(newDailyGame);
                
                // Make sure the solo is loaded
                if (createdGame != null && createdGame.SoloId.HasValue && createdGame.Solo == null)
                {
                    createdGame.Solo = selectedSolo; // Use the solo we already have
                }
                
                return createdGame;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating daily solo on the fly");
                return null;
            }
        }
    }
}