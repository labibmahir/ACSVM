using Domain.Dto.HIKVision;
using Microsoft.AspNetCore.SignalR;

namespace Api.NotificationHub
{
    public class NotificationsHub : Hub
    {
        public async Task SendEvent(AlarmInfo alarmInfo)
        {
            await Clients.All.SendAsync("ReceiveEvent", alarmInfo);
        }

        public override async Task OnConnectedAsync()
        {
            var userAccountId = Context.GetHttpContext()?.Request.Query["userAccountId"];

            if (!string.IsNullOrEmpty(userAccountId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userAccountId);
                Console.WriteLine($"User Connected: {userAccountId}");
                if (!string.IsNullOrEmpty(userAccountId))
                {

                }

            }

            await base.OnConnectedAsync();
        }

        // On Disconnected: Remove user from group (optional)
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userAccountId = Context.GetHttpContext()?.Request.Query["userAccountId"];

            if (!string.IsNullOrEmpty(userAccountId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userAccountId);
                Console.WriteLine($"User Disconnected: {userAccountId}");


            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
