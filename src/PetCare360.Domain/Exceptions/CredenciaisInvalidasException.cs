namespace PetCare360.Domain.Exceptions
{
    public class CredenciaisInvalidasException : Exception
    {
        public CredenciaisInvalidasException() : base("E-mail ou senha inválidos.") { }
    }
}