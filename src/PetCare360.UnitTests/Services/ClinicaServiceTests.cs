using Microsoft.Extensions.Logging;
using Moq;
using PetCare360.Application.Services;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;
using PetCare360.Domain.Pagination;
using PetCare360.UnitTests.Fixtures;

namespace PetCare360.UnitTests.Services
{
    [Collection("ServicesCollection")]
    public class ClinicaServiceTests
    {
        private readonly Mock<IClinicaRepository> _mockClinicaRepository;
        private readonly Mock<IAuditoriaService> _mockAuditoriaService;
        private readonly Mock<ILogger<ClinicaService>> _mockLogger;
        private readonly ClinicaService _clinicaService;

        public ClinicaServiceTests(TelemetryFixture fixture)
        {
            _mockClinicaRepository = new Mock<IClinicaRepository>();
            _mockAuditoriaService = new Mock<IAuditoriaService>();
            _mockLogger = new Mock<ILogger<ClinicaService>>();

            _clinicaService = new ClinicaService(
                _mockClinicaRepository.Object,
                _mockAuditoriaService.Object,
                _mockLogger.Object,
                fixture.MeterFactory);
        }

        private static Clinica CriarClinica(int id = 1)
        {
            return new Clinica
            {
                IdClinica = id,
                NmClinica = "Clínica Vida Animal",
                Cnpj = "12.345.678/0001-90",
                Endereco = "Av. Paulista, 1000",
                Telefone = "(11) 3333-4444",
                Email = "contato@vidaanimal.com"
            };
        }

        [Fact]
        public async Task CreateAsync_ClinicaValida_AdicionaERegistraAuditoria()
        {
            var clinica = CriarClinica(5);

            var resultado = await _clinicaService.CreateAsync(clinica);

            Assert.Equal("Clínica Vida Animal", resultado.NmClinica);
            _mockClinicaRepository.Verify(r => r.AddAsync(clinica), Times.Once);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Clinica", 5, AcaoAuditoria.Criacao, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetByCnpjAsync_CnpjCadastrado_RetornaClinica()
        {
            var clinica = CriarClinica();
            _mockClinicaRepository.Setup(r => r.GetByCnpjAsync("12.345.678/0001-90")).ReturnsAsync(clinica);

            var resultado = await _clinicaService.GetByCnpjAsync("12.345.678/0001-90");

            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.IdClinica);
        }

        [Fact]
        public async Task GetPagedAsync_ComFiltros_RepassaParametrosAoRepositorio()
        {
            var parametros = new ClinicaQueryParameters { Nome = "Vida", Pagina = 2, TamanhoPagina = 5 };
            var paginaEsperada = new PagedResult<Clinica>
            {
                Itens = new List<Clinica> { CriarClinica() },
                Pagina = 2,
                TamanhoPagina = 5,
                TotalItens = 6
            };
            _mockClinicaRepository.Setup(r => r.GetPagedAsync(parametros)).ReturnsAsync(paginaEsperada);

            var resultado = await _clinicaService.GetPagedAsync(parametros);

            Assert.Same(paginaEsperada, resultado);
            Assert.Equal(2, resultado.TotalPaginas);
            _mockClinicaRepository.Verify(r => r.GetPagedAsync(parametros), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ClinicaExiste_AtualizaCamposERetornaTrue()
        {
            var clinicaExistente = CriarClinica(3);
            var dadosNovos = CriarClinica(3);
            dadosNovos.NmClinica = "Clínica Vida Animal 24h";
            dadosNovos.Telefone = "(11) 95555-6666";
            _mockClinicaRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(clinicaExistente);

            var resultado = await _clinicaService.UpdateAsync(3, dadosNovos);

            Assert.True(resultado);
            Assert.Equal("Clínica Vida Animal 24h", clinicaExistente.NmClinica);
            Assert.Equal("(11) 95555-6666", clinicaExistente.Telefone);
            _mockClinicaRepository.Verify(r => r.UpdateAsync(clinicaExistente), Times.Once);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Clinica", 3, AcaoAuditoria.Atualizacao, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ClinicaNaoExiste_RetornaFalseSemAuditar()
        {
            _mockClinicaRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Clinica?)null);

            var resultado = await _clinicaService.UpdateAsync(99, CriarClinica(99));

            Assert.False(resultado);
            _mockClinicaRepository.Verify(r => r.UpdateAsync(It.IsAny<Clinica>()), Times.Never);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ClinicaExiste_RemoveERegistraExclusao()
        {
            var clinica = CriarClinica(4);
            _mockClinicaRepository.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(clinica);

            var resultado = await _clinicaService.DeleteAsync(4);

            Assert.True(resultado);
            _mockClinicaRepository.Verify(r => r.DeleteAsync(clinica), Times.Once);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Clinica", 4, AcaoAuditoria.Exclusao, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ClinicaNaoExiste_RetornaFalse()
        {
            _mockClinicaRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Clinica?)null);

            var resultado = await _clinicaService.DeleteAsync(99);

            Assert.False(resultado);
            _mockClinicaRepository.Verify(r => r.DeleteAsync(It.IsAny<Clinica>()), Times.Never);
        }
    }
}