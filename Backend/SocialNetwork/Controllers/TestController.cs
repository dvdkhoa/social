using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Api.Helpers;

namespace SocialNetwork.Api.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult getTest()
        {
            return Ok("Hello world");
        }
        //[HttpPost]
        //public IActionResult CreateTest(string naem)
        //{
        //    return Ok(naem);
        //}

        [HttpPost]
        public async Task<IActionResult> UploadFileToCloudinary(IFormFile formFile) 
        {
            
            return Ok(await CloudinaryHelper.UploadFileToCloudinary(formFile));
        }
    }
}
