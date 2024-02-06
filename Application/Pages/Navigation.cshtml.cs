namespace Application.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class NavigationModel(ILogger logger) : PageModel
{
    private readonly ILogger _logger = logger;

    public void OnGet()
    {

    }
}