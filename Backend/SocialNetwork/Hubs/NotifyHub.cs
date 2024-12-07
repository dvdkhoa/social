using Microsoft.AspNetCore.SignalR;
using SocialNetwork.BLL.Services;

namespace SocialNetwork.Api.Hubs
{
    public class NotifyHub : Hub
    {
        private readonly IAccountService _accountService;

        private const string followed = "f";
        private const string following = "i";


        public NotifyHub(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public Task SendNotifications(string message)
        {
            return Clients.All.SendAsync("ReceiveMessage", message);
        }

        public Task SendnotificationsToOthers(string message)
        {
            return Clients.Others.SendAsync("ReceiveMessage", message);
        }
        public async Task SendnotificationsToOthersWhenOnline(string userId, string message)
        {
            var user = await _accountService.GetUserResourcesById(  userId);

            if(user != null)
            {                
                var followers = (user.Followers.Select(f => f.Key)).Select(f => f + following).ToList();
                await Clients.GroupExcept(userId+ following, followers).SendAsync("NotifyOnline", message);
            }
            Console.WriteLine($"User: {userId} đăng nhập");
            //await Clients.Others.SendAsync("NotifyOnline", message);
        }

        //public override Task OnConnectedAsync()
        //{
        //    return Clients.Caller.SendAsync("OnConnectedResult", Context.ConnectionId, $"Client: {Context.ConnectionId}, kết nối thành công tới notifyHub");
        //}

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return Clients.Caller.SendAsync("OnDisconnected", Context.ConnectionId, $"Client: {Context.ConnectionId}, hủy kết nối tới notifyHub");
        }

        public async Task JoinGroup(string userId)
        {
            await Groups.AddToGroupAsync(this.Context.ConnectionId, userId+followed); //Tự join group của mình
            //await Groups.AddToGroupAsync(this.Context.ConnectionId, userId+following); //Tự join group của mình

            var user = await _accountService.GetUserResourcesById(userId);
            if(user != null)
            {
                var followers = user.Followers.ToList();
                foreach(var f in followers)
                {
                    await Groups.AddToGroupAsync(this.Context.ConnectionId, f.Key + followed); // Join vào các group các user follow mình
                }
                var followings = await _accountService.GetFollowings(userId);
                foreach (var f in followings)
                {
                    await Groups.AddToGroupAsync(this.Context.ConnectionId, f + following); // Join vào các group các user mình follow
                }
            }
            await Clients.Caller.SendAsync("ReceiveMessage", "Join các group thành công");
        }

        //public async Task SendComment(string userId, string)
        //{
        //    Clients.Group("")
        //}
    }
}
