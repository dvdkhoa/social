using Microsoft.AspNetCore.Mvc;
using SocialNetwork.BLL.Services;

namespace SocialNetwork.Api.Controllers
{
    public class MessageController : Controller
    {
        private readonly IMessService _messService;

        public MessageController(IMessService messService)
        {
            _messService = messService;
        }

        //public Task<IActionResult> CreateMessageAsync()
        //{

        //}
    }
}
