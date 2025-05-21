// Services/SoloService.cs
using ShredleApi.Models;
using ShredleApi.Data;

namespace ShredleApi.Services
{
    public class SoloService
    {
        private readonly SupabaseRepository _repository;

        public SoloService(SupabaseRepository repository)
        {
            _repository = repository;
        }

        public async Task<Solo?> GetSoloByIdAsync(int id)
        {
            return await _repository.GetSoloByIdAsync(id);
        }
    }
}