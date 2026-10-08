using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PetCare360.Domain.Pagination;

namespace PetCare360.Infrastructure.Extensions
{
    
    public static class QueryableExtensions
    {
        public static IOrderedQueryable<T> Ordenar<T, TKey>(
            this IQueryable<T> query,
            Expression<Func<T, TKey>> campo,
            bool ascendente)
        {
            return ascendente ? query.OrderBy(campo) : query.OrderByDescending(campo);
        }

        public static async Task<PagedResult<T>> PaginarAsync<T>(
            this IQueryable<T> query,
            QueryParameters parametros)
        {
            
            var totalItens = await query.CountAsync();

            
            var itens = await query
                .Skip((parametros.Pagina - 1) * parametros.TamanhoPagina)
                .Take(parametros.TamanhoPagina)
                .ToListAsync();

            return new PagedResult<T>
            {
                Itens = itens,
                Pagina = parametros.Pagina,
                TamanhoPagina = parametros.TamanhoPagina,
                TotalItens = totalItens
            };
        }
    }
}