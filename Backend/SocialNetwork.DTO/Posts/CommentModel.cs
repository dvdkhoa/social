using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DTO.Posts
{
    public class CommentModel
    {
        public string UserId { get; set; }
        public string PostId { get; set; }
        public string Text { get; set; }
    }
}
