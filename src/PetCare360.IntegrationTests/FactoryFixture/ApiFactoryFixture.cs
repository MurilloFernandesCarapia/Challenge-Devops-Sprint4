using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;
using PetCare360.Infrastructure.Data;
using PetCare360.IntegrationTests.Fakes;

namespace PetCare360.IntegrationTests.FactoryFixture
{
    public class ApiFactoryFixture : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:OracleConnection"] =
                        "User Id=teste;Password=teste;Data Source=localhost:1521/XEPDB1;"
                });
            });

            builder.ConfigureServices(services =>
            {
                var descritores = services
                    .Where(d => d.ServiceType.FullName != null &&
                                (d.ServiceType.FullName.Contains("DbContextOptions") ||
                                 d.ServiceType == typeof(AppDbContext)))
                    .ToList();

                foreach (var descritor in descritores)
                {
                    services.Remove(descritor);
                }

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("PetCare360TestDb"));

                services.RemoveAll<IAuditoriaRepository>();
                services.AddSingleton<IAuditoriaRepository, FakeAuditoriaRepository>();

                services.Configure<HealthCheckServiceOptions>(options =>
                {
                    var mongoCheck = options.Registrations.FirstOrDefault(r => r.Name == "mongodb");
                    if (mongoCheck != null)
                    {
                        options.Registrations.Remove(mongoCheck);
                    }
                });
            });
        }

        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GerarToken(PerfilUsuario.Admin));
        }

        public HttpClient CriarClienteComPerfil(string perfil)
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GerarToken(perfil));
            return client;
        }

        public HttpClient CriarClienteAnonimo()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Authorization = null;
            return client;
        }

        private string GerarToken(string perfil)
        {
            using var scope = Services.CreateScope();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

            var usuario = new Usuario
            {
                IdUsuario = 999,
                NmUsuario = $"Teste {perfil}",
                Email = $"{perfil.ToLower()}@teste.petcare360.com",
                Perfil = perfil
            };

            return tokenService.GerarToken(usuario).Token;
        }
    }

    [CollectionDefinition("ApiCollection")]
    public class ApiCollection : ICollectionFixture<ApiFactoryFixture>
    {
    }
}