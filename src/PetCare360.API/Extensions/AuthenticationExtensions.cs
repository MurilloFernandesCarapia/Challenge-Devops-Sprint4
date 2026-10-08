using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PetCare360.Infrastructure.Security;

namespace PetCare360.API.Extensions
{
    public static class AuthenticationExtensions
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtSettings>>((options, jwtSettings) =>
                {
                    var settings = jwtSettings.Value;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = settings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = settings.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            context.Response.Headers["WWW-Authenticate"] = "Bearer";
                            await EscreverProblemaAsync(
                                context.Response,
                                StatusCodes.Status401Unauthorized,
                                "Não autenticado",
                                "Envie um token JWT válido no header Authorization: Bearer {token}.");
                        },
                        OnForbidden = async context =>
                        {
                            await EscreverProblemaAsync(
                                context.Response,
                                StatusCodes.Status403Forbidden,
                                "Acesso negado",
                                "Seu perfil não tem permissão para acessar este recurso.");
                        }
                    };
                });

            services.AddAuthorization();

            return services;
        }

        private static async Task EscreverProblemaAsync(HttpResponse response, int statusCode, string titulo, string detalhe)
        {
            response.StatusCode = statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = titulo,
                Detail = detalhe,
                Instance = $"{response.HttpContext.Request.Method} {response.HttpContext.Request.Path}"
            };

            await response.WriteAsJsonAsync(problemDetails, JsonOptions, "application/problem+json");
        }
    }
}