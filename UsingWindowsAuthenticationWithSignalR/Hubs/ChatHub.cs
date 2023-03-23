using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace UsingWindowsAuthenticationWithSignalR.Hubs;

[Authorize("registered-user-policy")]
public class ChatHub: Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("ReceiveSystemMessage",
            $"{Context.UserIdentifier} joined.");
        await base.OnConnectedAsync();
    }

    [Authorize("admin-policy")]
    public void BanUser(string userName)
    {
        // ... ban a user from the chat room (something only Administrators can do) ...
    }


    [Authorize("DomainRestrictedPolicy")]
    public void ViewUserHistory(string username)
    {
    }
}