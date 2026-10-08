using PetCare360.Domain.Entities;
using PetCare360.Domain.Pagination;

namespace PetCare360.Domain.Interfaces
{
    public interface IClinicaService
    {
        Task<IEnumerable<Clinica>> GetAllAsync();
        Task<PagedResult<Clinica>> GetPagedAsync(ClinicaQueryParameters parametros);
        Task<Clinica?> GetByIdAsync(int id);
        Task<Clinica?> GetByCnpjAsync(string cnpj);
        Task<Clinica> CreateAsync(Clinica clinica);
        Task<bool> UpdateAsync(int id, Clinica clinicaAtualizada);
        Task<bool> DeleteAsync(int id);
    }
}