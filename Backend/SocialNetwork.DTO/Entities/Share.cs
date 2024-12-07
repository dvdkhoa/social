using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DTO.Entities
{
    public class Share
    {
        public ObjectId? OriginPostId { get; set; }
        public Owner? OriginOwner { get; set; }
    }
}
