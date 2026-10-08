using PetCare360.Domain.Dtos;
using PetCare360.Domain.Entities;

namespace PetCare360.Domain.Interfaces
{
    public interface ITokenService
    {
        TokenResponse GerarToken(Usuario usuario);
    }
}