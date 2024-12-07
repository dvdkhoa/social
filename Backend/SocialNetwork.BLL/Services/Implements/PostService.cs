using SocialNetwork.DAL.Repositories;
using SocialNetwork.DTO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SocialNetwork.BLL.Services.Implements
{
    public class PostService : IPostService
    {
        private readonly PostRepository _postRepository;
        private readonly UserRepository _userRepository;
        private readonly NotifyRepository _notifyRepository;

        public PostService(PostRepository postRepository, UserRepository userRepository, NotifyRepository notifyRepository)
        {
            _postRepository = postRepository;
            _userRepository = userRepository;
            _notifyRepository = notifyRepository;
        }

        public async Task<Comment> CommentAsync(string userId, string postId, string text)
        {
            await _postRepository.CommentAsync(userId, postId, text);

            var post = await _postRepository.GetPostById(postId);
            var user = await _userRepository.GetUserResourcesByIdAsync(userId);

            var message = $"{user.Profile.Name} vừa bình luận bài đăng của bạn.";

            if(userId != post.By.Id)
                await _notifyRepository.CreateAsync(post.By.Id, message, user.Profile.Image, NotificationType.Comment, postId);

            var newComment = post.Comments.OrderByDescending(cmt => cmt.Ts).FirstOrDefault();

            return newComment;
        }

        public async Task<Post> CreatePostAsync(string userId, string text, params PostFile[]? photos)
        {
            await _postRepository.CreateAsync(userId, text, photos);

            return _postRepository.GetNewPost();
        }

        public Task DeletePostAsync(string postId)
        {
            return _postRepository.DeletePostAsync(postId);
        }

        public Task<List<Comment>> GetCommentsByPostId(string postId)
        {
            return _postRepository.GetCommentsByPostId(postId);
        }

        public Task<Feed> GetNewsById(string userId)
        {
            return _postRepository.GetNewsById(userId);
        }

        public async Task<List<Post>> GetNewsFeed(string userId, int page = 0)
        {
            return await _postRepository.GetNewsFeed(userId, page);
        }

        public Task<Post> GetPostById(string postId)
        {
            return _postRepository.GetPostById(postId);
        }

        public Task<Feed> GetWallById(string userId)
        {
            return _postRepository.GetWallById(userId);
        }

        public async Task<List<Post>> GetWallFeed(string userId, int page = 0)
        {
            return await _postRepository.GetWallFeed(userId, page);
        }
        public async Task<int> LikeAsync(string userId, string postId)
        {
            var post = await _postRepository.GetPostById(postId);
            var exists = await _postRepository.LikeExists(userId, postId);

            if(exists)
                await _postRepository.UnLikeAsync(userId, postId);
            else
            {
                await _postRepository.LikeAsync(userId, postId);

                var user = await _userRepository.GetUserResourcesByIdAsync(userId);


                var message = $"{user.Profile.Name} vừa like bài đăng của bạn.";

                if(userId != post.By.Id)
                    await _notifyRepository.CreateAsync(post.By.Id, message, user.Profile.Image, NotificationType.Like, postId);
            }    

            return post.Likes.Count();
        }

        public Task<Post?> ShareAsync(string userId, string postId)
        {
            return _postRepository.ShareAsync(userId, postId);
        }

        public Task UpdateNewsPosts(string userId, List<Post> posts)
        {
            return _postRepository.UpdateNewsPosts(userId, posts);
        }

        public Task UpdatePostAsync(string postId, string text)
        {
            return _postRepository.UpdatePostAsync(postId, text);
        }

        public Task UpdateWallPosts(string userId, List<Post> posts)
        {
            return _postRepository.UpdateWallPosts(userId, posts);
        }
    }
}
