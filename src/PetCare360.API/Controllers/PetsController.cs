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
    public class PetsController : ControllerBase
    {
        private const string Rota = "/api/Pets";

        private readonly IPetService _petService;
        private readonly ILogger<PetsController> _logger;

        public PetsController(IPetService petService, ILogger<PetsController> logger)
        {
            _petService = petService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(RecursoPaginado<Pet>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PetQueryParameters parametros)
        {
            var pets = await _petService.GetPagedAsync(parametros);
            return Ok(HateoasBuilder.CriarRecursoPaginado(pets, parametros, Rota, CriarRecurso));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Recurso<Pet>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var pet = await _petService.GetByIdAsync(id);
            if (pet == null)
            {
                return NotFound("Pet não encontrado.");
            }
            return Ok(CriarRecurso(pet));
        }

        [HttpGet("tutor/{tutorId}")]
        [ProducesResponseType(typeof(IEnumerable<Recurso<Pet>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByTutor(int tutorId)
        {
            var pets = await _petService.GetByTutorAsync(tutorId);
            return Ok(pets.Select(CriarRecurso));
        }

        [HttpGet("especie/{especie}")]
        [ProducesResponseType(typeof(IEnumerable<Recurso<Pet>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByEspecie(string especie)
        {
            var pets = await _petService.GetByEspecieAsync(especie);
            return Ok(pets.Select(CriarRecurso));
        }

        [HttpGet("{id}/historico")]
        [ProducesResponseType(typeof(Recurso<Pet>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHistorico(int id)
        {
            var pet = await _petService.GetHistoricoAsync(id);
            if (pet == null)
            {
                return NotFound("Pet não encontrado.");
            }
            return Ok(CriarRecurso(pet));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Pet pet)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var petCriado = await _petService.CreateAsync(pet);
            return CreatedAtAction(nameof(GetById), new { id = petCriado.IdPet }, petCriado);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Pet petAtualizado)
        {
            if (id != petAtualizado.IdPet)
            {
                return BadRequest("O ID da URL não confere com o ID do corpo.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool atualizado = await _petService.UpdateAsync(id, petAtualizado);
            if (!atualizado)
            {
                return NotFound("Pet não encontrado.");
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
            bool removido = await _petService.DeleteAsync(id);
            if (!removido)
            {
                return NotFound("Pet não encontrado.");
            }

            return NoContent();
        }

        private static Recurso<Pet> CriarRecurso(Pet pet)
        {
            return HateoasBuilder.CriarRecurso(pet, Rota, pet.IdPet,
                new Link($"{Rota}/{pet.IdPet}/historico", "historico", "GET"),
                new Link($"/api/Tutores/{pet.IdTutor}", "tutor", "GET"),
                new Link($"/api/Consultas/pet/{pet.IdPet}", "consultas", "GET"),
                new Link($"/api/Vacinas/pet/{pet.IdPet}", "vacinas", "GET"),
                new Link($"/api/Medicamentos/pet/{pet.IdPet}", "medicamentos", "GET"));
        }
    }
}