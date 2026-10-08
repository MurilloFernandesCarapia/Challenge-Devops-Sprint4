using PetCare360.Domain.Entities;
using PetCare360.Domain.Pagination;

namespace PetCare360.Domain.Interfaces
{
    public interface IClinicaRepository
    {
        Task<IEnumerable<Clinica>> GetAllAsync();
        Task<PagedResult<Clinica>> GetPagedAsync(ClinicaQueryParameters parametros);
        Task<Clinica?> GetByIdAsync(int id);
        Task<Clinica?> GetByCnpjAsync(string cnpj);
        Task<bool> ExistsAsync(int id);
        Task AddAsync(Clinica clinica);
        Task UpdateAsync(Clinica clinica);
        Task DeleteAsync(Clinica clinica);
    }
}