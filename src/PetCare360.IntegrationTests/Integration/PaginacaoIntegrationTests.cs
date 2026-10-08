using System.Net.Http.Json;
using PetCare360.API.Hateoas;
using PetCare360.Domain.Entities;
using PetCare360.IntegrationTests.FactoryFixture;

namespace PetCare360.IntegrationTests.Integration
{
    [Collection("ApiCollection")]
    public class PaginacaoIntegrationTests
    {
        private readonly HttpClient _client;

        public PaginacaoIntegrationTests(ApiFactoryFixture factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<int> CriarTutorAsync()
        {
            var response = await _client.PostAsJsonAsync("/api/Tutores", new
            {
                nmTutor = "Tutor Paginação",
                cpf = $"123.456.789-{Random.Shared.Next(10, 99)}",
                email = $"paginacao.{Guid.NewGuid():N}@petcare360.com",
                telefone = "(11) 91111-2222",
                endereco = "Rua da Paginação, 10"
            });

            var tutor = await response.Content.ReadFromJsonAsync<Tutor>();
            return tutor!.IdTutor;
        }

        private async Task<string> CriarPetsAsync(params string[] nomes)
        {
            var idTutor = await CriarTutorAsync();
            var especie = $"Especie-{Guid.NewGuid():N}";

            foreach (var nome in nomes)
            {
                await _client.PostAsJsonAsync("/api/Pets", new
                {
                    nmPet = nome,
                    especie,
                    raca = "SRD",
                    idTutor
                });
            }

            return especie;
        }

        [Fact]
        public async Task ListarPets_TamanhoPaginaDois_RetornaPrimeiraPaginaComDoisItens()
        {
            var especie = await CriarPetsAsync("Pipoca", "Paçoca", "Pudim");

            var response = await _client.GetAsync($"/api/Pets?especie={especie}&pagina=1&tamanhoPagina=2");

            response.EnsureSuccessStatusCode();
            var resultado = await response.Content.ReadFromJsonAsync<RecursoPaginado<Pet>>();
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Itens.Count());
            Assert.Equal(3, resultado.TotalItens);
            Assert.Equal(2, resultado.TotalPaginas);
            Assert.True(resultado.TemProximaPagina);
            Assert.False(resultado.TemPaginaAnterior);
            Assert.Contains(resultado.Links, l => l.Rel == "next" && l.Href.Contains("pagina=2"));
            Assert.DoesNotContain(resultado.Links, l => l.Rel == "previous");
        }

        [Fact]
        public async Task ListarPets_SegundaPagina_RetornaItemRestante()
        {
            var especie = await CriarPetsAsync("Luna", "Mel", "Nick");

            var response = await _client.GetAsync($"/api/Pets?especie={especie}&pagina=2&tamanhoPagina=2");

            response.EnsureSuccessStatusCode();
            var resultado = await response.Content.ReadFromJsonAsync<RecursoPaginado<Pet>>();
            Assert.NotNull(resultado);
            Assert.Single(resultado.Itens);
            Assert.False(resultado.TemProximaPagina);
            Assert.True(resultado.TemPaginaAnterior);
            Assert.Contains(resultado.Links, l => l.Rel == "previous" && l.Href.Contains("pagina=1"));
        }

        [Fact]
        public async Task ListarPets_OrdenadoPorNomeDecrescente_RetornaEmOrdemInversa()
        {
            var especie = await CriarPetsAsync("Beta", "Alfa", "Gama");

            var response = await _client.GetAsync($"/api/Pets?especie={especie}&ordenarPor=nome&ascendente=false");

            response.EnsureSuccessStatusCode();
            var resultado = await response.Content.ReadFromJsonAsync<RecursoPaginado<Pet>>();
            Assert.NotNull(resultado);
            var nomes = resultado.Itens.Select(r => r.Dados.NmPet).ToList();
            Assert.Equal(new List<string> { "Gama", "Beta", "Alfa" }, nomes);
        }

        [Fact]
        public async Task ListarPets_FiltroPorNome_RetornaSomenteCorrespondentes()
        {
            var especie = await CriarPetsAsync("Bolinha", "Bolota", "Rex");

            var response = await _client.GetAsync($"/api/Pets?especie={especie}&nome=bol");

            response.EnsureSuccessStatusCode();
            var resultado = await response.Content.ReadFromJsonAsync<RecursoPaginado<Pet>>();
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.TotalItens);
            Assert.All(resultado.Itens, r => Assert.StartsWith("Bol", r.Dados.NmPet));
        }

        [Fact]
        public async Task ListarTutores_TamanhoPaginaAcimaDoLimite_LimitaEmCinquenta()
        {
            var response = await _client.GetAsync("/api/Tutores?tamanhoPagina=1000");

            response.EnsureSuccessStatusCode();
            var resultado = await response.Content.ReadFromJsonAsync<RecursoPaginado<Tutor>>();
            Assert.NotNull(resultado);
            Assert.Equal(50, resultado.TamanhoPagina);
        }
    }
}