using Microsoft.AspNetCore.SignalR;

namespace SignalrWithTypescript.Hubs;


// Strongly typed clients to avoid strings as client method names
public class ChatHub : Hub<IChatClient>
{
    public async Task NewMessage(string userName, string message) =>
        await Clients.All.ReceiveMessage(userName, message);

    public async Task SendMessageToCaller(string user, string message)
    {
        await Clients.Caller.ReceiveMessage(user, message);
    }
}

public interface IChatClient
{
    Task ReceiveMessage(string user, string message);
}