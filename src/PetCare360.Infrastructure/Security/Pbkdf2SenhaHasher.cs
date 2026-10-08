using System.Security.Cryptography;
using PetCare360.Domain.Interfaces;

namespace PetCare360.Infrastructure.Security
{
    public class Pbkdf2SenhaHasher : ISenhaHasher
    {
        private const int TamanhoSalt = 16;
        private const int TamanhoHash = 32;
        private const int Iteracoes = 100_000;

        public string GerarHash(string senha)
        {
            var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
            var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);

            return $"{Iteracoes}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool Verificar(string senha, string senhaHash)
        {
            var partes = senhaHash.Split('.');
            if (partes.Length != 3 || !int.TryParse(partes[0], out var iteracoes))
            {
                return false;
            }

            var salt = Convert.FromBase64String(partes[1]);
            var hashEsperado = Convert.FromBase64String(partes[2]);
            var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iteracoes, HashAlgorithmName.SHA256, hashEsperado.Length);

            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
        }
    }
}