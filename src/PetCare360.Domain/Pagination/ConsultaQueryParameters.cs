namespace PetCare360.Domain.Pagination
{
    public class ConsultaQueryParameters : QueryParameters
    {
        public int? IdPet { get; set; }
        public int? IdClinica { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }
}