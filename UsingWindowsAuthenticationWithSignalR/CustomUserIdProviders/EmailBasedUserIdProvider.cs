using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace UsingWindowsAuthenticationWithSignalR.CustomProviders;

public class EmailBasedUserIdProvider :IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst(ClaimTypes.Email)?.Value;
    }
}