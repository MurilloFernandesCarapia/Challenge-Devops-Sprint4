namespace PetCare360.Domain.Pagination
{
    public class TutorQueryParameters : QueryParameters
    {
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Cpf { get; set; }
    }
}