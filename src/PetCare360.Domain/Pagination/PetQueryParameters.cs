namespace PetCare360.Domain.Pagination
{
    
    public class PetQueryParameters : QueryParameters
    {
        public string? Nome { get; set; }
        public string? Especie { get; set; }
        public string? Raca { get; set; }
        public int? IdTutor { get; set; }
    }
}