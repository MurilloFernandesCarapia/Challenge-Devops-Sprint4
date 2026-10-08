namespace PetCare360.Infrastructure.Security
{
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiracaoMinutos { get; set; } = 60;
    }
}