using Microsoft.Extensions.Logging;
using PetCare360.Domain.Dtos;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Exceptions;
using PetCare360.Domain.Interfaces;

namespace PetCare360.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISenhaHasher _senhaHasher;
        private readonly ITokenService _tokenService;
        private readonly IAuditoriaService _auditoriaService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            ISenhaHasher senhaHasher,
            ITokenService tokenService,
            IAuditoriaService auditoriaService,
            ILogger<AuthService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _senhaHasher = senhaHasher;
            _tokenService = tokenService;
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        public async Task<UsuarioResponse> RegistrarAsync(RegistroUsuarioRequest request)
        {
            var email = request.Email.Trim().ToLower();

            bool emailJaCadastrado = await _usuarioRepository.ExistsByEmailAsync(email);
            if (emailJaCadastrado)
            {
                _logger.LogWarning("Tentativa de registro com e-mail já cadastrado: {Email}", email);
                throw new RegraDeNegocioException("Já existe um usuário cadastrado com esse e-mail.");
            }

            var usuario = new Usuario
            {
                NmUsuario = request.Nome.Trim(),
                Email = email,
                SenhaHash = _senhaHasher.GerarHash(request.Senha),
                Perfil = PerfilUsuario.Usuario,
                DtCriacao = DateTime.UtcNow
            };

            await _usuarioRepository.AddAsync(usuario);

            await _auditoriaService.RegistrarAsync(nameof(Usuario), usuario.IdUsuario, AcaoAuditoria.Criacao, $"Usuário {usuario.Email} registrado");

            _logger.LogInformation("Usuário registrado. IdUsuario: {IdUsuario}", usuario.IdUsuario);

            return new UsuarioResponse(usuario.IdUsuario, usuario.NmUsuario, usuario.Email, usuario.Perfil);
        }

        public async Task<TokenResponse> LoginAsync(LoginRequest request)
        {
            var email = request.Email.Trim().ToLower();
            var usuario = await _usuarioRepository.GetByEmailAsync(email);

            if (usuario == null || !_senhaHasher.Verificar(request.Senha, usuario.SenhaHash))
            {
                _logger.LogWarning("Tentativa de login com credenciais inválidas para {Email}", email);
                throw new CredenciaisInvalidasException();
            }

            _logger.LogInformation("Login realizado. IdUsuario: {IdUsuario}, Perfil: {Perfil}", usuario.IdUsuario, usuario.Perfil);

            return _tokenService.GerarToken(usuario);
        }
    }
}