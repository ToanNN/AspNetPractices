using Microsoft.AspNetCore.SignalR;

namespace SignalrWithTypescript.Hubs;

public class ChatHub : Hub
{
    public async Task NewMessage(long userName, string message) =>
        await Clients.All.SendAsync("messageReceived", userName, message);
}