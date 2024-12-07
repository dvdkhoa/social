using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DTO.Entities
{
    public class Comment
    {
        public Owner By { get; set; }
        public DateTime Ts { get; set; }
        public string Text { get; set; }
    }

}
