using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare360.API.Hateoas;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;
using PetCare360.Domain.Pagination;

namespace PetCare360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class MedicamentosController : ControllerBase
    {
        private const string Rota = "/api/Medicamentos";

        private readonly IMedicamentoService _medicamentoService;
        private readonly ILogger<MedicamentosController> _logger;

        public MedicamentosController(IMedicamentoService medicamentoService, ILogger<MedicamentosController> logger)
        {
            _medicamentoService = medicamentoService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(RecursoPaginado<Medicamento>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] MedicamentoQueryParameters parametros)
        {
            var medicamentos = await _medicamentoService.GetPagedAsync(parametros);
            return Ok(HateoasBuilder.CriarRecursoPaginado(medicamentos, parametros, Rota, CriarRecurso));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Recurso<Medicamento>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var medicamento = await _medicamentoService.GetByIdAsync(id);
            if (medicamento == null)
            {
                return NotFound("Medicamento não encontrado.");
            }
            return Ok(CriarRecurso(medicamento));
        }

        [HttpGet("pet/{petId}")]
        [ProducesResponseType(typeof(IEnumerable<Recurso<Medicamento>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByPet(int petId)
        {
            var medicamentos = await _medicamentoService.GetByPetAsync(petId);
            return Ok(medicamentos.Select(CriarRecurso));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Medicamento medicamento)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var medicamentoCriado = await _medicamentoService.CreateAsync(medicamento);
            return CreatedAtAction(nameof(GetById), new { id = medicamentoCriado.IdMedicamento }, medicamentoCriado);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Medicamento medicamentoAtualizado)
        {
            if (id != medicamentoAtualizado.IdMedicamento)
            {
                return BadRequest("O ID da URL não confere com o ID do corpo.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool atualizado = await _medicamentoService.UpdateAsync(id, medicamentoAtualizado);
            if (!atualizado)
            {
                return NotFound("Medicamento não encontrado.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = PerfilUsuario.Admin)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            bool removido = await _medicamentoService.DeleteAsync(id);
            if (!removido)
            {
                return NotFound("Medicamento não encontrado.");
            }

            return NoContent();
        }

        private static Recurso<Medicamento> CriarRecurso(Medicamento medicamento)
        {
            var recurso = HateoasBuilder.CriarRecurso(medicamento, Rota, medicamento.IdMedicamento,
                new Link($"/api/Pets/{medicamento.IdPet}", "pet", "GET"));

            if (medicamento.IdConsulta.HasValue)
            {
                recurso.Links.Add(new Link($"/api/Consultas/{medicamento.IdConsulta}", "consulta", "GET"));
            }

            return recurso;
        }
    }
}