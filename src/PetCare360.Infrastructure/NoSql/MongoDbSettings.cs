namespace PetCare360.Infrastructure.NoSql
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "petcare360";
        public string AuditoriaCollection { get; set; } = "auditoria";
    }
}