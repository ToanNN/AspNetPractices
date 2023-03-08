using AspnetCoreFeatures.OptionPattern;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AspnetCoreFeatures.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger _logger;
        private readonly IOptions<Position> _position;

        public WeatherForecastController(ILoggerFactory loggerFactory, IOptions<Position> position)
        {
            //Explicitly specify the logging category
            _logger = loggerFactory.CreateLogger("Controllers");
            _position = position;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogInformation(10, "Weather controller is called");
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)],
                Position = $"{_position.Value.Name} {_position.Value.Title}"
            })
            .ToArray();
        }
    }
}