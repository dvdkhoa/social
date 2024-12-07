using SocialNetwork.DTO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.BLL.Services
{
    public interface INotifyService
    {
        Task CreateAsync(string userId, string message, string thumnail, NotificationType type, string intent);
        Task<List<Notification>> GetNotifycationByUserAsync(string userId);

    }
}
