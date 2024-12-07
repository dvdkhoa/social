using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DTO.Entities.Base
{
    public class Entity<T>
    {
        [BsonId]
        public T Id { get; set; }

        public Meta Meta { get; set; } = new Meta();
    }
}
