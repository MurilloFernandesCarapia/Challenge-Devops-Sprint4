using PetCare360.Domain.Pagination;

namespace PetCare360.UnitTests.Domain
{
    public class PagedResultTests
    {
        [Theory]
        [InlineData(25, 10, 3)]
        [InlineData(20, 10, 2)]
        [InlineData(1, 50, 1)]
        [InlineData(0, 10, 0)]
        public void TotalPaginas_TotalItensETamanhoPagina_CalculaQuantidadeDePaginas(int totalItens, int tamanhoPagina, int paginasEsperadas)
        {
            var resultado = new PagedResult<string>
            {
                Pagina = 1,
                TamanhoPagina = tamanhoPagina,
                TotalItens = totalItens
            };

            var totalPaginas = resultado.TotalPaginas;

            Assert.Equal(paginasEsperadas, totalPaginas);
        }

        [Fact]
        public void Navegacao_PaginaIntermediaria_TemPaginaAnteriorEProxima()
        {
            var resultado = new PagedResult<string>
            {
                Pagina = 2,
                TamanhoPagina = 10,
                TotalItens = 25
            };

            var temAnterior = resultado.TemPaginaAnterior;
            var temProxima = resultado.TemProximaPagina;

            Assert.True(temAnterior);
            Assert.True(temProxima);
        }

        [Fact]
        public void Navegacao_UltimaPagina_NaoTemProximaPagina()
        {
            var resultado = new PagedResult<string>
            {
                Pagina = 3,
                TamanhoPagina = 10,
                TotalItens = 25
            };

            var temAnterior = resultado.TemPaginaAnterior;
            var temProxima = resultado.TemProximaPagina;

            Assert.True(temAnterior);
            Assert.False(temProxima);
        }
    }
}