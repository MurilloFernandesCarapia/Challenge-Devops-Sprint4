using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using PetCare360.Domain.Entities;
using PetCare360.Infrastructure.Security;

namespace PetCare360.UnitTests.Security
{
    public class JwtTokenServiceTests
    {
        private readonly JwtTokenService _tokenService;

        public JwtTokenServiceTests()
        {
            var settings = new JwtSettings
            {
                Key = "chave-de-teste-com-pelo-menos-32-caracteres!!",
                Issuer = "PetCare360.Testes",
                Audience = "PetCare360.Clientes",
                ExpiracaoMinutos = 30
            };

            _tokenService = new JwtTokenService(Options.Create(settings));
        }

        [Fact]
        public void GerarToken_UsuarioAdmin_GeraTokenComPerfilEEmissor()
        {
            var usuario = new Usuario { IdUsuario = 1, NmUsuario = "Admin", Email = "admin@petcare360.com", Perfil = PerfilUsuario.Admin };

            var resposta = _tokenService.GerarToken(usuario);

            var token = new JwtSecurityTokenHandler().ReadJwtToken(resposta.Token);
            Assert.Equal("PetCare360.Testes", token.Issuer);
            Assert.Contains("PetCare360.Clientes", token.Audiences);
            Assert.Contains(token.Claims, c => c.Value == PerfilUsuario.Admin);
            Assert.Contains(token.Claims, c => c.Value == "admin@petcare360.com");
        }

        [Fact]
        public void GerarToken_ExpiracaoConfigurada_DefineDataDeExpiracaoNoFuturo()
        {
            var usuario = new Usuario { IdUsuario = 2, NmUsuario = "Ana", Email = "ana@email.com", Perfil = PerfilUsuario.Usuario };

            var resposta = _tokenService.GerarToken(usuario);

            Assert.True(resposta.ExpiraEm > DateTime.UtcNow.AddMinutes(29));
            Assert.True(resposta.ExpiraEm <= DateTime.UtcNow.AddMinutes(30));
            Assert.Equal(PerfilUsuario.Usuario, resposta.Perfil);
        }
    }
}