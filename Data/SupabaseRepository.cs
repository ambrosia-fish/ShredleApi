// Data/SupabaseRepository.cs
using ShredleApi.Models;
using Supabase;

namespace ShredleApi.Data
{
    public class SupabaseRepository
    {
        private readonly Client _supabaseClient;

        public SupabaseRepository(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<Game?> GetGameByDateAsync(DateTime date)
        {
            var response = await _supabaseClient
                .From<Game>()
                .Where(g => g.Date == date)
                .Single();

            return response;
        }

        public async Task<Game?> GetTestGameByDateAsync(DateTime date)
        {
            var response = await _supabaseClient
                .From<GameTest>()
                .Where(g => g.Date == date)
                .Single();

            // Convert GameTest to Game for consistent return type
            if (response == null) return null;
            
            return new Game
            {
                Id = response.Id,
                Date = response.Date,
                SoloId = response.SoloId
            };
        }

        public async Task<Solo?> GetSoloByIdAsync(int id)
        {
            var response = await _supabaseClient
                .From<Solo>()
                .Where(s => s.Id == id)
                .Single();

            return response;
        }
    }
}