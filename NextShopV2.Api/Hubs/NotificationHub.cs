using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NextShopV2.Api.Models;

namespace NextShopV2.Api.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        // No server methods required for now. Clients connect and receive events from server via hub context.
        public override Task OnConnectedAsync()
        {
            // you can log connection or use Context.UserIdentifier
            return base.OnConnectedAsync();
        }
    }
}
