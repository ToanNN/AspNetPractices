namespace UsingOutputCaching.Middleware.ScopeServices;

public interface IMessageWriter
{
    void Write(string message);
}

public class ConsoleWriter : IMessageWriter
{
    public void Write(string message)
    {
        Console.WriteLine(message);
    }
}