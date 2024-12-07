using SocialNetwork.DAL.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DAL.Repositories.Base
{
    public abstract class BaseRepository
    {
        protected ResourceDbContext _context { get; }

        protected BaseRepository(ResourceDbContext context)
        {
            _context = context;
        }
    }
}
