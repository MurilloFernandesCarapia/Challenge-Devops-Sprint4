using PetCare360.Domain.Entities;
using PetCare360.Domain.Pagination;

namespace PetCare360.Domain.Interfaces
{
    public interface IMedicamentoRepository
    {
        Task<IEnumerable<Medicamento>> GetAllAsync();
        Task<PagedResult<Medicamento>> GetPagedAsync(MedicamentoQueryParameters parametros);
        Task<Medicamento?> GetByIdAsync(int id);
        Task<IEnumerable<Medicamento>> GetByPetAsync(int petId);
        Task AddAsync(Medicamento medicamento);
        Task UpdateAsync(Medicamento medicamento);
        Task DeleteAsync(Medicamento medicamento);
    }
}