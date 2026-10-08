using Microsoft.Extensions.Logging;
using Moq;
using PetCare360.Application.Services;
using PetCare360.Domain.Dtos;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Exceptions;
using PetCare360.Domain.Interfaces;

namespace PetCare360.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUsuarioRepository> _mockUsuarioRepository;
        private readonly Mock<ISenhaHasher> _mockSenhaHasher;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IAuditoriaService> _mockAuditoriaService;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUsuarioRepository = new Mock<IUsuarioRepository>();
            _mockSenhaHasher = new Mock<ISenhaHasher>();
            _mockTokenService = new Mock<ITokenService>();
            _mockAuditoriaService = new Mock<IAuditoriaService>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            _authService = new AuthService(
                _mockUsuarioRepository.Object,
                _mockSenhaHasher.Object,
                _mockTokenService.Object,
                _mockAuditoriaService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task RegistrarAsync_EmailNovo_CriaUsuarioComPerfilPadraoESenhaCriptografada()
        {
            var request = new RegistroUsuarioRequest { Nome = "Ana", Email = "Ana@Email.com", Senha = "senha123" };
            Usuario? usuarioGravado = null;
            _mockUsuarioRepository.Setup(r => r.ExistsByEmailAsync("ana@email.com")).ReturnsAsync(false);
            _mockSenhaHasher.Setup(h => h.GerarHash("senha123")).Returns("hash-gerado");
            _mockUsuarioRepository
                .Setup(r => r.AddAsync(It.IsAny<Usuario>()))
                .Callback<Usuario>(u => usuarioGravado = u)
                .Returns(Task.CompletedTask);

            var resposta = await _authService.RegistrarAsync(request);

            Assert.NotNull(usuarioGravado);
            Assert.Equal("ana@email.com", usuarioGravado.Email);
            Assert.Equal("hash-gerado", usuarioGravado.SenhaHash);
            Assert.Equal(PerfilUsuario.Usuario, usuarioGravado.Perfil);
            Assert.Equal(PerfilUsuario.Usuario, resposta.Perfil);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Usuario", It.IsAny<int>(), AcaoAuditoria.Criacao, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RegistrarAsync_EmailJaCadastrado_LancaRegraDeNegocioException()
        {
            var request = new RegistroUsuarioRequest { Nome = "Ana", Email = "ana@email.com", Senha = "senha123" };
            _mockUsuarioRepository.Setup(r => r.ExistsByEmailAsync("ana@email.com")).ReturnsAsync(true);

            var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => _authService.RegistrarAsync(request));

            Assert.Equal("Já existe um usuário cadastrado com esse e-mail.", excecao.Message);
            _mockUsuarioRepository.Verify(r => r.AddAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_CredenciaisValidas_RetornaToken()
        {
            var usuario = new Usuario { IdUsuario = 1, NmUsuario = "Admin", Email = "admin@petcare360.com", SenhaHash = "hash", Perfil = PerfilUsuario.Admin };
            var tokenEsperado = new TokenResponse("token-jwt", DateTime.UtcNow.AddHours(1), "Admin", "admin@petcare360.com", PerfilUsuario.Admin);
            _mockUsuarioRepository.Setup(r => r.GetByEmailAsync("admin@petcare360.com")).ReturnsAsync(usuario);
            _mockSenhaHasher.Setup(h => h.Verificar("Admin@123", "hash")).Returns(true);
            _mockTokenService.Setup(t => t.GerarToken(usuario)).Returns(tokenEsperado);

            var resposta = await _authService.LoginAsync(new LoginRequest { Email = "admin@petcare360.com", Senha = "Admin@123" });

            Assert.Equal("token-jwt", resposta.Token);
            Assert.Equal(PerfilUsuario.Admin, resposta.Perfil);
        }

        [Fact]
        public async Task LoginAsync_SenhaErrada_LancaCredenciaisInvalidasException()
        {
            var usuario = new Usuario { IdUsuario = 1, Email = "admin@petcare360.com", SenhaHash = "hash" };
            _mockUsuarioRepository.Setup(r => r.GetByEmailAsync("admin@petcare360.com")).ReturnsAsync(usuario);
            _mockSenhaHasher.Setup(h => h.Verificar("errada", "hash")).Returns(false);

            await Assert.ThrowsAsync<CredenciaisInvalidasException>(
                () => _authService.LoginAsync(new LoginRequest { Email = "admin@petcare360.com", Senha = "errada" }));

            _mockTokenService.Verify(t => t.GerarToken(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_UsuarioInexistente_LancaCredenciaisInvalidasException()
        {
            _mockUsuarioRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);

            await Assert.ThrowsAsync<CredenciaisInvalidasException>(
                () => _authService.LoginAsync(new LoginRequest { Email = "ninguem@email.com", Senha = "qualquer" }));
        }
    }
}