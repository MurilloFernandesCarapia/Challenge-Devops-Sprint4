namespace PetCare360.Domain.Pagination
{
    public class AuditoriaQueryParameters : QueryParameters
    {
        public AuditoriaQueryParameters()
        {
            Ascendente = false;
        }

        public string? Entidade { get; set; }
        public int? EntidadeId { get; set; }
        public string? Acao { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }
}