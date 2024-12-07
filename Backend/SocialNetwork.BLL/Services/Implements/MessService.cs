using SocialNetwork.DAL.Repositories;
using SocialNetwork.DTO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.BLL.Services.Implements
{
    public class MessService : IMessService
    {
        private readonly MessRepository _messRepository;
        public MessService(MessRepository messRepository)
        {
            _messRepository = messRepository;
        }
        public Task<bool> CreateMessage(string userId, string destId, Message message)
        {
            return _messRepository.CreateMessage(userId, destId, message);
        }
    }
}
