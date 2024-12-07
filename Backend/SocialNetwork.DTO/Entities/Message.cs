using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DTO.Entities
{
    public class Message : Base.Entity<ObjectId>
    {
        public Owner By { get; set; }
        public Owner To { get; set; }
        public string Content { get; set; }
        public MessType MessType { get; set; }

    }
}
