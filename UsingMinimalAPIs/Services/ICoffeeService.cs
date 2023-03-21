namespace UsingMinimalAPIs.Services;

public interface ICoffeeService
{
    public decimal GiveMeACoffee();
}

public class CoffeeService : ICoffeeService
{
    public decimal GiveMeACoffee()
    {
        return 1;
    }
}