using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SocialNetwork.DAL;
using SocialNetwork.DAL.Identity.Models;
using SocialNetwork.DAL.Repositories;
using SocialNetwork.DTO.Account;
using SocialNetwork.DTO.Extensions;
using SocialNetwork.DTO.Shared;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.BLL.Services.Implements
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly UserRepository _userRepository;
        private readonly AppSettings _appSettings;
        private readonly IPostService _postService;




        public AccountService(UserManager<User> userManager, UserRepository userRepository, IOptionsMonitor<AppSettings> appSettings, IPostService postService)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _appSettings = appSettings.CurrentValue;
            _postService = postService;
        }

        public async Task<bool> ChangeAvatar(string userId, string url)
        {
            var wall = await _postService.GetWallById(userId);
            var news = await _postService.GetNewsById(userId);

           if(wall != null)
            {
                wall.Posts.ForEach(post =>
                {
                    if (post.By.Id == userId)
                    {
                        post.By.Image = url;
                    }
                });

            }
            if (news != null)
            {
                news.Posts.ForEach(post =>
                {
                    if (post.By.Id == userId)
                    {
                        post.By.Image = url;
                    }
                });
            }
            Task.WaitAll(
                _userRepository.ChangeAvatar(userId, url),
                _postService.UpdateWallPosts(userId, wall.Posts),
                _postService.UpdateNewsPosts(userId, wall.Posts)
            );

            return true;
        }

        public Task<bool> ChangeBackGroundAsync(string userId, string url)
        {
            return _userRepository.ChangeBackGroundAsync(userId, url);
        }

        public async Task<IdentityResult> CreateUserAsync(RegisterModel registerModel)
        {
            var user = new User()
            {
                UserName = registerModel.Email,
                Email = registerModel.Email,
                Name = registerModel.Name
            };
            

            IdentityResult result = await _userManager.CreateAsync(user, registerModel.Password);
            if (result.Succeeded)
            {
                //await _userManager.AddClaimsAsync(user, new List<Claim>
                //                                        {
                //                                            new Claim(ClaimTypes.GivenName, registerModel.Name),
                //                                            new Claim(ClaimTypes.DateOfBirth, registerModel.BirthDay.ToString()),
                //                                            new Claim(ClaimTypes.Email, registerModel.Email),
                //                                        });
                await _userRepository.CreateAsync(user.Id, user.Name, registerModel.Gender, registerModel.Image);
            }

            return result;
        }

        public async Task<bool> FollowAsync(string userId, string destId)
        {
            return await _userRepository.FollowAsync(userId, destId);
        }

        public Task<List<DTO.Entities.User>> GetAllUser()
        {
            return _userRepository.GetAllUserResourcesAsync();
        }

        public List<string> GetFollowings(string userId)
        {
            return _userRepository.GetFollowings(userId);
        }

        public Task<DTO.Entities.Profile> GetProfileByIdAsync(string userId)
        {
            return _userRepository.ProfileAsync(userId);
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public Task<DTO.Entities.User> GetUserResourcesById(string userId)
        {
            return _userRepository.GetUserResourcesByIdAsync(userId);
        }

        public async Task<ApiResponse> LoginAsync(LoginModel loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.UserName);
            if(user == null)
            {
                return new ApiResponse {
                    IsSuccess = false,
                    Message = "Username not exists"
                };
            }
            var result =  await _userManager.CheckPasswordAsync(user, loginModel.Password);
            if(!result)
            {
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Username or password not valid."
                };
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var token = GenerateToken(claims);

            var userResources = await GetUserResourcesById(user.Id);

            return new ApiResponse
            {
                IsSuccess = true,
                Message = "Login successfully",
                Data = new
                {
                    token = token,
                    userId = user.Id,
                    Profile = await this.GetProfileByIdAsync(user.Id),
                    Followers = userResources.Followers,
                }
            };
        }

        public Task<List<DTO.Entities.User>> SearchUser(string userName)
        {
            return _userRepository.SearchUser(userName);
        }

        private string GenerateToken(IEnumerable<Claim> claims)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretKeyBytes = Encoding.UTF8.GetBytes(_appSettings.Secretkey);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(secretKeyBytes),
                                SecurityAlgorithms.HmacSha512Signature),
               
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription);


            return jwtTokenHandler.WriteToken(token);
        }
    }
}
