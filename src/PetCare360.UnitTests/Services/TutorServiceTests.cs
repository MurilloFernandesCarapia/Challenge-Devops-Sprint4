using Microsoft.Extensions.Logging;
using Moq;
using PetCare360.Application.Services;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;
using PetCare360.UnitTests.Fixtures;

namespace PetCare360.UnitTests.Services
{
    [Collection("ServicesCollection")]
    public class TutorServiceTests
    {
        private readonly Mock<ITutorRepository> _mockTutorRepository;
        private readonly Mock<IAuditoriaService> _mockAuditoriaService;
        private readonly Mock<ILogger<TutorService>> _mockLogger;
        private readonly TutorService _tutorService;

        public TutorServiceTests(TelemetryFixture fixture)
        {
            _mockTutorRepository = new Mock<ITutorRepository>();
            _mockAuditoriaService = new Mock<IAuditoriaService>();
            _mockLogger = new Mock<ILogger<TutorService>>();

            _tutorService = new TutorService(
                _mockTutorRepository.Object,
                _mockAuditoriaService.Object,
                _mockLogger.Object,
                fixture.MeterFactory);
        }

        [Fact]
        public async Task GetAllAsync_ExistemTutores_RetornaListaCompleta()
        {
            var tutores = new List<Tutor>
            {
                new Tutor { IdTutor = 1, NmTutor = "Ana", Cpf = "111", Email = "ana@email.com" },
                new Tutor { IdTutor = 2, NmTutor = "Bruno", Cpf = "222", Email = "bruno@email.com" }
            };
            _mockTutorRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(tutores);

            var resultado = await _tutorService.GetAllAsync();

            Assert.Equal(2, resultado.Count());
        }

        [Fact]
        public async Task CreateAsync_DadosValidos_CadastraTutorERegistraAuditoria()
        {
            var tutor = new Tutor { IdTutor = 3, NmTutor = "Ana", Cpf = "111", Email = "ana@email.com" };

            var resultado = await _tutorService.CreateAsync(tutor);

            Assert.Equal("Ana", resultado.NmTutor);
            _mockTutorRepository.Verify(r => r.AddAsync(tutor), Times.Once);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Tutor", 3, AcaoAuditoria.Criacao, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_TutorNaoExiste_RetornaFalse()
        {
            _mockTutorRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Tutor?)null);

            var resultado = await _tutorService.DeleteAsync(99);

            Assert.False(resultado);
            _mockTutorRepository.Verify(r => r.DeleteAsync(It.IsAny<Tutor>()), Times.Never);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_TutorExiste_AtualizaDadosERetornaTrue()
        {
            var tutorExistente = new Tutor { IdTutor = 4, NmTutor = "Ana", Cpf = "111", Email = "ana@email.com", Telefone = "(11) 90000-0000", Endereco = "Rua A, 1" };
            var dadosNovos = new Tutor { IdTutor = 4, NmTutor = "Ana Souza", Cpf = "111", Email = "ana.souza@email.com", Telefone = "(11) 98888-7777", Endereco = "Rua B, 2" };
            _mockTutorRepository.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(tutorExistente);

            var resultado = await _tutorService.UpdateAsync(4, dadosNovos);

            Assert.True(resultado);
            Assert.Equal("Ana Souza", tutorExistente.NmTutor);
            Assert.Equal("ana.souza@email.com", tutorExistente.Email);
            Assert.Equal("Rua B, 2", tutorExistente.Endereco);
            _mockTutorRepository.Verify(r => r.UpdateAsync(tutorExistente), Times.Once);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Tutor", 4, AcaoAuditoria.Atualizacao, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_TutorNaoExiste_RetornaFalse()
        {
            _mockTutorRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Tutor?)null);

            var resultado = await _tutorService.UpdateAsync(99, new Tutor { IdTutor = 99, NmTutor = "Ninguém" });

            Assert.False(resultado);
            _mockTutorRepository.Verify(r => r.UpdateAsync(It.IsAny<Tutor>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_TutorExiste_RemoveERegistraExclusao()
        {
            var tutor = new Tutor { IdTutor = 7, NmTutor = "Bruno", Cpf = "222", Email = "bruno@email.com" };
            _mockTutorRepository.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(tutor);

            var resultado = await _tutorService.DeleteAsync(7);

            Assert.True(resultado);
            _mockTutorRepository.Verify(r => r.DeleteAsync(tutor), Times.Once);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Tutor", 7, AcaoAuditoria.Exclusao, It.IsAny<string>()), Times.Once);
        }
    }
}