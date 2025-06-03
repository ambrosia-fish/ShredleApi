using ShredleApi.Models;
using ShredleApi.Data.Repositories;

namespace ShredleApi.Services
{
    public class SoloService
    {
        private readonly ISoloRepository _soloRepository;

        public SoloService(ISoloRepository soloRepository)
        {
            _soloRepository = soloRepository;
        }

        public async Task<Solo?> GetSoloByIdAsync(int id)
        {
            return await _soloRepository.GetByIdAsync(id);
        }
    }
}
