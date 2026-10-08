using Microsoft.EntityFrameworkCore;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;
using PetCare360.Domain.Pagination;
using PetCare360.Infrastructure.Data;
using PetCare360.Infrastructure.Extensions;

namespace PetCare360.Infrastructure.Repositories
{
    public class MedicamentoRepository : IMedicamentoRepository
    {
        private readonly AppDbContext _dbContext;

        public MedicamentoRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Medicamento>> GetAllAsync()
        {
            return await _dbContext.Medicamentos.ToListAsync();
        }

        public async Task<PagedResult<Medicamento>> GetPagedAsync(MedicamentoQueryParameters parametros)
        {
            var query = _dbContext.Medicamentos.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(parametros.Nome))
            {
                var nome = parametros.Nome.ToLower();
                query = query.Where(m => m.NmMedicamento.ToLower().Contains(nome));
            }

            if (parametros.IdPet.HasValue)
            {
                query = query.Where(m => m.IdPet == parametros.IdPet.Value);
            }

            if (parametros.EmUso.HasValue)
            {
                var hoje = DateTime.Today;
                query = parametros.EmUso.Value
                    ? query.Where(m => m.DtFim == null || m.DtFim >= hoje)
                    : query.Where(m => m.DtFim != null && m.DtFim < hoje);
            }

            query = parametros.OrdenarPor?.ToLower() switch
            {
                "nome" => query.Ordenar(m => m.NmMedicamento, parametros.Ascendente),
                "datainicio" => query.Ordenar(m => m.DtInicio, parametros.Ascendente),
                _ => query.Ordenar(m => m.IdMedicamento, parametros.Ascendente)
            };

            return await query.PaginarAsync(parametros);
        }

        public async Task<Medicamento?> GetByIdAsync(int id)
        {
            return await _dbContext.Medicamentos.FindAsync(id);
        }

        public async Task<IEnumerable<Medicamento>> GetByPetAsync(int petId)
        {
            return await _dbContext.Medicamentos
                .Where(m => m.IdPet == petId)
                .ToListAsync();
        }

        public async Task AddAsync(Medicamento medicamento)
        {
            await _dbContext.Medicamentos.AddAsync(medicamento);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Medicamento medicamento)
        {
            _dbContext.Medicamentos.Update(medicamento);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Medicamento medicamento)
        {
            _dbContext.Medicamentos.Remove(medicamento);
            await _dbContext.SaveChangesAsync();
        }
    }
}