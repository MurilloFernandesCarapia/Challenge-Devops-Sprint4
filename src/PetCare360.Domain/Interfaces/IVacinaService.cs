using PetCare360.Domain.Entities;
using PetCare360.Domain.Pagination;

namespace PetCare360.Domain.Interfaces
{
    public interface IVacinaService
    {
        Task<IEnumerable<Vacina>> GetAllAsync();
        Task<PagedResult<Vacina>> GetPagedAsync(VacinaQueryParameters parametros);
        Task<Vacina?> GetByIdAsync(int id);
        Task<IEnumerable<Vacina>> GetByPetAsync(int petId);
        Task<Vacina> CreateAsync(Vacina vacina);
        Task<bool> UpdateAsync(int id, Vacina vacinaAtualizada);
        Task<bool> DeleteAsync(int id);
    }
}