using ShredleApi.Models;

namespace ShredleApi.Data.Repositories
{
    public interface IGameRepository
    {
        Task<Game?> GetByDateAsync(DateTime date);
        Task<Game?> GetByIdAsync(int id);
        Task<Game> CreateAsync(Game game);
        Task<Game> UpdateAsync(Game game);
        Task DeleteAsync(int id);
    }
}
