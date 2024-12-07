using SocialNetwork.DTO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.BLL.Services
{
    public interface IMessService
    {
        Task<bool> CreateMessage(string userId, string destId, Message message);
    }
}
