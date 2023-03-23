using Microsoft.AspNetCore.SignalR;

namespace UsingWindowsAuthenticationWithSignalR.CustomProviders;

public class NameUserIdProvider:IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        // this will use Name claim instead of "Name Identifier" claim
        //to use the "Name" claim (which is the Windows username in the form [Domain]/[Username]),
        return connection.User?.Identity?.Name;
    }
}