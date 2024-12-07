using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DTO.Entities
{
    public class Messengers : Base.Entity<ObjectId>
    {
        public string UserId { get; set; }
        public Dictionary<string, Messenger> Messenger = new Dictionary<string, Messenger>();
    }
}
