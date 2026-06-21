using Microsoft.AspNetCore.SignalR;
using SmartLogistics.Notification.Api.Hubs;

namespace SmartLogistics.Notification.Api.Services;

public sealed class NotificationDispatcher(IHubContext<NotificationHub> hub)
{
    public Task SendToAllAsync(RealtimeNotification notification) =>
        hub.Clients.All.SendAsync("ReceiveNotification", notification);

    public Task SendToTenantAsync(string tenantId, RealtimeNotification notification) =>
        hub.Clients.Group($"tenant-{tenantId}").SendAsync("ReceiveNotification", notification);
}
