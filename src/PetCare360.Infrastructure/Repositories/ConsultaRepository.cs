using Microsoft.EntityFrameworkCore;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;
using PetCare360.Domain.Pagination;
using PetCare360.Infrastructure.Data;
using PetCare360.Infrastructure.Extensions;

namespace PetCare360.Infrastructure.Repositories
{
    public class ConsultaRepository : IConsultaRepository
    {
        private readonly AppDbContext _dbContext;

        public ConsultaRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Consulta>> GetAllAsync()
        {
            return await _dbContext.Consultas.ToListAsync();
        }

        public async Task<PagedResult<Consulta>> GetPagedAsync(ConsultaQueryParameters parametros)
        {
            var query = _dbContext.Consultas.AsNoTracking().AsQueryable();

            if (parametros.IdPet.HasValue)
            {
                query = query.Where(c => c.IdPet == parametros.IdPet.Value);
            }

            if (parametros.IdClinica.HasValue)
            {
                query = query.Where(c => c.IdClinica == parametros.IdClinica.Value);
            }

            if (parametros.DataInicio.HasValue)
            {
                query = query.Where(c => c.DtConsulta >= parametros.DataInicio.Value);
            }

            if (parametros.DataFim.HasValue)
            {
                query = query.Where(c => c.DtConsulta <= parametros.DataFim.Value);
            }

            query = parametros.OrdenarPor?.ToLower() switch
            {
                "data" => query.Ordenar(c => c.DtConsulta, parametros.Ascendente),
                _ => query.Ordenar(c => c.IdConsulta, parametros.Ascendente)
            };

            return await query.PaginarAsync(parametros);
        }

        public async Task<Consulta?> GetByIdAsync(int id)
        {
            return await _dbContext.Consultas.FindAsync(id);
        }

        public async Task<IEnumerable<Consulta>> GetByPetAsync(int petId)
        {
            return await _dbContext.Consultas
                .Where(c => c.IdPet == petId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Consulta>> GetByClinicaAsync(int clinicaId)
        {
            return await _dbContext.Consultas
                .Where(c => c.IdClinica == clinicaId)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbContext.Consultas.AnyAsync(c => c.IdConsulta == id);
        }

        public async Task AddAsync(Consulta consulta)
        {
            await _dbContext.Consultas.AddAsync(consulta);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Consulta consulta)
        {
            _dbContext.Consultas.Update(consulta);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Consulta consulta)
        {
            _dbContext.Consultas.Remove(consulta);
            await _dbContext.SaveChangesAsync();
        }
    }
}