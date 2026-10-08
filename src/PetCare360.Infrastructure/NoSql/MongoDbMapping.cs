using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using PetCare360.Domain.Entities;

namespace PetCare360.Infrastructure.NoSql
{
    public static class MongoDbMapping
    {
        private static readonly object Trava = new();

        public static void Registrar()
        {
            lock (Trava)
            {
                if (BsonClassMap.IsClassMapRegistered(typeof(RegistroAuditoria)))
                {
                    return;
                }

                BsonClassMap.RegisterClassMap<RegistroAuditoria>(map =>
                {
                    map.AutoMap();
                    map.SetIgnoreExtraElements(true);
                    map.MapIdMember(r => r.Id)
                        .SetIdGenerator(StringObjectIdGenerator.Instance)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
            }
        }
    }
}