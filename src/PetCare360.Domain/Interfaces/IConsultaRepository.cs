using PetCare360.Domain.Entities;
using PetCare360.Domain.Pagination;

namespace PetCare360.Domain.Interfaces
{
    public interface IConsultaRepository
    {
        Task<IEnumerable<Consulta>> GetAllAsync();
        Task<PagedResult<Consulta>> GetPagedAsync(ConsultaQueryParameters parametros);
        Task<Consulta?> GetByIdAsync(int id);
        Task<IEnumerable<Consulta>> GetByPetAsync(int petId);
        Task<IEnumerable<Consulta>> GetByClinicaAsync(int clinicaId);
        Task<bool> ExistsAsync(int id);
        Task AddAsync(Consulta consulta);
        Task UpdateAsync(Consulta consulta);
        Task DeleteAsync(Consulta consulta);
    }
}