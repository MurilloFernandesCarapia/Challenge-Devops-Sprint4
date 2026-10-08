namespace PetCare360.Domain.Dtos
{
    public record TokenResponse(string Token, DateTime ExpiraEm, string Nome, string Email, string Perfil);
}