using MongoDB.Driver;
using Microsoft.Extensions.Options;
using StudyMentorApi.Extensions;

namespace StudyMentorApi.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IOptions<MongoDbSettings> settings)
        {
            var config = settings.Value;

            if (string.IsNullOrWhiteSpace(config.ConnectionString))
                throw new InvalidOperationException("Mongo connection string is not configured.");

            if (string.IsNullOrWhiteSpace(config.DatabaseName))
                throw new InvalidOperationException("Mongo database name is not configured.");

            var client = new MongoClient(config.ConnectionString);
            _database = client.GetDatabase(config.DatabaseName);
        }

        public IMongoDatabase Database => _database;
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}
