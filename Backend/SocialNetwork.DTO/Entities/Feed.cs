using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DTO.Entities
{
    public class Feed : Base.Entity<ObjectId>
    {
        public string UserId { get; set; }
        public List<Post> Posts { get; set; } = new List<Post>();
    }
}
