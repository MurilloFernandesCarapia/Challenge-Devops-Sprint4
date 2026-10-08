using Microsoft.EntityFrameworkCore;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;
using PetCare360.Domain.Pagination;
using PetCare360.Infrastructure.Data;
using PetCare360.Infrastructure.Extensions;

namespace PetCare360.Infrastructure.Repositories
{
    public class PetRepository : IPetRepository
    {
        private readonly AppDbContext _dbContext;

        public PetRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Pet>> GetAllAsync()
        {
            return await _dbContext.Pets.ToListAsync();
        }

        public async Task<PagedResult<Pet>> GetPagedAsync(PetQueryParameters parametros)
        {
            
            var query = _dbContext.Pets.AsNoTracking().AsQueryable();

            
            if (!string.IsNullOrWhiteSpace(parametros.Nome))
            {
                var nome = parametros.Nome.ToLower();
                query = query.Where(p => p.NmPet.ToLower().Contains(nome));
            }

            if (!string.IsNullOrWhiteSpace(parametros.Especie))
            {
                var especie = parametros.Especie.ToLower();
                query = query.Where(p => p.Especie.ToLower() == especie);
            }

            if (!string.IsNullOrWhiteSpace(parametros.Raca))
            {
                var raca = parametros.Raca.ToLower();
                query = query.Where(p => p.Raca != null && p.Raca.ToLower().Contains(raca));
            }

            if (parametros.IdTutor.HasValue)
            {
                query = query.Where(p => p.IdTutor == parametros.IdTutor.Value);
            }

            
            query = parametros.OrdenarPor?.ToLower() switch
            {
                "nome" => query.Ordenar(p => p.NmPet, parametros.Ascendente),
                "especie" => query.Ordenar(p => p.Especie, parametros.Ascendente),
                "peso" => query.Ordenar(p => p.Peso, parametros.Ascendente),
                "datanascimento" => query.Ordenar(p => p.DtNascimento, parametros.Ascendente),
                _ => query.Ordenar(p => p.IdPet, parametros.Ascendente)
            };

            
            return await query.PaginarAsync(parametros);
        }

        public async Task<Pet?> GetByIdAsync(int id)
        {
            return await _dbContext.Pets.FindAsync(id);
        }

        public async Task<IEnumerable<Pet>> GetByTutorAsync(int tutorId)
        {
            return await _dbContext.Pets
                .Where(p => p.IdTutor == tutorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pet>> GetByEspecieAsync(string especie)
        {
            return await _dbContext.Pets
                .Where(p => p.Especie.ToLower() == especie.ToLower())
                .ToListAsync();
        }

        public async Task<Pet?> GetHistoricoAsync(int id)
        {
            return await _dbContext.Pets
                .Include(p => p.Consultas)
                .Include(p => p.Vacinas)
                .Include(p => p.Medicamentos)
                .FirstOrDefaultAsync(p => p.IdPet == id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbContext.Pets.AnyAsync(p => p.IdPet == id);
        }

        public async Task AddAsync(Pet pet)
        {
            await _dbContext.Pets.AddAsync(pet);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Pet pet)
        {
            _dbContext.Pets.Update(pet);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Pet pet)
        {
            _dbContext.Pets.Remove(pet);
            await _dbContext.SaveChangesAsync();
        }
    }
}