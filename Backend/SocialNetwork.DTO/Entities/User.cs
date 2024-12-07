using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DTO.Entities
{
    public class User : Base.Entity<string>
    {
        public Profile Profile { get; set; }

        public Dictionary<string, Profile> Followers { get; set; } = new Dictionary<string, Profile>();

        public List<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
