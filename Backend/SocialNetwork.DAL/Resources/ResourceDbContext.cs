using MongoDB.Driver;
using SocialNetwork.DTO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DAL.Resources
{
    public class ResourceDbContext
    {
        private readonly IMongoClient mongoClient;

        private readonly IMongoDatabase mongoDatabase;

        public ResourceDbContext(string connectionString, string dbName)
        {
            mongoClient = new MongoClient(connectionString);
            mongoDatabase = mongoClient.GetDatabase(dbName);
        }
        public IMongoCollection<User> Users => mongoDatabase.GetCollection<User>("users");
        public IMongoCollection<Post> Posts => mongoDatabase.GetCollection<Post>("posts");
        public IMongoCollection<Feed> Wall => mongoDatabase.GetCollection<Feed>("wall");
        public IMongoCollection<Feed> News => mongoDatabase.GetCollection<Feed>("news");
        public IMongoCollection<Messengers> Mess => mongoDatabase.GetCollection<Messengers>("mess");
    }
}
