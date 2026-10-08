using System.Net.Http.Json;
using PetCare360.API.Hateoas;
using PetCare360.Domain.Entities;
using PetCare360.IntegrationTests.FactoryFixture;

namespace PetCare360.IntegrationTests.Integration
{
    [Collection("ApiCollection")]
    public class HateoasIntegrationTests
    {
        private readonly HttpClient _client;

        public HateoasIntegrationTests(ApiFactoryFixture factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<Tutor> CriarTutorAsync()
        {
            var response = await _client.PostAsJsonAsync("/api/Tutores", new
            {
                nmTutor = "Tutor HATEOAS",
                cpf = $"222.333.444-{Random.Shared.Next(10, 99)}",
                email = $"hateoas.{Guid.NewGuid():N}@petcare360.com",
                telefone = "(11) 93333-4444",
                endereco = "Rua dos Links, 3"
            });

            return (await response.Content.ReadFromJsonAsync<Tutor>())!;
        }

        private async Task<Clinica> CriarClinicaAsync(string nome)
        {
            var response = await _client.PostAsJsonAsync("/api/Clinicas", new
            {
                nmClinica = nome,
                cnpj = $"11.222.333/0001-{Random.Shared.Next(10, 99)}",
                endereco = "Av. dos Links, 100",
                telefone = "(11) 3333-0000",
                email = "contato@clinicahateoas.com"
            });

            return (await response.Content.ReadFromJsonAsync<Clinica>())!;
        }

        [Fact]
        public async Task BuscarTutor_TutorExistente_RetornaLinkParaOsPetsDoTutor()
        {
            var tutor = await CriarTutorAsync();

            var response = await _client.GetAsync($"/api/Tutores/{tutor.IdTutor}");

            response.EnsureSuccessStatusCode();
            var recurso = await response.Content.ReadFromJsonAsync<Recurso<Tutor>>();
            Assert.NotNull(recurso);
            Assert.Equal(tutor.IdTutor, recurso.Dados.IdTutor);
            Assert.Contains(recurso.Links, l => l.Rel == "self" && l.Href == $"/api/Tutores/{tutor.IdTutor}");
            Assert.Contains(recurso.Links, l => l.Rel == "pets" && l.Href == $"/api/Pets/tutor/{tutor.IdTutor}");
        }

        [Fact]
        public async Task BuscarConsulta_ConsultaExistente_RetornaLinksParaPetEClinica()
        {
            var tutor = await CriarTutorAsync();
            var clinica = await CriarClinicaAsync("Clínica Consulta HATEOAS");
            var petResponse = await _client.PostAsJsonAsync("/api/Pets", new
            {
                nmPet = "Link",
                especie = "Gato",
                raca = "SRD",
                idTutor = tutor.IdTutor
            });
            var pet = await petResponse.Content.ReadFromJsonAsync<Pet>();
            var consultaResponse = await _client.PostAsJsonAsync("/api/Consultas", new
            {
                dtConsulta = "2026-09-10T10:00:00",
                descricao = "Retorno",
                diagnostico = "Saudável",
                idPet = pet!.IdPet,
                idClinica = clinica.IdClinica
            });
            var consulta = await consultaResponse.Content.ReadFromJsonAsync<Consulta>();

            var response = await _client.GetAsync($"/api/Consultas/{consulta!.IdConsulta}");

            response.EnsureSuccessStatusCode();
            var recurso = await response.Content.ReadFromJsonAsync<Recurso<Consulta>>();
            Assert.NotNull(recurso);
            Assert.Contains(recurso.Links, l => l.Rel == "pet" && l.Href == $"/api/Pets/{pet.IdPet}");
            Assert.Contains(recurso.Links, l => l.Rel == "clinica" && l.Href == $"/api/Clinicas/{clinica.IdClinica}");
        }

        [Fact]
        public async Task ListarClinicas_ComResultado_CadaItemTrazSeusLinks()
        {
            var nome = $"Clínica {Guid.NewGuid():N}";
            var clinica = await CriarClinicaAsync(nome);

            var response = await _client.GetAsync($"/api/Clinicas?nome={Uri.EscapeDataString(nome)}");

            response.EnsureSuccessStatusCode();
            var resultado = await response.Content.ReadFromJsonAsync<RecursoPaginado<Clinica>>();
            Assert.NotNull(resultado);
            var item = Assert.Single(resultado.Itens);
            Assert.Contains(item.Links, l => l.Rel == "consultas" && l.Href == $"/api/Consultas/clinica/{clinica.IdClinica}");
            Assert.Contains(resultado.Links, l => l.Rel == "create" && l.Method == "POST");
        }
    }
}