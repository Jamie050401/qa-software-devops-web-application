namespace Application.Pages.Shared;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class FooterModel(ILogger logger) : PageModel
{
    private readonly ILogger _logger = logger;

    public void OnGet()
    {

    }
}