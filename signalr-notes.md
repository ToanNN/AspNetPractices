# Hub
## Hubs are transient
- Don't store state in a property of the hub class. Each hub method call is executed on a new hub instance.
- Don't instantiate a hub directly via dependency injection. Use IHubContext instead
- Use await when calling asynchronous methods that depend on the hub staying alive

# Users and Groups
- User identifier
The user identifier is case-sensitive. by default, SignalR uses the ClaimTypes.NameIdentifier from the ClaimsPrincipal associated with the connection as the user identifier
- A group is a collection of connections associated with a name.
- Groups are the recommended way to send to a connection or multiple connections because the groups are managed by the application. 
A connection can be a member of multiple groups. Groups are ideal for something like a chat application, where each room can be represented as a group
- Group membership isn't preserved when a connection reconnects. The connection needs to rejoin the group when it's re-established. 
Group names are case-sensitive.

# Design
Use objects as parameters

public class TotalLengthRequest
{
    public string Param1 { get; set; }
}

public async Task GetTotalLength(TotalLengthRequest req)
{
    return req.Param1.Length;
}
Now, the client uses an object to call the method:

connection.invoke("GetTotalLength", { param1: "value1" });

The same technique works for methods defined on the client

public async Task Broadcast(string message)
{
    await Clients.All.SendAsync("ReceiveMessage", new
    {
        Message = message
    });
}

# Host and Scaling

SignalR requires that all HTTP requests for a specific connection be handled by the same server process. => Enable sticky sessions and Session affinity.
Enabling the "ARR Affinity" setting in your Azure App Service will enable "sticky sessions". 

## TCP connection resources
Standard HTTP clients use ephemeral connections. These connections can be closed when the client goes idle and reopened later. On the other hand, a SignalR connection is persistent.

If a server runs out of connections, you'll see random socket errors and connection reset errors. For example, `An attempt was made to access a socket in a way forbidden by its access permissions...`
To keep SignalR resource usage from causing errors in other web apps, run SignalR on different servers than your other web apps.
To keep SignalR resource usage from causing errors in a SignalR app, scale out to limit the number of connections a server has to handle.

## Scale out

When SignalR on one of the servers wants to send a message to all clients, the message only goes to the clients connected to that server.

## Redis backplane
. When a client makes a connection, the connection information is passed to the backplane. When a server wants to send a message to all clients, it sends to the backplane. The backplane knows all connected clients and which servers they're on. It sends the message to all clients via their respective servers

### IIS limitations on Windows client OS
Windows 10 and Windows 8.x are client operating systems. IIS on client operating systems has a limit of 10 concurrent connections. SignalR's connections are:

- Transient and frequently re-established.
- Not disposed immediately when no longer used.
When a client OS is used for development, we recommend:

Avoid IIS.
Use Kestrel or IIS Express as deployment targets.