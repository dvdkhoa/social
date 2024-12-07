using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using SocialNetwork.Api.Helpers;
using SocialNetwork.Api.Hubs;
using SocialNetwork.BLL.Services;
using SocialNetwork.BLL.Services.Implements;
using SocialNetwork.DTO.Entities;
using SocialNetwork.DTO.Posts;
using SocialNetwork.DTO.Shared;
using System.Collections;

namespace SocialNetwork.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<NotifyHub> _notifyHubContext;

        public PostController(IPostService postService, IWebHostEnvironment env, IHubContext<NotifyHub> notifyHubContext)
        {
            _postService = postService;
            _env = env;
            _notifyHubContext = notifyHubContext;
        }

        [HttpPost("CreatePost")]
        public async Task<IActionResult> CreatePostAsync([FromForm]CreatePostModel createPostModel)
        {
            List<PostFile>? list = null;
            if (createPostModel.PhotoFile?.Length > 0)
            {

                //string newName = Guid.NewGuid().ToString() + fileType;
                //var path = Path.Combine(_env.WebRootPath, "uploads", newName);

                //using (var stream = new FileStream(path, FileMode.Create))
                //{
                //    await createPostModel.PhotoFile.CopyToAsync(stream);
                //}
                string? fileType = Path.GetExtension(createPostModel.PhotoFile.FileName);

                string fileName = await CloudinaryHelper.UploadFileToCloudinary(createPostModel.PhotoFile, fileType);

                list = new List<PostFile>
                {
                    new PostFile
                    {
                        Id = Guid.NewGuid().ToString(),
                        Url = fileName,
                        FileType = fileType == ".mp4" ? FileType.Video : FileType.Image
                    }
                };
            }

            var newPost = await _postService.CreatePostAsync(createPostModel.UserId, createPostModel.Text, list?.ToArray());

            var newPostViewModel = new
            {
                id = newPost.Id.ToString(),
                newPost.By,
                newPost.Type,
                newPost.Meta,
                newPost.Detail,
                newPost.Comments,
                newPost.Likes
            };
            

            await _notifyHubContext.Clients.All.SendAsync("receiveMessage", $"Tài khoản có userId: {createPostModel.UserId} đăng tải {createPostModel.Text}");

            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Data = newPostViewModel,
                Message = "Create post successfully"
            });
        }

        [HttpGet("GetNews")]
        public async Task<IActionResult> GetNews(string userId)
        {
            var posts = await _postService.GetNewsFeed(userId, 0);

            var postsViewModel = posts?.Select(post =>
            {
                return new
                {
                    id = post.Id.ToString(),
                    post.By,
                    post.Type,
                    post.Meta,
                    post.Detail,
                    Share = new
                    {
                        OriginPostId = post.Share?.OriginPostId.ToString(),
                        OriginOwner = post.Share?.OriginOwner
                    },
                    post.Comments,
                    post.Likes
                };
            });
   

            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Data = postsViewModel
            });
        }

        [HttpGet("GetWall")]
        public async Task<IActionResult> GetWall(string userId)
        {
            var posts = await _postService.GetWallFeed(userId, 0);

            var postsViewModel = posts?.Select(post =>
            {
                return new
                {
                    id = post.Id.ToString(),
                    post.By,
                    post.Meta,
                    post.Type,
                    post.Detail,
                    Share = new {
                        OriginPostId = post.Share?.OriginPostId.ToString(),
                        OriginOwner = post.Share?.OriginOwner
                    },
                    post.Comments,
                    post.Likes
                };
            });
            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Data = postsViewModel
            });
        }

        [HttpPost("Comment")]
        public async Task<IActionResult> Comment([FromBody] CommentModel commentModel)
        {
            var newComment = await _postService.CommentAsync(commentModel.UserId, commentModel.PostId, commentModel.Text);
            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Data = newComment,
                Message = "Comment successfully"
            });
        }

        [HttpPost("Like")]
        public async Task<IActionResult> Like([FromBody] LikeModel likeModel)
        {
            var likeCount = await _postService.LikeAsync(likeModel.UserId, likeModel.PostId);
            return Ok(new ApiResponse
            {
                Data = new { likeCount = likeCount },
                IsSuccess = true
            });
        }


        [HttpGet("Comments")]
        public async Task<IActionResult> GetComments(string postId)
        {
            var comments = await _postService.GetCommentsByPostId(postId);

            return Ok(new ApiResponse
            {
                Data= comments,
                IsSuccess = true

            });
        }


        [HttpPut("UpdatePost")]
        public async Task<IActionResult> UpdatePostAsync(string postId, string text)
        {
            await _postService.UpdatePostAsync(postId, text);
            return Ok();
        }

        [HttpDelete("DeletePost")]
        public async Task<IActionResult> DeletePostAsync(string postId)
        {
            await _postService.DeletePostAsync(postId);
            return Ok();
        }


        private async Task<String> uploadVideo(IFormFile file)
        {
            // Get the server path, wwwroot
            string webRootPath = _env.WebRootPath;

            // Building the path to the uploads directory
            var fileRoute = Path.Combine(webRootPath, "uploads");

            // Get the mime type
            var mimeType = file.ContentType;
            //var mimeType = HttpContext.Request.Form.Files.GetFile("file").ContentType;

            // Get File Extension
            string extension = System.IO.Path.GetExtension(file.FileName);

            // Generate Random name.
            string name = Guid.NewGuid().ToString().Substring(0, 8) + extension;

            // Build the full path inclunding the file name
            string link = Path.Combine(fileRoute, name);

            // Create directory if it dose not exist.
            FileInfo dir = new FileInfo(fileRoute);
            dir?.Directory?.Create();

            // Basic validation on mime types and file extension
            string[] videoMimetypes = { "video/mp4", "video/webm", "video/ogg" };
            string[] videoExt = { ".mp4", ".webm", ".ogg" };

            try
            {
                if (Array.IndexOf(videoMimetypes, mimeType) >= 0 && (Array.IndexOf(videoExt, extension) >= 0))
                {
                    // Copy contents to memory stream.
                    Stream stream;
                    stream = new MemoryStream();
                    file.CopyTo(stream);
                    stream.Position = 0;
                    String serverPath = link;

                    // Save the file
                    using (FileStream writerFileStream = System.IO.File.Create(serverPath))
                    {
                        await stream.CopyToAsync(writerFileStream);
                        writerFileStream.Dispose();
                    }

                    // Return the file path as json
                    Hashtable videoUrl = new Hashtable();
                    videoUrl.Add("link", "/uploads/" + name);
                    return videoUrl.ToJson().ToString();
                }
                throw new ArgumentException("The video did not pass the validation");
            } catch(ArgumentException ex) {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        [HttpPost("UploadFiles")]
        [Produces("application/json")]
        public async Task<IActionResult> Post(List<IFormFile> files)
        {
            // Get the file from the POST request
            //var theFile = HttpContext.Request.Form.Files.GetFile("file");
            var theFile = files[0];

            // Get the server path, wwwroot
            string webRootPath = _env.WebRootPath;

            // Building the path to the uploads directory
            var fileRoute = Path.Combine(webRootPath, "uploads");

            // Get the mime type
            var mimeType = files[0].ContentType;
            //var mimeType = HttpContext.Request.Form.Files.GetFile("file").ContentType;

            // Get File Extension
            string extension = System.IO.Path.GetExtension(theFile.FileName);

            // Generate Random name.
            string name = Guid.NewGuid().ToString().Substring(0, 8) + extension;

            // Build the full path inclunding the file name
            string link = Path.Combine(fileRoute, name);

            // Create directory if it dose not exist.
            FileInfo dir = new FileInfo(fileRoute);
            dir.Directory.Create();

            // Basic validation on mime types and file extension
            string[] videoMimetypes = { "video/mp4", "video/webm", "video/ogg" };
            string[] videoExt = { ".mp4", ".webm", ".ogg" };

            try
            {
                if (Array.IndexOf(videoMimetypes, mimeType) >= 0 && (Array.IndexOf(videoExt, extension) >= 0))
                {
                    // Copy contents to memory stream.
                    Stream stream;
                    stream = new MemoryStream();
                    theFile.CopyTo(stream);
                    stream.Position = 0;
                    String serverPath = link;

                    // Save the file
                    using (FileStream writerFileStream = System.IO.File.Create(serverPath))
                    {
                        await stream.CopyToAsync(writerFileStream);
                        writerFileStream.Dispose();
                    }

                    // Return the file path as json
                    Hashtable videoUrl = new Hashtable();
                    videoUrl.Add("link", "/uploads/" + name);

                    return Ok(videoUrl);
                }
                throw new ArgumentException("The video did not pass the validation");
            }

            catch (ArgumentException ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost("Share")]
        public async Task<IActionResult> ShareAsync(string userId, string postId)
        {
            await _postService.ShareAsync(userId, postId);

            return Ok();
        }
    }
}

