using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SmartLogistics.Shared.Audit;

public static class AuditHttpExtensions
{
    public static (string UserId, string? IpAddress, string TraceId) GetAuditContext(this HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub")
            ?? context.User.FindFirstValue("preferred_username")
            ?? "system";

        return (userId, context.Connection.RemoteIpAddress?.ToString(), context.TraceIdentifier);
    }
}
