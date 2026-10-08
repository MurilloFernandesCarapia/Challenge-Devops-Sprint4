using PetCare360.Domain.Entities;
using PetCare360.Domain.Pagination;

namespace PetCare360.Domain.Interfaces
{
    public interface ITutorService
    {
        Task<IEnumerable<Tutor>> GetAllAsync();
        Task<PagedResult<Tutor>> GetPagedAsync(TutorQueryParameters parametros);
        Task<Tutor?> GetByIdAsync(int id);
        Task<Tutor> CreateAsync(Tutor tutor);
        Task<bool> UpdateAsync(int id, Tutor tutorAtualizado);
        Task<bool> DeleteAsync(int id);
    }
}