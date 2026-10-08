using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetCare360.Domain.Entities;
using PetCare360.Domain.Interfaces;

namespace PetCare360.Infrastructure.Data
{
    public static class AdminSeeder
    {
        public static async Task CriarAdminPadraoAsync(this IServiceProvider services, IConfiguration configuration)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var senhaHasher = scope.ServiceProvider.GetRequiredService<ISenhaHasher>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(AdminSeeder));

            var email = (configuration["AdminPadrao:Email"] ?? "admin@petcare360.com").ToLower();
            var senha = configuration["AdminPadrao:Senha"] ?? "Admin@123";

            try
            {
                bool adminExiste = await dbContext.Usuarios.AnyAsync(u => u.Email == email);
                if (adminExiste)
                {
                    return;
                }

                dbContext.Usuarios.Add(new Usuario
                {
                    NmUsuario = "Administrador",
                    Email = email,
                    SenhaHash = senhaHasher.GerarHash(senha),
                    Perfil = PerfilUsuario.Admin,
                    DtCriacao = DateTime.UtcNow
                });

                await dbContext.SaveChangesAsync();

                logger.LogInformation("Usuário administrador padrão criado: {Email}", email);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Não foi possível criar o administrador padrão. Verifique se o banco está no ar e se as migrations foram aplicadas.");
            }
        }
    }
}