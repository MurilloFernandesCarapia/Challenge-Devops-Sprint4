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
    public class ClinicasController : ControllerBase
    {
        private const string Rota = "/api/Clinicas";

        private readonly IClinicaService _clinicaService;
        private readonly ILogger<ClinicasController> _logger;

        public ClinicasController(IClinicaService clinicaService, ILogger<ClinicasController> logger)
        {
            _clinicaService = clinicaService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(RecursoPaginado<Clinica>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] ClinicaQueryParameters parametros)
        {
            var clinicas = await _clinicaService.GetPagedAsync(parametros);
            return Ok(HateoasBuilder.CriarRecursoPaginado(clinicas, parametros, Rota, CriarRecurso));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Recurso<Clinica>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var clinica = await _clinicaService.GetByIdAsync(id);
            if (clinica == null)
            {
                return NotFound("Clínica não encontrada.");
            }
            return Ok(CriarRecurso(clinica));
        }

        [HttpGet("cnpj/{cnpj}")]
        [ProducesResponseType(typeof(Recurso<Clinica>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCnpj(string cnpj)
        {
            var clinica = await _clinicaService.GetByCnpjAsync(cnpj);
            if (clinica == null)
            {
                return NotFound("Clínica não encontrada.");
            }
            return Ok(CriarRecurso(clinica));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Clinica clinica)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clinicaCriada = await _clinicaService.CreateAsync(clinica);
            return CreatedAtAction(nameof(GetById), new { id = clinicaCriada.IdClinica }, clinicaCriada);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Clinica clinicaAtualizada)
        {
            if (id != clinicaAtualizada.IdClinica)
            {
                return BadRequest("O ID da URL não confere com o ID do corpo.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool atualizada = await _clinicaService.UpdateAsync(id, clinicaAtualizada);
            if (!atualizada)
            {
                return NotFound("Clínica não encontrada.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = PerfilUsuario.Admin)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            bool removida = await _clinicaService.DeleteAsync(id);
            if (!removida)
            {
                return NotFound("Clínica não encontrada.");
            }

            return NoContent();
        }

        private static Recurso<Clinica> CriarRecurso(Clinica clinica)
        {
            return HateoasBuilder.CriarRecurso(clinica, Rota, clinica.IdClinica,
                new Link($"/api/Consultas/clinica/{clinica.IdClinica}", "consultas", "GET"));
        }
    }
}