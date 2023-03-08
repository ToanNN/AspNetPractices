using Microsoft.Extensions.Logging;

public class Runner
{
    private ILogger<Runner> _logger;

    public Runner(ILogger<Runner> logger){
        _logger = logger;
    }

    public void DoAction(string name){
        _logger.LogDebug(14, "Doing {Action}", name);
    }
}