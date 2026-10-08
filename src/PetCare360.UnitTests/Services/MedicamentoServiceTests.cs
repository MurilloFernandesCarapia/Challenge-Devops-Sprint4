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
    public class MedicamentoServiceTests
    {
        private readonly Mock<IMedicamentoRepository> _mockMedicamentoRepository;
        private readonly Mock<IPetRepository> _mockPetRepository;
        private readonly Mock<IAuditoriaService> _mockAuditoriaService;
        private readonly Mock<ILogger<MedicamentoService>> _mockLogger;
        private readonly MedicamentoService _medicamentoService;

        public MedicamentoServiceTests(TelemetryFixture fixture)
        {
            _mockMedicamentoRepository = new Mock<IMedicamentoRepository>();
            _mockPetRepository = new Mock<IPetRepository>();
            _mockAuditoriaService = new Mock<IAuditoriaService>();
            _mockLogger = new Mock<ILogger<MedicamentoService>>();

            _medicamentoService = new MedicamentoService(
                _mockMedicamentoRepository.Object,
                _mockPetRepository.Object,
                _mockAuditoriaService.Object,
                _mockLogger.Object,
                fixture.MeterFactory);
        }

        private static Medicamento CriarMedicamento(int id = 1, int idPet = 1)
        {
            return new Medicamento
            {
                IdMedicamento = id,
                NmMedicamento = "Amoxicilina",
                Dosagem = "250mg",
                Frequencia = "12 em 12 horas",
                DtInicio = new DateTime(2026, 9, 1),
                DtFim = new DateTime(2026, 9, 10),
                IdPet = idPet
            };
        }

        [Fact]
        public async Task CreateAsync_PetExiste_PrescreveMedicamento()
        {
            var medicamento = CriarMedicamento(4, 2);
            _mockPetRepository.Setup(r => r.ExistsAsync(2)).ReturnsAsync(true);

            var resultado = await _medicamentoService.CreateAsync(medicamento);

            Assert.Equal("Amoxicilina", resultado.NmMedicamento);
            _mockMedicamentoRepository.Verify(r => r.AddAsync(medicamento), Times.Once);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Medicamento", 4, AcaoAuditoria.Criacao, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_PetNaoExiste_LancaRegraDeNegocioException()
        {
            var medicamento = CriarMedicamento(idPet: 999);
            _mockPetRepository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

            var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => _medicamentoService.CreateAsync(medicamento));

            Assert.Equal("O pet informado não existe.", excecao.Message);
            _mockMedicamentoRepository.Verify(r => r.AddAsync(It.IsAny<Medicamento>()), Times.Never);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_MedicamentoExiste_RetornaMedicamento()
        {
            _mockMedicamentoRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CriarMedicamento());

            var resultado = await _medicamentoService.GetByIdAsync(1);

            Assert.NotNull(resultado);
            Assert.Equal("250mg", resultado.Dosagem);
        }

        [Fact]
        public async Task UpdateAsync_MedicamentoExiste_AtualizaDosagemERetornaTrue()
        {
            var medicamentoExistente = CriarMedicamento(7);
            var dadosNovos = CriarMedicamento(7);
            dadosNovos.Dosagem = "500mg";
            dadosNovos.Frequencia = "8 em 8 horas";
            _mockMedicamentoRepository.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(medicamentoExistente);

            var resultado = await _medicamentoService.UpdateAsync(7, dadosNovos);

            Assert.True(resultado);
            Assert.Equal("500mg", medicamentoExistente.Dosagem);
            Assert.Equal("8 em 8 horas", medicamentoExistente.Frequencia);
            _mockMedicamentoRepository.Verify(r => r.UpdateAsync(medicamentoExistente), Times.Once);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Medicamento", 7, AcaoAuditoria.Atualizacao, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_MedicamentoExiste_RemoveERegistraExclusao()
        {
            var medicamento = CriarMedicamento(9);
            _mockMedicamentoRepository.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(medicamento);

            var resultado = await _medicamentoService.DeleteAsync(9);

            Assert.True(resultado);
            _mockMedicamentoRepository.Verify(r => r.DeleteAsync(medicamento), Times.Once);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Medicamento", 9, AcaoAuditoria.Exclusao, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_MedicamentoNaoExiste_RetornaFalseSemAuditar()
        {
            _mockMedicamentoRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Medicamento?)null);

            var resultado = await _medicamentoService.DeleteAsync(99);

            Assert.False(resultado);
            _mockMedicamentoRepository.Verify(r => r.DeleteAsync(It.IsAny<Medicamento>()), Times.Never);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}