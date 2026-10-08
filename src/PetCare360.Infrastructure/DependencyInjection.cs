using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PetCare360.Application.Diagnostics;
using PetCare360.Application.Services;
using PetCare360.Domain.Interfaces;
using PetCare360.Infrastructure.Data;
using PetCare360.Infrastructure.HealthChecks;
using PetCare360.Infrastructure.NoSql;
using PetCare360.Infrastructure.Repositories;
using PetCare360.Infrastructure.Security;

namespace PetCare360.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("OracleConnection");

            services.AddDbContext<AppDbContext>(options =>
                options.UseOracle(connectionString,
                    b => b.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)));

            services.AddMongoDb(configuration);

            services.AddScoped<ITutorRepository, TutorRepository>();
            services.AddScoped<IPetRepository, PetRepository>();
            services.AddScoped<IClinicaRepository, ClinicaRepository>();
            services.AddScoped<IConsultaRepository, ConsultaRepository>();
            services.AddScoped<IVacinaRepository, VacinaRepository>();
            services.AddScoped<IMedicamentoRepository, MedicamentoRepository>();
            services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();

            services.AddScoped<ITutorService, TutorService>();
            services.AddScoped<IPetService, PetService>();
            services.AddScoped<IClinicaService, ClinicaService>();
            services.AddScoped<IConsultaService, ConsultaService>();
            services.AddScoped<IVacinaService, VacinaService>();
            services.AddScoped<IMedicamentoService, MedicamentoService>();
            services.AddScoped<IAuditoriaService, AuditoriaService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddSingleton<ISenhaHasher, Pbkdf2SenhaHasher>();
            services.AddScoped<ITokenService, JwtTokenService>();

            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
                .AddDbContextCheck<AppDbContext>(
                    name: "oracle-database",
                    tags: new[] { "ready" })
                .AddCheck<MongoDbHealthCheck>(
                    name: "mongodb",
                    tags: new[] { "ready" })
                .AddCheck<MigrationsHealthCheck>(
                    name: "migrations",
                    tags: new[] { "startup" });

            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(TelemetryConstants.ServiceName);

            services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSource(TelemetryConstants.ServiceName)
                        .AddConsoleExporter();
                })
                .WithMetrics(meterProviderBuilder =>
                {
                    meterProviderBuilder
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddMeter(TelemetryConstants.MeterName)
                        .AddConsoleExporter();
                });

            return services;
        }

        private static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            var mongoSettings = new MongoDbSettings();
            mongoSettings.ConnectionString = configuration["MongoDbSettings:ConnectionString"] ?? mongoSettings.ConnectionString;
            mongoSettings.DatabaseName = configuration["MongoDbSettings:DatabaseName"] ?? mongoSettings.DatabaseName;
            mongoSettings.AuditoriaCollection = configuration["MongoDbSettings:AuditoriaCollection"] ?? mongoSettings.AuditoriaCollection;

            MongoDbMapping.Registrar();

            services.AddSingleton(mongoSettings);

            services.AddSingleton<IMongoClient>(_ =>
            {
                var clientSettings = MongoClientSettings.FromConnectionString(mongoSettings.ConnectionString);
                clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
                return new MongoClient(clientSettings);
            });

            services.AddSingleton<IMongoDatabase>(sp =>
                sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

            return services;
        }
    }
}