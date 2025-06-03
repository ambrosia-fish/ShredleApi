using Microsoft.EntityFrameworkCore;
using ShredleApi.Models;

namespace ShredleApi.Data.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly ShredleDbContext _context;

        public GameRepository(ShredleDbContext context)
        {
            _context = context;
        }

        public async Task<Game?> GetByDateAsync(DateTime date)
        {
            return await _context.Games
                .Include(g => g.Solo)
                .FirstOrDefaultAsync(g => g.Date.Date == date.Date);
        }

        public async Task<Game?> GetByIdAsync(int id)
        {
            return await _context.Games
                .Include(g => g.Solo)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<Game> CreateAsync(Game game)
        {
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            return game;
        }

        public async Task<Game> UpdateAsync(Game game)
        {
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
            return game;
        }

        public async Task DeleteAsync(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game != null)
            {
                _context.Games.Remove(game);
                await _context.SaveChangesAsync();
            }
        }
    }
}
