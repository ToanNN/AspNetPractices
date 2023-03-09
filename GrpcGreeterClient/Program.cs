using Grpc.Net.Client;
using GrpcGreaterClient;

//create a channel pointing to the server
using var channel = GrpcChannel.ForAddress("https://localhost:7067");
var client = new Greeter.GreeterClient(channel);
var reply = await client.SayHelloAsync(new HelloRequest()
{
    Name = "Vu Thi Hong Van"
});

Console.WriteLine("Greeting: " + reply.Message);
Console.WriteLine("Press any key to exit");
Console.ReadLine();