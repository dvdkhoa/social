using MongoDB.Bson;
using MongoDB.Driver;
using SocialNetwork.DAL.Repositories.Base;
using SocialNetwork.DAL.Resources;
using SocialNetwork.DTO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DAL.Repositories
{
    public class FeedRepository : BaseRepository
    {
        public FeedRepository(ResourceDbContext context) : base(context)
        {

        }
        const int PAGE_SIZE = 100;

        public Task<List<Post>> GetNewsFeed(string userId, int page = 0)
        {
            return GetFeed(_context.News, userId, page * PAGE_SIZE, PAGE_SIZE);
        }

        public Task<List<Post>> GetWallFeed(string userId, int page = 0)
        {
            return GetFeed(_context.Wall, userId, page * PAGE_SIZE, PAGE_SIZE);
        }

        async Task<List<Post>> GetFeed(IMongoCollection<Feed> collection, string userId, int skip, int limit)
        {
            PipelineDefinition<Feed, Feed> pipeline = new[]
            {
                new BsonDocument {{"$match", new BsonDocument {{nameof(Feed.UserId), userId}}}},
                new BsonDocument {{"$unwind", "$Posts"}},
                new BsonDocument {{"$sort", new BsonDocument {{"Posts.Meta.Created", -1}}}},
                new BsonDocument {{"$skip", skip}},
                new BsonDocument {{"$limit", limit}},
                new BsonDocument {{"$group", new BsonDocument {{"_id", BsonNull.Value}, {"Posts", new BsonDocument("$push", "$Posts")}}}},
                new BsonDocument {{"$project", new BsonDocument {{"_id", 0}, {"Posts", 1}}}}
            };
        
            return (await collection.AggregateAsync(pipeline)).FirstOrDefault()?.Posts; 
        }

        public async Task AppendPostAsync(Post post)
        {
            var wallDest = new List<string> { post.By.Id };
            var newsDest = new List<string> { post.By.Id };

            var followers = await _context.Users.Find(x => x.Id == post.By.Id).Project(x => x.Followers).ToListAsync();
            if (followers.All(x => x.Any()))
                newsDest.AddRange(followers.SelectMany(x => x.Keys));

            var kq = await Task.WhenAll(AppendPostAsync(_context.Wall, wallDest, post),
                AppendPostAsync(_context.News, newsDest, post));
            Console.Write("alo");
        }

        public async Task<bool> AppendPostAsync(IMongoCollection<Feed> collection, List<string> destUsers, Post post)
        {
            var filter = Builders<Feed>.Filter.In(nameof(Feed.UserId), destUsers);
            var update = Builders<Feed>.Update.Push(nameof(Feed.Posts), post);
            var result = await collection.UpdateManyAsync(filter, update, new UpdateOptions { IsUpsert = true });
            return result.ModifiedCount > 0;
        }

        public async Task AppendCommentAsync(string postId, Comment comment)
        {
            await Task.WhenAll(
                _context.Wall.UpdateManyAsync(
                    Builders<Feed>.Filter.Eq($"{nameof(Feed.Posts)}._id", ObjectId.Parse(postId)),
                    Builders<Feed>.Update.Push($"{nameof(Feed.Posts)}.$.{nameof(Post.Comments)}", comment)),
                _context.News.UpdateManyAsync(
                    Builders<Feed>.Filter.Eq($"{nameof(Feed.Posts)}._id", ObjectId.Parse(postId)),
                    Builders<Feed>.Update.Push($"{nameof(Feed.Posts)}.$.{nameof(Post.Comments)}", comment))
            );
        }

        public async Task AppendLikeAsync(string postId, Like like)
        {
            await Task.WhenAll(
                _context.Wall.UpdateManyAsync(
                    Builders<Feed>.Filter.Eq($"{nameof(Feed.Posts)}._id", ObjectId.Parse(postId)),
                    Builders<Feed>.Update.AddToSet("Posts.$.Likes", like)),
                _context.News.UpdateManyAsync(
                    Builders<Feed>.Filter.Eq($"{nameof(Feed.Posts)}._id", ObjectId.Parse(postId)),
                    Builders<Feed>.Update.AddToSet("Posts.$.Likes", like))
                );
        }
        public async Task AppendUnLikeAsync(string postId, string userId)
        {

            var obId = ObjectId.Parse(postId);

            var filter = Builders<Feed>.Filter.Eq($"{nameof(Feed.Posts)}._id", obId);

            var update = new BsonDocument { { "$pull",
                             new BsonDocument{{"Posts.$.Likes",
                                 new BsonDocument { { "By._id", userId} } }}} };

            await Task.WhenAll(
                _context.Wall.UpdateManyAsync(filter, update),
                _context.News.UpdateManyAsync(filter, update));
        }

        public async Task AppendUpdatePostAsync(ObjectId postId, Post post)
        {
            await Task.WhenAll(
                _context.Wall.UpdateManyAsync(
                    Builders<Feed>.Filter.Eq("Posts._id", postId),
                    Builders<Feed>.Update.Set("Posts.$", post)),    
                _context.News.UpdateManyAsync(
                    Builders<Feed>.Filter.Eq("Posts._id", postId),
                    Builders<Feed>.Update.Set("Posts.$", post))
            );
        }

        public async Task AppendDeletePostAsync(ObjectId postId)
        {
            var update = new BsonDocument
            {
                { "$pull", new BsonDocument{ { "Posts", new BsonDocument { { "_id", postId } } } } }
            };

            await Task.WhenAll(
                _context.News.UpdateManyAsync(
                        Builders<Feed>.Filter.Eq("Posts._id", postId),
                        update),
                _context.Wall.UpdateManyAsync(
                            Builders<Feed>.Filter.Eq("Posts._id", postId),
                            update
           ));
        }
    }
}
