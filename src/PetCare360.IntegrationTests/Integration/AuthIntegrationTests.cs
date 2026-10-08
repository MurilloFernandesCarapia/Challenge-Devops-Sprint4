using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PetCare360.Domain.Dtos;
using PetCare360.Domain.Entities;
using PetCare360.IntegrationTests.FactoryFixture;

namespace PetCare360.IntegrationTests.Integration
{
    [Collection("ApiCollection")]
    public class AuthIntegrationTests
    {
        private const string EmailAdmin = "admin@petcare360.com";
        private const string SenhaAdmin = "Admin@123";

        private readonly HttpClient _client;

        public AuthIntegrationTests(ApiFactoryFixture factory)
        {
            _client = factory.CriarClienteAnonimo();
        }

        [Fact]
        public async Task Login_AdminPadrao_RetornaTokenComPerfilAdmin()
        {
            var credenciais = new { email = EmailAdmin, senha = SenhaAdmin };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", credenciais);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
            Assert.NotNull(token);
            Assert.False(string.IsNullOrWhiteSpace(token.Token));
            Assert.Equal(PerfilUsuario.Admin, token.Perfil);
        }

        [Fact]
        public async Task Login_SenhaErrada_RetornaUnauthorized()
        {
            var credenciais = new { email = EmailAdmin, senha = "senha-errada" };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", credenciais);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task Registrar_EmailNovo_RetornaCreatedComPerfilUsuario()
        {
            var novoUsuario = new
            {
                nome = "Usuário Teste",
                email = $"usuario.{Guid.NewGuid():N}@petcare360.com",
                senha = "senha123"
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/registrar", novoUsuario);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var usuario = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
            Assert.NotNull(usuario);
            Assert.Equal(PerfilUsuario.Usuario, usuario.Perfil);
        }

        [Fact]
        public async Task Registrar_EmailJaCadastrado_RetornaBadRequest()
        {
            var usuarioDuplicado = new { nome = "Outro Admin", email = EmailAdmin, senha = "senha123" };

            var response = await _client.PostAsJsonAsync("/api/Auth/registrar", usuarioDuplicado);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Me_SemToken_RetornaUnauthorized()
        {
            var response = await _client.GetAsync("/api/Auth/me");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Me_ComTokenValido_RetornaDadosDoUsuarioLogado()
        {
            var login = await _client.PostAsJsonAsync("/api/Auth/login", new { email = EmailAdmin, senha = SenhaAdmin });
            var token = await login.Content.ReadFromJsonAsync<TokenResponse>();
            var requisicao = new HttpRequestMessage(HttpMethod.Get, "/api/Auth/me");
            requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token!.Token);

            var response = await _client.SendAsync(requisicao);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var usuario = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
            Assert.NotNull(usuario);
            Assert.Equal(EmailAdmin, usuario.Email);
            Assert.Equal(PerfilUsuario.Admin, usuario.Perfil);
        }
    }
}