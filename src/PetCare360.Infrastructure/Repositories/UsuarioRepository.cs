using Microsoft.EntityFrameworkCore;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;
using PetCare360.Infrastructure.Data;

namespace PetCare360.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _dbContext;

        public UsuarioRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _dbContext.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _dbContext.Usuarios.AnyAsync(u => u.Email == email);
        }

        public async Task AddAsync(Usuario usuario)
        {
            await _dbContext.Usuarios.AddAsync(usuario);
            await _dbContext.SaveChangesAsync();
        }
    }
}