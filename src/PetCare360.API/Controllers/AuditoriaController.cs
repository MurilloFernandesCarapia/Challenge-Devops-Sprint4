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
    [Authorize(Roles = PerfilUsuario.Admin)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class AuditoriaController : ControllerBase
    {
        private const string Rota = "/api/Auditoria";

        private static readonly Dictionary<string, string> RotasPorEntidade = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Tutor"] = "/api/Tutores",
            ["Pet"] = "/api/Pets",
            ["Clinica"] = "/api/Clinicas",
            ["Consulta"] = "/api/Consultas",
            ["Vacina"] = "/api/Vacinas",
            ["Medicamento"] = "/api/Medicamentos"
        };

        private readonly IAuditoriaService _auditoriaService;

        public AuditoriaController(IAuditoriaService auditoriaService)
        {
            _auditoriaService = auditoriaService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(RecursoPaginado<RegistroAuditoria>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] AuditoriaQueryParameters parametros)
        {
            var registros = await _auditoriaService.GetPagedAsync(parametros);
            return Ok(HateoasBuilder.CriarRecursoPaginado(registros, parametros, Rota, CriarRecurso, permiteCriacao: false));
        }

        [HttpGet("{entidade}/{entidadeId}")]
        [ProducesResponseType(typeof(IEnumerable<Recurso<RegistroAuditoria>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistorico(string entidade, int entidadeId)
        {
            var registros = await _auditoriaService.GetHistoricoAsync(entidade, entidadeId);
            return Ok(registros.Select(CriarRecurso));
        }

        private static Recurso<RegistroAuditoria> CriarRecurso(RegistroAuditoria registro)
        {
            var recurso = new Recurso<RegistroAuditoria> { Dados = registro };

            recurso.Links.Add(new Link($"{Rota}/{registro.Entidade}/{registro.EntidadeId}", "historico", "GET"));

            if (RotasPorEntidade.TryGetValue(registro.Entidade, out var rotaEntidade) && registro.Acao != AcaoAuditoria.Exclusao)
            {
                recurso.Links.Add(new Link($"{rotaEntidade}/{registro.EntidadeId}", "recurso", "GET"));
            }

            return recurso;
        }
    }
}