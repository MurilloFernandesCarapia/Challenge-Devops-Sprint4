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
    public class ConsultasController : ControllerBase
    {
        private const string Rota = "/api/Consultas";

        private readonly IConsultaService _consultaService;
        private readonly ILogger<ConsultasController> _logger;

        public ConsultasController(IConsultaService consultaService, ILogger<ConsultasController> logger)
        {
            _consultaService = consultaService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(RecursoPaginado<Consulta>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] ConsultaQueryParameters parametros)
        {
            var consultas = await _consultaService.GetPagedAsync(parametros);
            return Ok(HateoasBuilder.CriarRecursoPaginado(consultas, parametros, Rota, CriarRecurso));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Recurso<Consulta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var consulta = await _consultaService.GetByIdAsync(id);
            if (consulta == null)
            {
                return NotFound("Consulta não encontrada.");
            }
            return Ok(CriarRecurso(consulta));
        }

        [HttpGet("pet/{petId}")]
        [ProducesResponseType(typeof(IEnumerable<Recurso<Consulta>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByPet(int petId)
        {
            var consultas = await _consultaService.GetByPetAsync(petId);
            return Ok(consultas.Select(CriarRecurso));
        }

        [HttpGet("clinica/{clinicaId}")]
        [ProducesResponseType(typeof(IEnumerable<Recurso<Consulta>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByClinica(int clinicaId)
        {
            var consultas = await _consultaService.GetByClinicaAsync(clinicaId);
            return Ok(consultas.Select(CriarRecurso));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Consulta consulta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var consultaCriada = await _consultaService.CreateAsync(consulta);
            return CreatedAtAction(nameof(GetById), new { id = consultaCriada.IdConsulta }, consultaCriada);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Consulta consultaAtualizada)
        {
            if (id != consultaAtualizada.IdConsulta)
            {
                return BadRequest("O ID da URL não confere com o ID do corpo.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool atualizada = await _consultaService.UpdateAsync(id, consultaAtualizada);
            if (!atualizada)
            {
                return NotFound("Consulta não encontrada.");
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
            bool removida = await _consultaService.DeleteAsync(id);
            if (!removida)
            {
                return NotFound("Consulta não encontrada.");
            }

            return NoContent();
        }

        private static Recurso<Consulta> CriarRecurso(Consulta consulta)
        {
            return HateoasBuilder.CriarRecurso(consulta, Rota, consulta.IdConsulta,
                new Link($"/api/Pets/{consulta.IdPet}", "pet", "GET"),
                new Link($"/api/Clinicas/{consulta.IdClinica}", "clinica", "GET"));
        }
    }
}