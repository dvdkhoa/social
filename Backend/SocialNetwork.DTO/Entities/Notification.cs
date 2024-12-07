using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DTO.Entities
{
    public class Notification : Base.Entity<ObjectId>
    {
        public string? Message { get; set; }
        public string? Thumbnail { get; set; }

        [BsonRepresentation(BsonType.String)]
        public NotificationType Type { get; set; }

        public string? IntentId { get; set; }
        public bool Seen { get; set; }

        public Notification()
        {
            Id = ObjectId.GenerateNewId();
        }
    }
    
    public enum NotificationType
    {
        Like,
        Comment,
        Follow,
        Other
    }
}
