using Microsoft.Extensions.Logging;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;
using PetCare360.Domain.Pagination;

namespace PetCare360.Application.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly IAuditoriaRepository _auditoriaRepository;
        private readonly ILogger<AuditoriaService> _logger;

        public AuditoriaService(IAuditoriaRepository auditoriaRepository, ILogger<AuditoriaService> logger)
        {
            _auditoriaRepository = auditoriaRepository;
            _logger = logger;
        }

        public async Task RegistrarAsync(string entidade, int entidadeId, string acao, string descricao)
        {
            var registro = new RegistroAuditoria
            {
                Entidade = entidade,
                EntidadeId = entidadeId,
                Acao = acao,
                Descricao = descricao,
                DataHora = DateTime.UtcNow
            };

            try
            {
                await _auditoriaRepository.AddAsync(registro);
                _logger.LogInformation("Auditoria registrada: {Acao} em {Entidade} {EntidadeId}", acao, entidade, entidadeId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Não foi possível registrar a auditoria de {Acao} em {Entidade} {EntidadeId}", acao, entidade, entidadeId);
            }
        }

        public async Task<PagedResult<RegistroAuditoria>> GetPagedAsync(AuditoriaQueryParameters parametros)
        {
            return await _auditoriaRepository.GetPagedAsync(parametros);
        }

        public async Task<IEnumerable<RegistroAuditoria>> GetHistoricoAsync(string entidade, int entidadeId)
        {
            return await _auditoriaRepository.GetByEntidadeAsync(entidade, entidadeId);
        }
    }
}