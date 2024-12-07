using MongoDB.Driver;
using SocialNetwork.DAL.Repositories.Base;
using SocialNetwork.DAL.Resources;
using SocialNetwork.DTO.Entities;

namespace SocialNetwork.DAL.Repositories
{
    public class UserRepository : BaseRepository
    {
        public UserRepository(ResourceDbContext context) : base(context)
        {

        }

        public Task CreateAsync(string userId, Profile profile)
        {
            var user = new User() { Id = userId, Profile = profile };

            return CreateAsync(user);
        }

        public Task CreateAsync(string userId, string name, string gender, string image)
        {
            var user = new User
            {
                Id = userId,
                Profile = new Profile(name, image, gender)
            };

            return CreateAsync(user);
        }

        public async Task CreateAsync(User user)
        {
            await Task.WhenAll(_context.Users.InsertOneAsync(user),
                _context.Wall.InsertOneAsync(new Feed { UserId = user.Id }),
                _context.News.InsertOneAsync(new Feed { UserId = user.Id}),
                _context.Mess.InsertOneAsync(new Messengers { UserId = user.Id }));
        }

        public async Task<bool> FollowAsync(string userId, string destId)
        {
            var profile = await ProfileAsync(userId);

            if (profile != null)
            {
                if(!IsFollowed(userId, destId))
                {
                    var path = "Followers." + userId;

                    var update = Builders<User>.Update.Set(path, profile)
                        .Set(x => x.Meta.Updated, DateTime.UtcNow);

                    var result = await _context.Users.UpdateOneAsync(x => x.Id == destId, update, new UpdateOptions()
                    {
                        IsUpsert = true
                    });

                    return result.ModifiedCount > 0;
                }
                else
                {
                    //var destUser = _context.Users.Find(u => u.Id == destId).FirstOrDefault();
                    //var kq = destUser.Followers.Remove(userId);

                    //if(kq)
                    //{
                    var update = Builders<User>.Update.Unset("Followers." + userId);


                        var result = await _context.Users.UpdateOneAsync(u => u.Id == destId, update);
                        return result.ModifiedCount > 0;
                    //}   
                }    
            }
            return false;
        }

        public bool IsFollowed(string userId, string destId)
        {
            var dsFollower = _context.Users.Find(u => u.Id == destId).SingleOrDefault().Followers.ToList();

            bool isFollowed = false;

            dsFollower.ForEach(u =>
            {
                if (u.Key == userId)
                    isFollowed = true;
            });
            return isFollowed;
        }

        public Task<Profile> ProfileAsync(string userId)
        {
            return _context.Users.Find(x => x.Id == userId).Project(x => x.Profile).SingleOrDefaultAsync();
        }


        public async Task<User> GetUserResourcesByIdAsync(string userId)
        {
            var user = (await _context.Users.FindAsync(user => user.Id == userId)).FirstOrDefault();

            return user;
        }

        public async Task<List<User>> GetAllUserResourcesAsync()
        {
            return await (await _context.Users.FindAsync(_ => true)).ToListAsync();
        }

        public async Task<List<User>> SearchUser(string userName)
        {
            var users =  (await _context.Users.FindAsync(user => user.Profile.Name.Contains(userName))).ToList();

            return users;
        }

        public async Task<List<string>> GetFollowings(string userId)
        {
            var followings = await _context.Users.AsQueryable().ToListAsync();

            List<string> followingsAsString = (from f in followings
                                               where f.Followers.Keys.Contains(userId)
                                               select f.Id).ToList();

            return followingsAsString;
        }


        public async Task<bool> ChangeAvatar(string userId, string url)
        {
            var user = await GetUserResourcesByIdAsync(userId);
            user.Profile.Image = url;

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);

            var result = await _context.Users.ReplaceOneAsync(filter, user);

            return result.ModifiedCount > 0;
        }
        
        public async Task<bool> ChangeBackGroundAsync(string userId, string url)
        {
            var user = await GetUserResourcesByIdAsync(userId);
            user.Profile.Background = url;

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);

            var result = await _context.Users.ReplaceOneAsync(filter, user);

            

            return result.ModifiedCount > 0;
        }

        public async Task<List<Owner>> GetListFollowings(IEnumerable<string> list_userId)
        {
            var filter = Builders<User>.Filter.In(u => u.Id, list_userId);

            var users = await _context.Users.Find(filter).ToListAsync();

            var followings = users.Select(u => new Owner(u.Id, u.Profile)).ToList();

            return followings;
        }
        public async Task<List<Owner>> GetListFollowers(string userId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);

            var users = await _context.Users.Find(filter).Project(u => u.Followers).FirstOrDefaultAsync();

            var followers = (from u in users
                             select new Owner { Id = u.Key, Name = u.Value.Name, Image = u.Value.Image, Background = u.Value.Background, Gender = u.Value.Gender })
                             .ToList();


            return followers;
        }
        
    }
}
