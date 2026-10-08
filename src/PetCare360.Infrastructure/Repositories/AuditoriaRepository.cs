using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;
using PetCare360.Domain.Pagination;
using PetCare360.Infrastructure.NoSql;

namespace PetCare360.Infrastructure.Repositories
{
    public class AuditoriaRepository : IAuditoriaRepository
    {
        private readonly IMongoCollection<RegistroAuditoria> _colecao;

        public AuditoriaRepository(IMongoDatabase database, MongoDbSettings settings)
        {
            _colecao = database.GetCollection<RegistroAuditoria>(settings.AuditoriaCollection);
        }

        public async Task AddAsync(RegistroAuditoria registro)
        {
            await _colecao.InsertOneAsync(registro);
        }

        public async Task<PagedResult<RegistroAuditoria>> GetPagedAsync(AuditoriaQueryParameters parametros)
        {
            var filtro = MontarFiltro(parametros);

            var totalItens = await _colecao.CountDocumentsAsync(filtro);

            var busca = _colecao.Find(filtro);
            var ordenada = parametros.Ascendente
                ? busca.SortBy(r => r.DataHora)
                : busca.SortByDescending(r => r.DataHora);

            var itens = await ordenada
                .Skip((parametros.Pagina - 1) * parametros.TamanhoPagina)
                .Limit(parametros.TamanhoPagina)
                .ToListAsync();

            return new PagedResult<RegistroAuditoria>
            {
                Itens = itens,
                Pagina = parametros.Pagina,
                TamanhoPagina = parametros.TamanhoPagina,
                TotalItens = (int)totalItens
            };
        }

        public async Task<IEnumerable<RegistroAuditoria>> GetByEntidadeAsync(string entidade, int entidadeId)
        {
            var builder = Builders<RegistroAuditoria>.Filter;
            var filtro = FiltroEntidade(entidade) & builder.Eq(r => r.EntidadeId, entidadeId);

            return await _colecao.Find(filtro)
                .SortByDescending(r => r.DataHora)
                .ToListAsync();
        }

        private static FilterDefinition<RegistroAuditoria> MontarFiltro(AuditoriaQueryParameters parametros)
        {
            var builder = Builders<RegistroAuditoria>.Filter;
            var filtro = builder.Empty;

            if (!string.IsNullOrWhiteSpace(parametros.Entidade))
            {
                filtro &= FiltroEntidade(parametros.Entidade);
            }

            if (parametros.EntidadeId.HasValue)
            {
                filtro &= builder.Eq(r => r.EntidadeId, parametros.EntidadeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(parametros.Acao))
            {
                filtro &= builder.Eq(r => r.Acao, parametros.Acao.ToUpper());
            }

            if (parametros.DataInicio.HasValue)
            {
                filtro &= builder.Gte(r => r.DataHora, parametros.DataInicio.Value);
            }

            if (parametros.DataFim.HasValue)
            {
                filtro &= builder.Lte(r => r.DataHora, parametros.DataFim.Value);
            }

            return filtro;
        }

        private static FilterDefinition<RegistroAuditoria> FiltroEntidade(string entidade)
        {
            var padrao = new BsonRegularExpression($"^{Regex.Escape(entidade)}$", "i");
            return Builders<RegistroAuditoria>.Filter.Regex(r => r.Entidade, padrao);
        }
    }
}