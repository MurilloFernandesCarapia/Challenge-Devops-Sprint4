using Microsoft.EntityFrameworkCore;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;
using PetCare360.Domain.Pagination;
using PetCare360.Infrastructure.Data;
using PetCare360.Infrastructure.Extensions;

namespace PetCare360.Infrastructure.Repositories
{
    public class VacinaRepository : IVacinaRepository
    {
        private readonly AppDbContext _dbContext;

        public VacinaRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Vacina>> GetAllAsync()
        {
            return await _dbContext.Vacinas.ToListAsync();
        }

        public async Task<PagedResult<Vacina>> GetPagedAsync(VacinaQueryParameters parametros)
        {
            var query = _dbContext.Vacinas.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(parametros.Nome))
            {
                var nome = parametros.Nome.ToLower();
                query = query.Where(v => v.NmVacina.ToLower().Contains(nome));
            }

            if (!string.IsNullOrWhiteSpace(parametros.Fabricante))
            {
                var fabricante = parametros.Fabricante.ToLower();
                query = query.Where(v => v.Fabricante != null && v.Fabricante.ToLower().Contains(fabricante));
            }

            if (parametros.IdPet.HasValue)
            {
                query = query.Where(v => v.IdPet == parametros.IdPet.Value);
            }

            if (parametros.ProximaDoseAte.HasValue)
            {
                query = query.Where(v => v.DtProximaDose != null && v.DtProximaDose <= parametros.ProximaDoseAte.Value);
            }

            query = parametros.OrdenarPor?.ToLower() switch
            {
                "nome" => query.Ordenar(v => v.NmVacina, parametros.Ascendente),
                "dataaplicacao" => query.Ordenar(v => v.DtAplicacao, parametros.Ascendente),
                "proximadose" => query.Ordenar(v => v.DtProximaDose, parametros.Ascendente),
                _ => query.Ordenar(v => v.IdVacina, parametros.Ascendente)
            };

            return await query.PaginarAsync(parametros);
        }

        public async Task<Vacina?> GetByIdAsync(int id)
        {
            return await _dbContext.Vacinas.FindAsync(id);
        }

        public async Task<IEnumerable<Vacina>> GetByPetAsync(int petId)
        {
            return await _dbContext.Vacinas
                .Where(v => v.IdPet == petId)
                .ToListAsync();
        }

        public async Task AddAsync(Vacina vacina)
        {
            await _dbContext.Vacinas.AddAsync(vacina);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Vacina vacina)
        {
            _dbContext.Vacinas.Update(vacina);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Vacina vacina)
        {
            _dbContext.Vacinas.Remove(vacina);
            await _dbContext.SaveChangesAsync();
        }
    }
}