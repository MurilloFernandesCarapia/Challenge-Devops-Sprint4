namespace PetCare360.Domain.Entities
{
    public class RegistroAuditoria
    {
        public string? Id { get; set; }
        public string Entidade { get; set; } = string.Empty;
        public int EntidadeId { get; set; }
        public string Acao { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataHora { get; set; } = DateTime.UtcNow;
    }
}