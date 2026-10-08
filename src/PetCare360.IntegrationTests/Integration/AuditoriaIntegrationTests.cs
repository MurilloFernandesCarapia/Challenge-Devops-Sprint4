using System.Net.Http.Json;
using PetCare360.API.Hateoas;
using PetCare360.Domain.Entities;
using PetCare360.IntegrationTests.FactoryFixture;

namespace PetCare360.IntegrationTests.Integration
{
    [Collection("ApiCollection")]
    public class AuditoriaIntegrationTests
    {
        private readonly HttpClient _client;

        public AuditoriaIntegrationTests(ApiFactoryFixture factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<Tutor> CriarTutorAsync()
        {
            var response = await _client.PostAsJsonAsync("/api/Tutores", new
            {
                nmTutor = "Tutor Auditoria",
                cpf = $"444.555.666-{Random.Shared.Next(10, 99)}",
                email = $"auditoria.{Guid.NewGuid():N}@petcare360.com",
                telefone = "(11) 94444-5555",
                endereco = "Rua do Log, 404"
            });

            return (await response.Content.ReadFromJsonAsync<Tutor>())!;
        }

        [Fact]
        public async Task CriarTutor_DadosValidos_GravaAuditoriaDeCriacao()
        {
            var tutor = await CriarTutorAsync();

            var response = await _client.GetAsync($"/api/Auditoria?entidade=Tutor&entidadeId={tutor.IdTutor}");

            response.EnsureSuccessStatusCode();
            var resultado = await response.Content.ReadFromJsonAsync<RecursoPaginado<RegistroAuditoria>>();
            Assert.NotNull(resultado);
            var registro = Assert.Single(resultado.Itens);
            Assert.Equal(AcaoAuditoria.Criacao, registro.Dados.Acao);
            Assert.Contains(registro.Links, l => l.Rel == "recurso" && l.Href == $"/api/Tutores/{tutor.IdTutor}");
            Assert.DoesNotContain(resultado.Links, l => l.Rel == "create");
        }

        [Fact]
        public async Task CicloDoPet_CriarAtualizarEExcluir_GravaHistoricoCompletoNaAuditoria()
        {
            var tutor = await CriarTutorAsync();
            var criacao = await _client.PostAsJsonAsync("/api/Pets", new
            {
                nmPet = "Auditado",
                especie = "Cachorro",
                raca = "SRD",
                idTutor = tutor.IdTutor
            });
            var pet = await criacao.Content.ReadFromJsonAsync<Pet>();
            await _client.PutAsJsonAsync($"/api/Pets/{pet!.IdPet}", new
            {
                idPet = pet.IdPet,
                nmPet = "Auditado Editado",
                especie = "Cachorro",
                raca = "SRD",
                idTutor = tutor.IdTutor
            });
            await _client.DeleteAsync($"/api/Pets/{pet.IdPet}");

            var response = await _client.GetAsync($"/api/Auditoria/Pet/{pet.IdPet}");

            response.EnsureSuccessStatusCode();
            var historico = await response.Content.ReadFromJsonAsync<List<Recurso<RegistroAuditoria>>>();
            Assert.NotNull(historico);
            var acoes = historico.Select(r => r.Dados.Acao).ToList();
            Assert.Equal(3, acoes.Count);
            Assert.Contains(AcaoAuditoria.Criacao, acoes);
            Assert.Contains(AcaoAuditoria.Atualizacao, acoes);
            Assert.Contains(AcaoAuditoria.Exclusao, acoes);
        }
    }
}