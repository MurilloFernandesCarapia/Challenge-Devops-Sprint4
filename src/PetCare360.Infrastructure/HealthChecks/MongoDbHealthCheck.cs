using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PetCare360.Infrastructure.HealthChecks
{
    public class MongoDbHealthCheck : IHealthCheck
    {
        private readonly IMongoDatabase _database;

        public MongoDbHealthCheck(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var comando = new BsonDocumentCommand<BsonDocument>(new BsonDocument("ping", 1));
                await _database.RunCommandAsync(comando, cancellationToken: cancellationToken);

                return HealthCheckResult.Healthy("MongoDB respondendo.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Não foi possível conectar no MongoDB.", ex);
            }
        }
    }
}