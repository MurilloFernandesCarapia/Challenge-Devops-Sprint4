using PetCare360.Domain.Pagination;

namespace PetCare360.UnitTests.Domain
{
    public class QueryParametersTests
    {
        [Fact]
        public void Construtor_SemValores_UsaValoresPadrao()
        {
            var parametros = new PetQueryParameters();

            var pagina = parametros.Pagina;
            var tamanhoPagina = parametros.TamanhoPagina;

            Assert.Equal(1, pagina);
            Assert.Equal(10, tamanhoPagina);
            Assert.True(parametros.Ascendente);
            Assert.Null(parametros.OrdenarPor);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Pagina_ValorMenorQueUm_AjustaParaPrimeiraPagina(int paginaInformada)
        {
            var parametros = new PetQueryParameters();

            parametros.Pagina = paginaInformada;

            Assert.Equal(1, parametros.Pagina);
        }

        [Fact]
        public void TamanhoPagina_AcimaDoMaximo_LimitaNoMaximoPermitido()
        {
            var parametros = new TutorQueryParameters();

            parametros.TamanhoPagina = 1000;

            Assert.Equal(QueryParameters.TamanhoMaximoPagina, parametros.TamanhoPagina);
        }

        [Fact]
        public void TamanhoPagina_ValorZero_UsaTamanhoPadrao()
        {
            var parametros = new VacinaQueryParameters();

            parametros.TamanhoPagina = 0;

            Assert.Equal(10, parametros.TamanhoPagina);
        }

        [Fact]
        public void FiltrosEspecificos_ValoresInformados_SaoMantidos()
        {
            var inicio = new DateTime(2026, 9, 1);
            var fim = new DateTime(2026, 9, 30);

            var consulta = new ConsultaQueryParameters { IdPet = 1, IdClinica = 2, DataInicio = inicio, DataFim = fim };
            var medicamento = new MedicamentoQueryParameters { Nome = "Amoxicilina", IdPet = 3, EmUso = true };
            var vacina = new VacinaQueryParameters { Nome = "V10", Fabricante = "Zoetis", IdPet = 4, ProximaDoseAte = fim };

            Assert.Equal(1, consulta.IdPet);
            Assert.Equal(2, consulta.IdClinica);
            Assert.Equal(inicio, consulta.DataInicio);
            Assert.Equal(fim, consulta.DataFim);
            Assert.Equal("Amoxicilina", medicamento.Nome);
            Assert.Equal(3, medicamento.IdPet);
            Assert.True(medicamento.EmUso);
            Assert.Equal("V10", vacina.Nome);
            Assert.Equal("Zoetis", vacina.Fabricante);
            Assert.Equal(4, vacina.IdPet);
            Assert.Equal(fim, vacina.ProximaDoseAte);
        }
    }
}