using Microsoft.EntityFrameworkCore;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;
using PetCare360.Domain.Pagination;
using PetCare360.Infrastructure.Data;
using PetCare360.Infrastructure.Extensions;

namespace PetCare360.Infrastructure.Repositories
{
    public class ClinicaRepository : IClinicaRepository
    {
        private readonly AppDbContext _dbContext;

        public ClinicaRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Clinica>> GetAllAsync()
        {
            return await _dbContext.Clinicas.ToListAsync();
        }

        public async Task<PagedResult<Clinica>> GetPagedAsync(ClinicaQueryParameters parametros)
        {
            var query = _dbContext.Clinicas.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(parametros.Nome))
            {
                var nome = parametros.Nome.ToLower();
                query = query.Where(c => c.NmClinica.ToLower().Contains(nome));
            }

            if (!string.IsNullOrWhiteSpace(parametros.Cnpj))
            {
                var cnpj = parametros.Cnpj;
                query = query.Where(c => c.Cnpj.Contains(cnpj));
            }

            query = parametros.OrdenarPor?.ToLower() switch
            {
                "nome" => query.Ordenar(c => c.NmClinica, parametros.Ascendente),
                _ => query.Ordenar(c => c.IdClinica, parametros.Ascendente)
            };

            return await query.PaginarAsync(parametros);
        }

        public async Task<Clinica?> GetByIdAsync(int id)
        {
            return await _dbContext.Clinicas.FindAsync(id);
        }

        public async Task<Clinica?> GetByCnpjAsync(string cnpj)
        {
            return await _dbContext.Clinicas
                .FirstOrDefaultAsync(c => c.Cnpj == cnpj);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbContext.Clinicas.AnyAsync(c => c.IdClinica == id);
        }

        public async Task AddAsync(Clinica clinica)
        {
            await _dbContext.Clinicas.AddAsync(clinica);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Clinica clinica)
        {
            _dbContext.Clinicas.Update(clinica);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Clinica clinica)
        {
            _dbContext.Clinicas.Remove(clinica);
            await _dbContext.SaveChangesAsync();
        }
    }
}