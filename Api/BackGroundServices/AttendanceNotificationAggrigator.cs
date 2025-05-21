using Api.NotificationHub;
using Domain.Dto.HIKVision;
using Microsoft.AspNetCore.SignalR;

namespace Api.BackGroundServices
{
    public class AttendanceNotificationAggrigator
    {
        private readonly IHubContext<NotificationsHub> _hubContext;

        public AttendanceNotificationAggrigator(IHubContext<NotificationsHub> hubContext)
        {

            _hubContext = hubContext;
        }
        public async Task SendAccessControllEventNotification(AlarmInfo info)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveEvent", info);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
