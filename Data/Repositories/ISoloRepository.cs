using ShredleApi.Models;

namespace ShredleApi.Data.Repositories
{
    public interface ISoloRepository
    {
        Task<Solo?> GetByIdAsync(int id);
        Task<IEnumerable<Solo>> GetAllAsync();
        Task<Solo> CreateAsync(Solo solo);
        Task<Solo> UpdateAsync(Solo solo);
        Task DeleteAsync(int id);
    }
}
