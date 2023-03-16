using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakingHttpRequests.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }

    public async Task OnGet()
    {
        //Get a named client
        var client = _clientFactory.CreateClient("GitHub");
        var httpResponseMessage = await client.GetAsync("repos/dotnet/AspNetCore.Docs/branches");

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
        }
    }
}