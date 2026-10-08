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
    public class TutoresController : ControllerBase
    {
        private const string Rota = "/api/Tutores";

        private readonly ITutorService _tutorService;
        private readonly ILogger<TutoresController> _logger;

        public TutoresController(ITutorService tutorService, ILogger<TutoresController> logger)
        {
            _tutorService = tutorService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(RecursoPaginado<Tutor>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] TutorQueryParameters parametros)
        {
            var tutores = await _tutorService.GetPagedAsync(parametros);
            return Ok(HateoasBuilder.CriarRecursoPaginado(tutores, parametros, Rota, CriarRecurso));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Recurso<Tutor>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var tutor = await _tutorService.GetByIdAsync(id);
            if (tutor == null)
            {
                return NotFound("Tutor não encontrado.");
            }
            return Ok(CriarRecurso(tutor));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Tutor tutor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tutorCriado = await _tutorService.CreateAsync(tutor);
            return CreatedAtAction(nameof(GetById), new { id = tutorCriado.IdTutor }, tutorCriado);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Tutor tutorAtualizado)
        {
            if (id != tutorAtualizado.IdTutor)
            {
                return BadRequest("O ID da URL não confere com o ID do corpo da requisição.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool atualizado = await _tutorService.UpdateAsync(id, tutorAtualizado);
            if (!atualizado)
            {
                return NotFound("Tutor não encontrado.");
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
            bool removido = await _tutorService.DeleteAsync(id);
            if (!removido)
            {
                return NotFound("Tutor não encontrado.");
            }

            return NoContent();
        }

        private static Recurso<Tutor> CriarRecurso(Tutor tutor)
        {
            return HateoasBuilder.CriarRecurso(tutor, Rota, tutor.IdTutor,
                new Link($"/api/Pets/tutor/{tutor.IdTutor}", "pets", "GET"));
        }
    }
}