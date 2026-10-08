using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare360.API.Middleware;
using PetCare360.Domain.Exceptions;

namespace PetCare360.API.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, titulo, detalhe) = MapearExcecao(exception);

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Erro inesperado ao processar {Metodo} {Caminho}",
                    httpContext.Request.Method, httpContext.Request.Path);
            }
            else
            {
                _logger.LogWarning("Requisição {Metodo} {Caminho} recusada com status {StatusCode}: {Mensagem}",
                    httpContext.Request.Method, httpContext.Request.Path, statusCode, exception.Message);
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = titulo,
                Detail = detalhe,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            };

            problemDetails.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;

            if (httpContext.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var valor) && valor is string correlationId)
            {
                problemDetails.Extensions["correlationId"] = correlationId;

                httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName] = correlationId;
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, JsonOptions, "application/problem+json", cancellationToken);

            return true;
        }

        private static (int StatusCode, string Titulo, string Detalhe) MapearExcecao(Exception exception)
        {
            return exception switch
            {
                RegraDeNegocioException => (
                    StatusCodes.Status400BadRequest,
                    "Regra de negócio violada",
                    exception.Message),

                CredenciaisInvalidasException => (
                    StatusCodes.Status401Unauthorized,
                    "Não autorizado",
                    exception.Message),

                DbUpdateException => (
                    StatusCodes.Status409Conflict,
                    "Conflito ao gravar os dados",
                    "A operação viola uma restrição do banco: registro duplicado (CPF, e-mail ou CNPJ) ou vinculado a outros dados."),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Erro interno no servidor",
                    "Ocorreu um erro inesperado. Informe o correlationId ao suporte.")
            };
        }
    }
}