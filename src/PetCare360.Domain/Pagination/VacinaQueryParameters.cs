namespace PetCare360.Domain.Pagination
{
    public class VacinaQueryParameters : QueryParameters
    {
        public string? Nome { get; set; }
        public string? Fabricante { get; set; }
        public int? IdPet { get; set; }
        public DateTime? ProximaDoseAte { get; set; }
    }
}