// Services/GameService.cs
using ShredleApi.Models;
using ShredleApi.Data;

namespace ShredleApi.Services
{
    public class GameService
    {
        private readonly SupabaseRepository _repository;

        public GameService(SupabaseRepository repository)
        {
            _repository = repository;
        }

        public async Task<Game?> GetDailyGameAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _repository.GetGameByDateAsync(today);
        }

        public async Task<Game?> GetDailyTestGameAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _repository.GetTestGameByDateAsync(today);
        }
    }
}