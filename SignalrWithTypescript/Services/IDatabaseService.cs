namespace SignalrWithTypescript.Services;

public interface IDatabaseService
{
    string GetUserName(string user);
}

class DatabaseService : IDatabaseService
{
    public string GetUserName(string user)
    {
        return $"Name {user}";
    }
}