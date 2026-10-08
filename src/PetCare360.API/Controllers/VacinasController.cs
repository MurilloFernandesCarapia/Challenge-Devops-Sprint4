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
    public class VacinasController : ControllerBase
    {
        private const string Rota = "/api/Vacinas";

        private readonly IVacinaService _vacinaService;
        private readonly ILogger<VacinasController> _logger;

        public VacinasController(IVacinaService vacinaService, ILogger<VacinasController> logger)
        {
            _vacinaService = vacinaService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(RecursoPaginado<Vacina>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] VacinaQueryParameters parametros)
        {
            var vacinas = await _vacinaService.GetPagedAsync(parametros);
            return Ok(HateoasBuilder.CriarRecursoPaginado(vacinas, parametros, Rota, CriarRecurso));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Recurso<Vacina>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var vacina = await _vacinaService.GetByIdAsync(id);
            if (vacina == null)
            {
                return NotFound("Vacina não encontrada.");
            }
            return Ok(CriarRecurso(vacina));
        }

        [HttpGet("pet/{petId}")]
        [ProducesResponseType(typeof(IEnumerable<Recurso<Vacina>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByPet(int petId)
        {
            var vacinas = await _vacinaService.GetByPetAsync(petId);
            return Ok(vacinas.Select(CriarRecurso));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Vacina vacina)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vacinaCriada = await _vacinaService.CreateAsync(vacina);
            return CreatedAtAction(nameof(GetById), new { id = vacinaCriada.IdVacina }, vacinaCriada);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Vacina vacinaAtualizada)
        {
            if (id != vacinaAtualizada.IdVacina)
            {
                return BadRequest("O ID da URL não confere com o ID do corpo.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool atualizada = await _vacinaService.UpdateAsync(id, vacinaAtualizada);
            if (!atualizada)
            {
                return NotFound("Vacina não encontrada.");
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
            bool removida = await _vacinaService.DeleteAsync(id);
            if (!removida)
            {
                return NotFound("Vacina não encontrada.");
            }

            return NoContent();
        }

        private static Recurso<Vacina> CriarRecurso(Vacina vacina)
        {
            var recurso = HateoasBuilder.CriarRecurso(vacina, Rota, vacina.IdVacina,
                new Link($"/api/Pets/{vacina.IdPet}", "pet", "GET"));

            if (vacina.IdConsulta.HasValue)
            {
                recurso.Links.Add(new Link($"/api/Consultas/{vacina.IdConsulta}", "consulta", "GET"));
            }

            return recurso;
        }
    }
}