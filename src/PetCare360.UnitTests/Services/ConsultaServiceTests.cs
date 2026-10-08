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
    public class ConsultaServiceTests
    {
        private readonly Mock<IConsultaRepository> _mockConsultaRepository;
        private readonly Mock<IPetRepository> _mockPetRepository;
        private readonly Mock<IClinicaRepository> _mockClinicaRepository;
        private readonly Mock<IAuditoriaService> _mockAuditoriaService;
        private readonly Mock<ILogger<ConsultaService>> _mockLogger;
        private readonly ConsultaService _consultaService;

        public ConsultaServiceTests(TelemetryFixture fixture)
        {
            _mockConsultaRepository = new Mock<IConsultaRepository>();
            _mockPetRepository = new Mock<IPetRepository>();
            _mockClinicaRepository = new Mock<IClinicaRepository>();
            _mockAuditoriaService = new Mock<IAuditoriaService>();
            _mockLogger = new Mock<ILogger<ConsultaService>>();

            _consultaService = new ConsultaService(
                _mockConsultaRepository.Object,
                _mockPetRepository.Object,
                _mockClinicaRepository.Object,
                _mockAuditoriaService.Object,
                _mockLogger.Object,
                fixture.MeterFactory);
        }

        [Fact]
        public async Task CreateAsync_PetEClinicaExistem_RegistraConsulta()
        {
            var consulta = new Consulta { IdConsulta = 7, DtConsulta = new DateTime(2026, 9, 10), IdPet = 1, IdClinica = 1 };
            _mockPetRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
            _mockClinicaRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

            var resultado = await _consultaService.CreateAsync(consulta);

            Assert.Equal(1, resultado.IdPet);
            _mockConsultaRepository.Verify(r => r.AddAsync(consulta), Times.Once);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Consulta", 7, AcaoAuditoria.Criacao, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_PetNaoExiste_LancaRegraDeNegocioException()
        {
            var consulta = new Consulta { IdPet = 999, IdClinica = 1 };
            _mockPetRepository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

            var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => _consultaService.CreateAsync(consulta));

            Assert.Equal("O pet informado não existe.", excecao.Message);
            _mockClinicaRepository.Verify(r => r.ExistsAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ClinicaNaoExiste_LancaRegraDeNegocioException()
        {
            var consulta = new Consulta { IdPet = 1, IdClinica = 999 };
            _mockPetRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
            _mockClinicaRepository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

            var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => _consultaService.CreateAsync(consulta));

            Assert.Equal("A clínica informada não existe.", excecao.Message);
            _mockConsultaRepository.Verify(r => r.AddAsync(It.IsAny<Consulta>()), Times.Never);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetByPetAsync_PetComConsultas_RetornaSomenteAsDoPet()
        {
            var consultas = new List<Consulta>
            {
                new Consulta { IdConsulta = 1, IdPet = 1, IdClinica = 1 },
                new Consulta { IdConsulta = 2, IdPet = 1, IdClinica = 2 }
            };
            _mockConsultaRepository.Setup(r => r.GetByPetAsync(1)).ReturnsAsync(consultas);

            var resultado = await _consultaService.GetByPetAsync(1);

            Assert.Equal(2, resultado.Count());
            Assert.All(resultado, c => Assert.Equal(1, c.IdPet));
        }

        [Fact]
        public async Task GetByClinicaAsync_ClinicaComConsultas_RetornaSomenteAsDaClinica()
        {
            var consultas = new List<Consulta>
            {
                new Consulta { IdConsulta = 3, IdPet = 1, IdClinica = 2 },
                new Consulta { IdConsulta = 4, IdPet = 5, IdClinica = 2 }
            };
            _mockConsultaRepository.Setup(r => r.GetByClinicaAsync(2)).ReturnsAsync(consultas);

            var resultado = await _consultaService.GetByClinicaAsync(2);

            Assert.Equal(2, resultado.Count());
            Assert.All(resultado, c => Assert.Equal(2, c.IdClinica));
        }

        [Fact]
        public async Task UpdateAsync_ConsultaExiste_AtualizaDiagnosticoERetornaTrue()
        {
            var consultaExistente = new Consulta { IdConsulta = 5, DtConsulta = new DateTime(2026, 9, 10), Descricao = "Check-up", Diagnostico = "Em análise", IdPet = 1, IdClinica = 1 };
            var dadosNovos = new Consulta { IdConsulta = 5, DtConsulta = new DateTime(2026, 9, 12), Descricao = "Check-up", Diagnostico = "Saudável", IdPet = 1, IdClinica = 1 };
            _mockConsultaRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(consultaExistente);

            var resultado = await _consultaService.UpdateAsync(5, dadosNovos);

            Assert.True(resultado);
            Assert.Equal("Saudável", consultaExistente.Diagnostico);
            Assert.Equal(new DateTime(2026, 9, 12), consultaExistente.DtConsulta);
            _mockConsultaRepository.Verify(r => r.UpdateAsync(consultaExistente), Times.Once);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Consulta", 5, AcaoAuditoria.Atualizacao, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ConsultaNaoExiste_RetornaFalse()
        {
            _mockConsultaRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Consulta?)null);

            var resultado = await _consultaService.UpdateAsync(99, new Consulta { IdConsulta = 99 });

            Assert.False(resultado);
            _mockConsultaRepository.Verify(r => r.UpdateAsync(It.IsAny<Consulta>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ConsultaExiste_RemoveERegistraExclusao()
        {
            var consulta = new Consulta { IdConsulta = 6, IdPet = 1, IdClinica = 1 };
            _mockConsultaRepository.Setup(r => r.GetByIdAsync(6)).ReturnsAsync(consulta);

            var resultado = await _consultaService.DeleteAsync(6);

            Assert.True(resultado);
            _mockConsultaRepository.Verify(r => r.DeleteAsync(consulta), Times.Once);
            _mockAuditoriaService.Verify(a => a.RegistrarAsync("Consulta", 6, AcaoAuditoria.Exclusao, It.IsAny<string>()), Times.Once);
        }
    }
}