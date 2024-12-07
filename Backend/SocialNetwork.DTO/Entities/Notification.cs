    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DTO.Entities
{
    public class Notification
    {
        public string? Message { get; set; }
        public string? Thumbnail { get; set; }
        public string? Intent { get; set; }
        public bool Seen { get; set; }
    }
}
