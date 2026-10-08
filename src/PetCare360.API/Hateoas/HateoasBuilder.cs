using System.Globalization;
using PetCare360.Domain.Pagination;

namespace PetCare360.API.Hateoas
{
    public static class HateoasBuilder
    {
        public static Recurso<T> CriarRecurso<T>(T dados, string rotaBase, int id, params Link[] linksExtras)
        {
            var recurso = new Recurso<T>
            {
                Dados = dados,
                Links = new List<Link>
                {
                    new Link($"{rotaBase}/{id}", "self", "GET"),
                    new Link($"{rotaBase}/{id}", "update", "PUT"),
                    new Link($"{rotaBase}/{id}", "delete", "DELETE")
                }
            };

            recurso.Links.AddRange(linksExtras);

            return recurso;
        }

        public static RecursoPaginado<T> CriarRecursoPaginado<T>(
            PagedResult<T> resultado,
            QueryParameters parametros,
            string rotaBase,
            Func<T, Recurso<T>> criarRecurso,
            bool permiteCriacao = true)
        {
            var recursoPaginado = new RecursoPaginado<T>
            {
                Itens = resultado.Itens.Select(criarRecurso).ToList(),
                Pagina = resultado.Pagina,
                TamanhoPagina = resultado.TamanhoPagina,
                TotalItens = resultado.TotalItens,
                TotalPaginas = resultado.TotalPaginas,
                TemPaginaAnterior = resultado.TemPaginaAnterior,
                TemProximaPagina = resultado.TemProximaPagina
            };

            recursoPaginado.Links.Add(new Link(MontarUrl(rotaBase, parametros, resultado.Pagina), "self", "GET"));
            recursoPaginado.Links.Add(new Link(MontarUrl(rotaBase, parametros, 1), "first", "GET"));

            if (resultado.TemPaginaAnterior)
            {
                recursoPaginado.Links.Add(new Link(MontarUrl(rotaBase, parametros, resultado.Pagina - 1), "previous", "GET"));
            }

            if (resultado.TemProximaPagina)
            {
                recursoPaginado.Links.Add(new Link(MontarUrl(rotaBase, parametros, resultado.Pagina + 1), "next", "GET"));
            }

            if (resultado.TotalPaginas > 0)
            {
                recursoPaginado.Links.Add(new Link(MontarUrl(rotaBase, parametros, resultado.TotalPaginas), "last", "GET"));
            }

            if (permiteCriacao)
            {
                recursoPaginado.Links.Add(new Link(rotaBase, "create", "POST"));
            }

            return recursoPaginado;
        }

        private static string MontarUrl(string rotaBase, QueryParameters parametros, int pagina)
        {
            var valores = new List<string> { $"pagina={pagina}" };

            var propriedades = parametros.GetType()
                .GetProperties()
                .Where(p => p.Name != nameof(QueryParameters.Pagina));

            foreach (var propriedade in propriedades)
            {
                var valor = propriedade.GetValue(parametros);
                if (valor == null)
                {
                    continue;
                }

                var nome = char.ToLowerInvariant(propriedade.Name[0]) + propriedade.Name[1..];
                valores.Add($"{nome}={Uri.EscapeDataString(Formatar(valor))}");
            }

            return $"{rotaBase}?{string.Join("&", valores)}";
        }

        private static string Formatar(object valor)
        {
            return valor switch
            {
                DateTime data => data.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                bool booleano => booleano ? "true" : "false",
                _ => Convert.ToString(valor, CultureInfo.InvariantCulture) ?? string.Empty
            };
        }
    }
}