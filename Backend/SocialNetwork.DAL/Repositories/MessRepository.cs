using MongoDB.Driver;
using SocialNetwork.DAL.Resources;
using SocialNetwork.DTO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DAL.Repositories
{
    public class MessRepository : Base.BaseRepository
    {
        public MessRepository(ResourceDbContext context) : base(context)
        { 
        }


        public async Task<bool> CreateMessage(string userId, string destId, Message message)
        {
            var filter = Builders<Messengers>.Filter.Eq(m => m.UserId, userId);

            //var update = Builders<Messengers>.Update.Push("Messages.", message);

            var update = Builders<Messengers>.Update.Push("Messages",  "");

            var results = await _context.Mess.UpdateOneAsync(filter, update);

            return results.ModifiedCount > 0;
        }
    }
}
