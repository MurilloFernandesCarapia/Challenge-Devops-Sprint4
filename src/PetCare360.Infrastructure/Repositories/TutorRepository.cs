using Microsoft.EntityFrameworkCore;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;
using PetCare360.Domain.Pagination;
using PetCare360.Infrastructure.Data;
using PetCare360.Infrastructure.Extensions;

namespace PetCare360.Infrastructure.Repositories
{
    public class TutorRepository : ITutorRepository
    {
        private readonly AppDbContext _dbContext;

        public TutorRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Tutor>> GetAllAsync()
        {
            return await _dbContext.Tutores.ToListAsync();
        }

        public async Task<PagedResult<Tutor>> GetPagedAsync(TutorQueryParameters parametros)
        {
            var query = _dbContext.Tutores.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(parametros.Nome))
            {
                var nome = parametros.Nome.ToLower();
                query = query.Where(t => t.NmTutor.ToLower().Contains(nome));
            }

            if (!string.IsNullOrWhiteSpace(parametros.Email))
            {
                var email = parametros.Email.ToLower();
                query = query.Where(t => t.Email.ToLower().Contains(email));
            }

            if (!string.IsNullOrWhiteSpace(parametros.Cpf))
            {
                var cpf = parametros.Cpf;
                query = query.Where(t => t.Cpf.Contains(cpf));
            }

            query = parametros.OrdenarPor?.ToLower() switch
            {
                "nome" => query.Ordenar(t => t.NmTutor, parametros.Ascendente),
                "email" => query.Ordenar(t => t.Email, parametros.Ascendente),
                _ => query.Ordenar(t => t.IdTutor, parametros.Ascendente)
            };

            return await query.PaginarAsync(parametros);
        }

        public async Task<Tutor?> GetByIdAsync(int id)
        {
            return await _dbContext.Tutores.FindAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbContext.Tutores.AnyAsync(t => t.IdTutor == id);
        }

        public async Task AddAsync(Tutor tutor)
        {
            await _dbContext.Tutores.AddAsync(tutor);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Tutor tutor)
        {
            _dbContext.Tutores.Update(tutor);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Tutor tutor)
        {
            _dbContext.Tutores.Remove(tutor);
            await _dbContext.SaveChangesAsync();
        }
    }
}