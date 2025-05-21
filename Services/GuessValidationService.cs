// Services/GuessValidationService.cs
using ShredleApi.Models;
using OpenAI.Chat;

namespace ShredleApi.Services
{
    public class GuessValidationService
    {
        private readonly ChatClient _chatClient;
        private readonly SoloService _soloService;

        public GuessValidationService(ChatClient chatClient, SoloService soloService)
        {
            _chatClient = chatClient;
            _soloService = soloService;
        }

        public async Task<GuessResponse> ValidateGuessAsync(GuessRequest request)
        {
            // Get the actual solo details
            var solo = await _soloService.GetSoloByIdAsync(request.SoloId);
            if (solo == null)
            {
                return new GuessResponse { Correct = false, Attempt = request.Attempt };
            }

            // Use OpenAI to validate the guess
            var isCorrect = await ValidateWithOpenAI(solo.Title, solo.Artist, request.Guess);

            return new GuessResponse 
            { 
                Correct = isCorrect, 
                Attempt = request.Attempt 
            };
        }

        private async Task<bool> ValidateWithOpenAI(string actualTitle, string actualArtist, string userGuess)
        {
            var prompt = $"Does the guess '{userGuess}' match the song '{actualTitle}' by {actualArtist}? " +
                        "Consider language differences, colloquial names, and minor spelling errors. " +
                        "Respond with only 'true' or 'false'.";

            var completion = await _chatClient.CompleteChatAsync(prompt);
            var response = completion.Value.Content[0].Text.Trim().ToLower();

            return response == "true";
        }
    }
}