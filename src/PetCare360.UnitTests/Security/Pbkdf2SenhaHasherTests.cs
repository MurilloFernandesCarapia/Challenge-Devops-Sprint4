using PetCare360.Infrastructure.Security;

namespace PetCare360.UnitTests.Security
{
    public class Pbkdf2SenhaHasherTests
    {
        private readonly Pbkdf2SenhaHasher _senhaHasher;

        public Pbkdf2SenhaHasherTests()
        {
            _senhaHasher = new Pbkdf2SenhaHasher();
        }

        [Fact]
        public void GerarHash_SenhaValida_NaoGuardaSenhaEmTextoPuro()
        {
            var senha = "MinhaSenha@123";

            var hash = _senhaHasher.GerarHash(senha);

            Assert.DoesNotContain(senha, hash);
            Assert.Equal(3, hash.Split('.').Length);
        }

        [Fact]
        public void GerarHash_MesmaSenhaDuasVezes_GeraHashesDiferentes()
        {
            var senha = "MinhaSenha@123";

            var primeiroHash = _senhaHasher.GerarHash(senha);
            var segundoHash = _senhaHasher.GerarHash(senha);

            Assert.NotEqual(primeiroHash, segundoHash);
        }

        [Fact]
        public void Verificar_SenhaCorreta_RetornaTrue()
        {
            var hash = _senhaHasher.GerarHash("MinhaSenha@123");

            var valida = _senhaHasher.Verificar("MinhaSenha@123", hash);

            Assert.True(valida);
        }

        [Fact]
        public void Verificar_SenhaErrada_RetornaFalse()
        {
            var hash = _senhaHasher.GerarHash("MinhaSenha@123");

            var valida = _senhaHasher.Verificar("SenhaErrada", hash);

            Assert.False(valida);
        }

        [Fact]
        public void Verificar_HashEmFormatoInvalido_RetornaFalse()
        {
            var hashInvalido = "isso-nao-e-um-hash";

            var valida = _senhaHasher.Verificar("qualquer", hashInvalido);

            Assert.False(valida);
        }
    }
}