using SocialNetwork.DTO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.BLL.Services
{
    public interface IPostService
    {
        public Task<Post> GetPostById(string postId);
        public Task<List<Post>> GetNewsFeed(string userId, int page = 0);
        public Task<List<Post>> GetWallFeed(string userId, int page = 0);
        public Task<Post> CreatePostAsync(string userId, string text, params PostFile[]? photos);
        public Task<Comment> CommentAsync(string userId, string postId, string text);
        public Task<int> LikeAsync(string userId, string postId);
        public Task<List<Comment>> GetCommentsByPostId(string postId);
        public Task UpdatePostAsync(string postId, string text);
        public Task DeletePostAsync(string postId);
        public Task<Feed> GetWallById(string userId);
        public Task<Feed> GetNewsById(string userId);

        public Task UpdateWallPosts(string userId, List<Post> posts);
        public Task UpdateNewsPosts(string userId, List<Post> posts);
        public Task<Post?> ShareAsync(string userId, string postId);


    }
}
