using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocialNetwork.Api.Helpers;
using SocialNetwork.BLL.Services;
using SocialNetwork.DAL.Identity.Models;
using SocialNetwork.DTO.Account;
using SocialNetwork.DTO.Extensions;
using SocialNetwork.DTO.Shared;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SocialNetwork.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly AppSettings _appSettings;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody]RegisterModel registerModel)
        {
            var result = await _accountService.CreateUserAsync(registerModel);

            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest(result.Errors);
        }

        [HttpGet("registerFake")]
        public async Task<IActionResult> RegisterWithFakeData()
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetStringAsync("https://randomuser.me/api/?results=100&nat=gb,us&inc=gender,name,email,picture");

                var results = JsonConvert.DeserializeObject<JObject>(response).Value<JArray>("results");

                string UpFirst(string input)
                {
                    return char.ToUpper(input[0]) + input.Substring(1);
                }

                foreach (var randUser in results)
                {
                    var gender = UpFirst(randUser.Value<string>("gender"));
                    var first = UpFirst(randUser.SelectToken("name.first").Value<string>());
                    var last = UpFirst(randUser.SelectToken("name.last").Value<string>());
                    var email = randUser.Value<string>("email");
                    var picture = randUser.SelectToken("picture.large").Value<string>();

                    var model = new RegisterModel()
                    {
                        Email = email,
                        Name = first + last,
                        Gender = gender,
                        Image = picture,
                        Password = "123123"
                    };

                    await Register(model);
                }
            }

            return this.Ok();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            var result = await _accountService.LoginAsync(loginModel);
            if (result.IsSuccess)
            {
                return Ok(result);
            }    
            return BadRequest(result);
        }


        [HttpPost("Follow")]
        public async Task<IActionResult> Follow(FollowModel followModel)
        {
            var result = await _accountService.FollowAsync(followModel.userId, followModel.destId);

            if(result)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("GetAllUser")]
        public async Task<IActionResult> GetUsersAsync()
        {
            return Ok( new
            {
                users = await _accountService.GetAllUser()
            });;
        }

        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser(string userId)
        {
            var userResources = await _accountService.GetUserResourcesById(userId);

            var followings = await _accountService.GetFollowings(userId);

            var apiUserResponse = new
            {
                userId = userResources.Id,
                userResources.Meta,
                userResources.Profile,
                userResources.Followers,
                followings
            };

            return Ok(apiUserResponse);
        }
        
        [HttpGet("SearchUser")]
        public async Task<IActionResult> SearchUser(string userName)
        {
            return Ok(await _accountService.SearchUser(userName));
        }


        [HttpPut("ChangeAvatar")]
        public async Task<IActionResult> ChangeAvatarAsync(string userId, IFormFile file)
        {
            bool result = false;

            if (userId == null || file == null)
                return BadRequest();

            var user = await _accountService.GetUserResourcesById(userId);
            if (user is null)
                return BadRequest();

            var url = await CloudinaryHelper.UploadFileToCloudinary(file);
            if(url != null)
            {
                result = await _accountService.ChangeAvatar(userId, url);
            }

            return Ok(new ApiResponse
            {
                Data = url,
                IsSuccess = true,
                Message = "Change avatar successfully"
            });
        }

        [HttpPut("ChangeBackground")]
        public async Task<IActionResult> ChangeBackgroundAsync(string userId, IFormFile file)
        {
            bool result = false;

            if (userId == null || file == null)
                return BadRequest();

            var user = await _accountService.GetUserResourcesById(userId);
            if (user is null)
                return BadRequest();

            var url = await CloudinaryHelper.UploadFileToCloudinary(file);
            if (url != null)
            {
                result = await _accountService.ChangeBackGroundAsync(userId, url);
            }

            return Ok(new ApiResponse
            {
                Data = url,
                IsSuccess = true,
                Message = "Change background successfully"
            });
        }

        [HttpPost("GetFollowings")]
        public async Task<IActionResult> GetListFollowings(String userId)
        {
            var followings = await _accountService.GetFollowings(userId);

            return Ok(followings);
        }
        [HttpPost("GetListFollowings")]
        public async Task<IActionResult> GetListFollowings(string[] list_userId)
        {
            var followings = await _accountService.GetListFollowings(list_userId);

            return Ok(followings);
        }
        [HttpPost("GetListFollowers")]  
        public async Task<IActionResult> GetListFollowers(String userId)
        {
            var followers = await _accountService.GetListFollowers(userId);

            return Ok(followers);
        }
    }
}
