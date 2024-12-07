using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DTO.Entities
{
    public class Detail
    {
        public string Text { get; set; }
        public List<PostFile> PostFiles { get; set; } = new List<PostFile>();
    }
}
