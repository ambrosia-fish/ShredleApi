using Microsoft.EntityFrameworkCore;
using ShredleApi.Models;

namespace ShredleApi.Data.Repositories
{
    public class SoloRepository : ISoloRepository
    {
        private readonly ShredleDbContext _context;

        public SoloRepository(ShredleDbContext context)
        {
            _context = context;
        }

        public async Task<Solo?> GetByIdAsync(int id)
        {
            return await _context.Solos.FindAsync(id);
        }

        public async Task<IEnumerable<Solo>> GetAllAsync()
        {
            return await _context.Solos.ToListAsync();
        }

        public async Task<Solo> CreateAsync(Solo solo)
        {
            _context.Solos.Add(solo);
            await _context.SaveChangesAsync();
            return solo;
        }

        public async Task<Solo> UpdateAsync(Solo solo)
        {
            _context.Solos.Update(solo);
            await _context.SaveChangesAsync();
            return solo;
        }

        public async Task DeleteAsync(int id)
        {
            var solo = await _context.Solos.FindAsync(id);
            if (solo != null)
            {
                _context.Solos.Remove(solo);
                await _context.SaveChangesAsync();
            }
        }
    }
}
