namespace PetCare360.Domain.Pagination
{
    public class ClinicaQueryParameters : QueryParameters
    {
        public string? Nome { get; set; }
        public string? Cnpj { get; set; }
    }
}