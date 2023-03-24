using Microsoft.AspNetCore.SignalR;
using SignalrWithTypescript.Services;

namespace SignalrWithTypescript.Hubs;

// Strongly typed clients to avoid strings as client method names
public class ChatHub : Hub<IChatClient>
{
    // Change the name to a new name
    [HubMethodName("AcceptMessages")]
    public async Task NewMessage(string userName, string message)
    {
        await Clients.All.ReceiveMessage(userName, message);
    }

    public async Task SendMessageToCaller(string user, string message, IDatabaseService databaseService)
    {
        var userName = databaseService.GetUserName(user);
        await Clients.Caller.ReceiveMessage(userName, message);
    }

    public async Task<string> WaitForMessage(string connectionId)
    {
        // Call the client with the connection id to get the client's message
        string message = await Clients.Client(connectionId).GetMessageFromClient();
        return message;
    }

    //public override async Task OnConnectedAsync()
    //{
    //    await Groups.AddToGroupAsync(Context.ConnectionId, "SuperUserGroup");
    //}

    //// Exception is not null when connection is interrupted
    //// null when Graceful disconnection
    //public override async Task OnDisconnectedAsync(Exception? exception)
    //{
    //    //RemoveFromGroupAsync does not need to be called in OnDisconnectedAsync, it's automatically handled for you.
    //    //await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SuperUserGroup");
    //    await base.OnDisconnectedAsync(exception);
    //}
}

public interface IChatClient
{
    Task ReceiveMessage(string user, string message);
    Task<string> GetMessageFromClient();
}