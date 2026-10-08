using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare360.Domain.Dtos;
using PetCare360.Domain.Interfaces;

namespace PetCare360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("registrar")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await _authService.RegistrarAsync(request);
            return StatusCode(StatusCodes.Status201Created, usuario);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await _authService.LoginAsync(request);
            return Ok(token);
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Me()
        {
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var idUsuario);

            var usuario = new UsuarioResponse(
                idUsuario,
                User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
                User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
                User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty);

            return Ok(usuario);
        }
    }
}