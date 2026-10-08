using System.Net;
using System.Net.Http.Json;
using PetCare360.Domain.Entities;
using PetCare360.IntegrationTests.FactoryFixture;

namespace PetCare360.IntegrationTests.Integration
{
    [Collection("ApiCollection")]
    public class AutorizacaoIntegrationTests
    {
        private readonly ApiFactoryFixture _factory;

        public AutorizacaoIntegrationTests(ApiFactoryFixture factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ListarPets_SemToken_RetornaUnauthorized()
        {
            var clienteAnonimo = _factory.CriarClienteAnonimo();

            var response = await clienteAnonimo.GetAsync("/api/Pets");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task ListarPets_PerfilUsuario_RetornaOk()
        {
            var clienteUsuario = _factory.CriarClienteComPerfil(PerfilUsuario.Usuario);

            var response = await clienteUsuario.GetAsync("/api/Pets");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ExcluirTutor_PerfilUsuario_RetornaForbidden()
        {
            var clienteAdmin = _factory.CreateClient();
            var clienteUsuario = _factory.CriarClienteComPerfil(PerfilUsuario.Usuario);
            var criacao = await clienteAdmin.PostAsJsonAsync("/api/Tutores", new
            {
                nmTutor = "Tutor Protegido",
                cpf = $"777.888.999-{Random.Shared.Next(10, 99)}",
                email = $"protegido.{Guid.NewGuid():N}@petcare360.com",
                telefone = "(11) 97777-8888",
                endereco = "Rua Segura, 403"
            });
            var tutor = await criacao.Content.ReadFromJsonAsync<Tutor>();

            var response = await clienteUsuario.DeleteAsync($"/api/Tutores/{tutor!.IdTutor}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var buscaAposTentativa = await clienteAdmin.GetAsync($"/api/Tutores/{tutor.IdTutor}");
            Assert.Equal(HttpStatusCode.OK, buscaAposTentativa.StatusCode);
        }

        [Fact]
        public async Task ConsultarAuditoria_PerfilUsuario_RetornaForbidden()
        {
            var clienteUsuario = _factory.CriarClienteComPerfil(PerfilUsuario.Usuario);

            var response = await clienteUsuario.GetAsync("/api/Auditoria");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ConsultarAuditoria_PerfilAdmin_RetornaOk()
        {
            var clienteAdmin = _factory.CriarClienteComPerfil(PerfilUsuario.Admin);

            var response = await clienteAdmin.GetAsync("/api/Auditoria");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task HealthLive_SemToken_ContinuaPublico()
        {
            var clienteAnonimo = _factory.CriarClienteAnonimo();

            var response = await clienteAnonimo.GetAsync("/health/live");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}