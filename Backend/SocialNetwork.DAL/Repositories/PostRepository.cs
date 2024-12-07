using MongoDB.Bson;
using MongoDB.Driver;
using SocialNetwork.DAL.Repositories.Base;
using SocialNetwork.DAL.Resources;
using SocialNetwork.DTO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DAL.Repositories
{
    public class PostRepository : FeedRepository
    {
        public PostRepository(ResourceDbContext context) : base(context)
        {

        }

        public Task CreateAsync(string userId, string text, params PostFile[] photos)
        {
            var profile = _context.Users.Find(x => x.Id == userId)
                .Project(x => x.Profile)
                .SingleOrDefault();

            if (profile != null)
            {
                if (photos?.Any(x => x.Url != null) == true)
                    return CreateAsync(new Owner(userId, profile), text, photos.ToList());

                return CreateAsync(new Owner(userId, profile), text);
            }

            return Task.CompletedTask;

        }

        public Task CreateAsync(Owner owner, string text, List<PostFile> files)
        {
            if (files == null)
                throw new ArgumentNullException(nameof(files));

            var post = new Post
            {
                By = owner,
                Detail = new Detail { Text = text, PostFiles = files },
                Type = PostType.Photo,
            };

            return CreateAsync(post);
        }

        public async Task CreateAsync(Owner owner, string text)
        {
            var post = new Post
            {
                By = owner,
                Detail = new Detail { Text = text },
                Type = PostType.Status
            };

            await CreateAsync(post);
        }

        async Task CreateAsync(Post post)
        {
            await _context.Posts.InsertOneAsync(post);

            await AppendPostAsync(post);
        }

        public Task CommentAsync(string userId, string postId, string text)
        {
            var profile = _context.Users.Find(x => x.Id == userId)
                .Project(x => x.Profile).SingleOrDefault();

            if (profile != null)
                return CommentAsync(new Owner(userId, profile), postId, text);

            return Task.CompletedTask;
        }

        public async Task CommentAsync(Owner user, string postId, string text)
        {
            var comment = new Comment { By = user, Text = text, Ts = DateTime.UtcNow };

            await _context.Posts.UpdateOneAsync(
                Builders<Post>.Filter.Eq("_id", ObjectId.Parse(postId)),
                Builders<Post>.Update.Push(nameof(Post.Comments), comment));

            await AppendCommentAsync(postId, comment);
        }

        public async Task LikeAsync(string userId, string postId)
        {
            var profile = getProfileByUserId(userId);

            if (profile != null)
            {
                var owner = new Owner(userId, profile);
                var like = new Like { By=owner, Ts = DateTime.UtcNow };

                await _context.Posts.UpdateOneAsync(
                        Builders<Post>.Filter.Eq("_id", ObjectId.Parse(postId)),
                        Builders<Post>.Update.AddToSet("Likes", like));
                await AppendLikeAsync(postId, like);
            }
        }

        public async Task<bool> LikeExists(string userId, string postId)
        {
            var profile = getProfileByUserId(userId);

            //var postFilter = new BsonDocument { { "_id", ObjectId.Parse(postId) } };
            //var likeFilter = new BsonDocument { { "Likes.By._id", userId } };

            var postFilter = Builders<Post>.Filter.Eq("_id", ObjectId.Parse(postId));
            var likeFilter = Builders<Post>.Filter.Eq("Likes.By._id", userId);  
            

            var posts = await _context.Posts.FindAsync(Builders<Post>.Filter.And(postFilter,likeFilter));

            return posts.Any();
        }
        public async Task UnLikeAsync(string userId, string postId)
        {
            var profile = getProfileByUserId(userId);

            if (profile != null)
            {

                var update = new BsonDocument
                        {
                            { "$pull", new BsonDocument{ { "Likes", new BsonDocument { { "By._id", userId } } } } }
                        };

                await _context.Posts.UpdateOneAsync(
                        Builders<Post>.Filter.Eq("_id", ObjectId.Parse(postId)), update);
                await AppendUnLikeAsync(postId, userId);
            }
        }
        private Profile getProfileByUserId(string userId)
        {
            return _context.Users.Find(user => user.Id == userId)
                                        .Project(user => user.Profile)
                                        .SingleOrDefault();
        }

        public Task<Post> GetPostById(string postId)
        {
            return _context.Posts.Find(post => post.Id == ObjectId.Parse(postId)).FirstOrDefaultAsync();
        }

        public Post GetNewPost()
        {
            var newPost = _context.Posts.AsQueryable().OrderByDescending(p => p.Meta.Created).FirstOrDefault();

            return newPost;
        }


        public async Task<List<Comment>> GetCommentsByPostId(string postId)
        {
            var post = await (await _context.Posts.FindAsync(p => p.Id == ObjectId.Parse(postId))).FirstOrDefaultAsync();


            return post.Comments;
        }


        public async Task UpdatePostAsync(string postId, string text)
        {
            var post = await (await _context.Posts.FindAsync(p => p.Id == ObjectId.Parse(postId))).FirstOrDefaultAsync();
            if (post is null)
                return;
            post.Detail.Text = text;
            post.Meta.Updated = DateTime.Now;

            FilterDefinition<Post> filter = Builders<Post>.Filter.Eq(p => p.Id, post.Id);

            await _context.Posts.ReplaceOneAsync(filter, post);
            await AppendUpdatePostAsync(post.Id, post);
        }

        public async Task DeletePostAsync(string postId)
        {
            var objectPostId = ObjectId.Parse(postId);
            FilterDefinition<Post> filter = Builders<Post>.Filter.Eq(p => p.Id, objectPostId);
            await _context.Posts.DeleteOneAsync(filter);
            await AppendDeletePostAsync(objectPostId);
        }


        public async Task<Feed> GetWallById(string userId) => await _context.Wall.Find(w => w.UserId == userId).Limit(1).SingleOrDefaultAsync();
        //public async Task<Feed> GetWallById(string userId)
        //{
        //    //var wall = await _context.Wall.Find(Builders<Feed>.Filter.Eq(w => w.UserId, userId)).FirstOrDefaultAsync();

        //    var wall = _context.Wall.Find(Builders<Feed>.Filter.Empty).ToList();

        //    return wall[0];
        //}


        public async Task<Feed> GetNewsById(string userId) => await _context.News.Find(w => w.UserId == userId).Limit(1).FirstOrDefaultAsync();


        public async Task UpdateWallPosts(string userId, List<Post> posts)
        {
            var filter = Builders<Feed>.Filter.Eq(w => w.UserId, userId);

            var update = Builders<Feed>.Update.Set("Posts", posts);

            var results = await _context.Wall.UpdateManyAsync(filter, update);
        }
        public async Task UpdateNewsPosts(string userId, List<Post> posts)
        {
            var filter = Builders<Feed>.Filter.Eq(w => w.UserId, userId);

            var update = Builders<Feed>.Update.Set("Posts", posts);

            var results = await _context.News.UpdateManyAsync(filter, update);
        }

        public async Task ShareAsync(string userId, string postId)
        {
            var filter = Builders<Post>.Filter.Eq("_id", ObjectId.Parse(postId));
            var post = await (await _context.Posts.FindAsync(filter)).FirstOrDefaultAsync();

            var profile = _context.Users.Find(x => x.Id == userId)
                .Project(x => x.Profile)
                .SingleOrDefault();

            if (post == null)
                return;

            Post newPost = new Post
            {
                By = new Owner(userId, profile),
                Share = new Share
                {
                    OriginOwner = post.By,
                    OriginPostId = post.Id
                },
                Comments = post.Comments,
                Likes = post.Likes,
                Detail = post.Detail,
                Meta = post.Meta,
                Type = PostType.Share,
            };
            await CreateAsync(newPost);
        }
    }
}
