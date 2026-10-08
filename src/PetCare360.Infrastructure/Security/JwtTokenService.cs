using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PetCare360.Domain.Dtos;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;

namespace PetCare360.Infrastructure.Security
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public TokenResponse GerarToken(Usuario usuario)
        {
            var expiraEm = DateTime.UtcNow.AddMinutes(_settings.ExpiracaoMinutos);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, usuario.NmUsuario),
                new Claim(ClaimTypes.Role, usuario.Perfil)
            };

            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expiraEm,
                signingCredentials: credenciais);

            var tokenGerado = new JwtSecurityTokenHandler().WriteToken(token);

            return new TokenResponse(tokenGerado, expiraEm, usuario.NmUsuario, usuario.Email, usuario.Perfil);
        }
    }
}