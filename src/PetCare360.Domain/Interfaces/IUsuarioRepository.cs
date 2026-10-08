using PetCare360.Domain.Entities;

namespace PetCare360.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task AddAsync(Usuario usuario);
    }
}