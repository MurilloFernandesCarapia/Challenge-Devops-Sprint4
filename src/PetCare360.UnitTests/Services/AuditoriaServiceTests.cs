using Microsoft.Extensions.Logging;
using Moq;
using PetCare360.Application.Services;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;
using PetCare360.Domain.Pagination;

namespace PetCare360.UnitTests.Services
{
    public class AuditoriaServiceTests
    {
        private readonly Mock<IAuditoriaRepository> _mockAuditoriaRepository;
        private readonly Mock<ILogger<AuditoriaService>> _mockLogger;
        private readonly AuditoriaService _auditoriaService;

        public AuditoriaServiceTests()
        {
            _mockAuditoriaRepository = new Mock<IAuditoriaRepository>();
            _mockLogger = new Mock<ILogger<AuditoriaService>>();
            _auditoriaService = new AuditoriaService(_mockAuditoriaRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task RegistrarAsync_DadosValidos_GravaRegistroNoRepositorio()
        {
            RegistroAuditoria? registroGravado = null;
            _mockAuditoriaRepository
                .Setup(r => r.AddAsync(It.IsAny<RegistroAuditoria>()))
                .Callback<RegistroAuditoria>(r => registroGravado = r)
                .Returns(Task.CompletedTask);

            await _auditoriaService.RegistrarAsync("Pet", 10, AcaoAuditoria.Criacao, "Pet Rex cadastrado");

            _mockAuditoriaRepository.Verify(r => r.AddAsync(It.IsAny<RegistroAuditoria>()), Times.Once);
            Assert.NotNull(registroGravado);
            Assert.Equal("Pet", registroGravado.Entidade);
            Assert.Equal(10, registroGravado.EntidadeId);
            Assert.Equal(AcaoAuditoria.Criacao, registroGravado.Acao);
            Assert.Equal(DateTimeKind.Utc, registroGravado.DataHora.Kind);
        }

        [Fact]
        public async Task RegistrarAsync_MongoIndisponivel_NaoPropagaExcecao()
        {
            _mockAuditoriaRepository
                .Setup(r => r.AddAsync(It.IsAny<RegistroAuditoria>()))
                .ThrowsAsync(new TimeoutException("MongoDB fora do ar"));

            var excecao = await Record.ExceptionAsync(
                () => _auditoriaService.RegistrarAsync("Tutor", 1, AcaoAuditoria.Exclusao, "Tutor removido"));

            Assert.Null(excecao);
        }

        [Fact]
        public async Task GetPagedAsync_ComFiltro_RetornaResultadoDoRepositorio()
        {
            var parametros = new AuditoriaQueryParameters { Entidade = "Pet" };
            var esperado = new PagedResult<RegistroAuditoria>
            {
                Itens = new List<RegistroAuditoria> { new RegistroAuditoria { Entidade = "Pet", EntidadeId = 1 } },
                Pagina = 1,
                TamanhoPagina = 10,
                TotalItens = 1
            };
            _mockAuditoriaRepository.Setup(r => r.GetPagedAsync(parametros)).ReturnsAsync(esperado);

            var resultado = await _auditoriaService.GetPagedAsync(parametros);

            Assert.Same(esperado, resultado);
        }

        [Fact]
        public void AuditoriaQueryParameters_SemValores_OrdenaDoMaisRecenteParaOMaisAntigo()
        {
            var parametros = new AuditoriaQueryParameters();

            var ascendente = parametros.Ascendente;

            Assert.False(ascendente);
        }
    }
}