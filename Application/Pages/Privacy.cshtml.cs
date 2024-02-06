namespace Application.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class PrivacyModel(ILogger<PrivacyModel> logger) : PageModel
{
    public NavigationModel NavigationModel { get; } = new NavigationModel(logger);
    private readonly ILogger<PrivacyModel> _logger = logger;

    public void OnGet()
    {
    }
}