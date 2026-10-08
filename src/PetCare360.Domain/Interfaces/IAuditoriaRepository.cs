using PetCare360.Domain.Entities;
using PetCare360.Domain.Pagination;

namespace PetCare360.Domain.Interfaces
{
    public interface IAuditoriaRepository
    {
        Task AddAsync(RegistroAuditoria registro);
        Task<PagedResult<RegistroAuditoria>> GetPagedAsync(AuditoriaQueryParameters parametros);
        Task<IEnumerable<RegistroAuditoria>> GetByEntidadeAsync(string entidade, int entidadeId);
    }
}