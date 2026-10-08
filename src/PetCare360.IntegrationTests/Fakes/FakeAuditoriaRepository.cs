using System.Collections.Concurrent;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;
using PetCare360.Domain.Pagination;

namespace PetCare360.IntegrationTests.Fakes
{
    public class FakeAuditoriaRepository : IAuditoriaRepository
    {
        private readonly ConcurrentBag<RegistroAuditoria> _registros = new();

        public Task AddAsync(RegistroAuditoria registro)
        {
            registro.Id = Guid.NewGuid().ToString("N");
            _registros.Add(registro);
            return Task.CompletedTask;
        }

        public Task<PagedResult<RegistroAuditoria>> GetPagedAsync(AuditoriaQueryParameters parametros)
        {
            var consulta = _registros.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(parametros.Entidade))
            {
                consulta = consulta.Where(r => r.Entidade.Equals(parametros.Entidade, StringComparison.OrdinalIgnoreCase));
            }

            if (parametros.EntidadeId.HasValue)
            {
                consulta = consulta.Where(r => r.EntidadeId == parametros.EntidadeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(parametros.Acao))
            {
                consulta = consulta.Where(r => r.Acao == parametros.Acao.ToUpper());
            }

            var filtrados = consulta.OrderByDescending(r => r.DataHora).ToList();

            var resultado = new PagedResult<RegistroAuditoria>
            {
                Itens = filtrados
                    .Skip((parametros.Pagina - 1) * parametros.TamanhoPagina)
                    .Take(parametros.TamanhoPagina)
                    .ToList(),
                Pagina = parametros.Pagina,
                TamanhoPagina = parametros.TamanhoPagina,
                TotalItens = filtrados.Count
            };

            return Task.FromResult(resultado);
        }

        public Task<IEnumerable<RegistroAuditoria>> GetByEntidadeAsync(string entidade, int entidadeId)
        {
            IEnumerable<RegistroAuditoria> registros = _registros
                .Where(r => r.Entidade.Equals(entidade, StringComparison.OrdinalIgnoreCase) && r.EntidadeId == entidadeId)
                .OrderByDescending(r => r.DataHora)
                .ToList();

            return Task.FromResult(registros);
        }
    }
}