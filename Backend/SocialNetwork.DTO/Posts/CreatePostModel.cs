using Microsoft.AspNetCore.Http;
using SocialNetwork.DTO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DTO.Posts
{
    public class CreatePostModel
    {
        public string UserId { get; set; }
        public string Text { get; set; }
        //public Photo[] Photos { get; set; }
        public IFormFile? PhotoFile { get; set; }
    }
}
