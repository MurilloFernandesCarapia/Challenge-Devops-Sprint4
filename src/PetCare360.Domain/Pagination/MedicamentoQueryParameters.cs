namespace PetCare360.Domain.Pagination
{
    public class MedicamentoQueryParameters : QueryParameters
    {
        public string? Nome { get; set; }
        public int? IdPet { get; set; }
        public bool? EmUso { get; set; }
    }
}