using PetCare360.Domain.Dtos;

namespace PetCare360.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<UsuarioResponse> RegistrarAsync(RegistroUsuarioRequest request);
        Task<TokenResponse> LoginAsync(LoginRequest request);
    }
}