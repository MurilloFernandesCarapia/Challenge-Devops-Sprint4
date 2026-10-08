using Microsoft.Extensions.Logging;
using Moq;
using PetCare360.Application.Services;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Exceptions;
using PetCare360.Domain.Interfaces;
using PetCare360.UnitTests.Fixtures;

namespace PetCare360.UnitTests.Services
{
    [Collection("ServicesCollection")]
    public class VacinaServiceTests
    {
        private readonly Mock<IVacinaRepository> _mockVacinaRepository;
        private readonly Mock<IPetRepository> _mockPetRepository;
        private readonly Mock<IAuditoriaService> _mockAuditoriaService;
        private readonly Mock<ILogger<VacinaService>> _mockLogger;
        private readonly VacinaService _vacinaService;

        public VacinaServiceTests(TelemetryFixture fixture)
        {
            _mockVacinaRepository = new Mock<IVacinaRepository>();
            _mockPetRepository = new Mock<IPetRepository>();
            _mockAuditoriaService = new Mock<IAuditoriaService>();
            _mockLogger = new Mock<ILogger<VacinaService>>();

            _vacinaService = new VacinaService(
                _mockVacinaRepository.Object,
                _mockPetRepository.Object,
                _mockAuditoriaService.Object,
                _mockLogger.Object,
                fixture.MeterFactory);
        }

        private static Vacina CriarVacina(int id = 1, int idPet = 1)
        {
            return new Vacina
            {
                IdVacina = id,
                NmVacina = "V10",
                Fabricante = "Zoetis",
                DtAplicacao = new DateTime(2026, 9, 1),
                DtProximaDose = new DateTime(2027, 9, 1),
                IdPet = idPet
            };
        }

        [Fact]
        public async Task CreateAsync_PetExiste_RegistraVacina()
        {
            var vacina = CriarVacina(8, 2);
            _mockPetRepository.Setup(r => r.ExistsAsync(2)).ReturnsAsync(true);

            var resultado = await _vacinaService.CreateAsync(vacina);

            Assert.Equal("V10", resultado.NmVacina);
            _mockVacinaRepository.Verify(r => r.AddAsync(vacina), Times.Once);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Vacina", 8, AcaoAuditoria.Criacao, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_PetNaoExiste_LancaRegraDeNegocioException()
        {
            var vacina = CriarVacina(idPet: 999);
            _mockPetRepository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

            var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => _vacinaService.CreateAsync(vacina));

            Assert.Equal("O pet informado não existe.", excecao.Message);
            _mockVacinaRepository.Verify(r => r.AddAsync(It.IsAny<Vacina>()), Times.Never);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetByPetAsync_PetComVacinas_RetornaVacinasDoPet()
        {
            var vacinas = new List<Vacina> { CriarVacina(1, 3), CriarVacina(2, 3) };
            _mockVacinaRepository.Setup(r => r.GetByPetAsync(3)).ReturnsAsync(vacinas);

            var resultado = await _vacinaService.GetByPetAsync(3);

            Assert.Equal(2, resultado.Count());
            Assert.All(resultado, v => Assert.Equal(3, v.IdPet));
        }

        [Fact]
        public async Task UpdateAsync_VacinaExiste_AtualizaDatasERetornaTrue()
        {
            var vacinaExistente = CriarVacina(5);
            var dadosNovos = CriarVacina(5);
            dadosNovos.DtProximaDose = new DateTime(2027, 3, 15);
            dadosNovos.IdConsulta = 10;
            _mockVacinaRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(vacinaExistente);

            var resultado = await _vacinaService.UpdateAsync(5, dadosNovos);

            Assert.True(resultado);
            Assert.Equal(new DateTime(2027, 3, 15), vacinaExistente.DtProximaDose);
            Assert.Equal(10, vacinaExistente.IdConsulta);
            _mockVacinaRepository.Verify(r => r.UpdateAsync(vacinaExistente), Times.Once);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Vacina", 5, AcaoAuditoria.Atualizacao, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_VacinaNaoExiste_RetornaFalse()
        {
            _mockVacinaRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Vacina?)null);

            var resultado = await _vacinaService.UpdateAsync(99, CriarVacina(99));

            Assert.False(resultado);
            _mockVacinaRepository.Verify(r => r.UpdateAsync(It.IsAny<Vacina>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_VacinaExiste_RemoveERegistraExclusao()
        {
            var vacina = CriarVacina(6);
            _mockVacinaRepository.Setup(r => r.GetByIdAsync(6)).ReturnsAsync(vacina);

            var resultado = await _vacinaService.DeleteAsync(6);

            Assert.True(resultado);
            _mockVacinaRepository.Verify(r => r.DeleteAsync(vacina), Times.Once);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Vacina", 6, AcaoAuditoria.Exclusao, It.IsAny<string>()), Times.Once);
        }
    }
}