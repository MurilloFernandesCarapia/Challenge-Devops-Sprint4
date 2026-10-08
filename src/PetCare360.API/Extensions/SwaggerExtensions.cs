using System.Reflection;
using Microsoft.OpenApi;

namespace PetCare360.API.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerComJwt(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "PetCare360 API",
                    Version = "v1",
                    Description = "API do PetCare 360 - Challenge FIAP 2026. Faça login em POST /api/Auth/login e use o botão Authorize para enviar o token."
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Cole aqui apenas o token retornado no login (sem a palavra Bearer)."
                });

                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });

            return services;
        }
    }
}