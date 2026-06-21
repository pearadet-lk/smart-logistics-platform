using Microsoft.AspNetCore.SignalR;

namespace SmartLogistics.Notification.Api.Hubs;

public sealed class NotificationHub : Hub
{
    public Task SubscribeToTenant(string tenantId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"tenant-{tenantId}");
}

public sealed record RealtimeNotification(
    string Type,
    string Title,
    string Message,
    DateTime Timestamp,
    object? Data = null);
