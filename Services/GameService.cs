using ShredleApi.Models;
using ShredleApi.Data.Repositories;

namespace ShredleApi.Services
{
    public class GameService
    {
        private readonly IGameRepository _gameRepository;

        public GameService(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<Game?> GetDailyGameAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _gameRepository.GetByDateAsync(today);
        }
    }
}
