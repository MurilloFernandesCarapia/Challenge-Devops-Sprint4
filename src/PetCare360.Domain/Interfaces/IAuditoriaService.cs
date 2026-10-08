using PetCare360.Domain.Entities;
using PetCare360.Domain.Pagination;

namespace PetCare360.Domain.Interfaces
{
    public interface IAuditoriaService
    {
        Task RegistrarAsync(string entidade, int entidadeId, string acao, string descricao);
        Task<PagedResult<RegistroAuditoria>> GetPagedAsync(AuditoriaQueryParameters parametros);
        Task<IEnumerable<RegistroAuditoria>> GetHistoricoAsync(string entidade, int entidadeId);
    }
}