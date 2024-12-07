using SocialNetwork.DAL.Repositories;
using SocialNetwork.DAL.Resources;
using Xunit;

namespace SocialNetwork.Tests
{
    public class UserRepositoryTest
    {
        [Fact]
        public async void Follow()
        {
            var context = GetContext();

            var currentUser = "ea8db189-5faf-441b-a344-59f29e960314";
            var destUser = "1f443aba-6cd8-4bd2-bf36-a853711e7451";
            await GetRepo().FollowAsync(currentUser, destUser);
            //if (await new UserRepository(context).FollowAsync(currentUser, destUser))
            //    await new FeedRepository(context).AppendFollowingPostAsync(currentUser, destUser);
        }

        [Fact]
        public async void UnLike()
        {
            await getPostRepo().UnLikeAsync("50f73725-3848-4510-bb35-c89c4b4fb3ef", "6242c939a75449aeaacb0359");
        }

        static ResourceDbContext GetContext()
        {
            return new ResourceDbContext("mongodb://localhost:27017", "socialnetworktest");
        }

        static UserRepository GetRepo()
        {
            var repo = new UserRepository(GetContext());
            return repo;
        }
        static PostRepository getPostRepo() => new PostRepository(GetContext());
    }
}