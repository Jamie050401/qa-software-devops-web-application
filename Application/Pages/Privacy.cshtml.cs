namespace Application.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared;

public class PrivacyModel(ILogger<PrivacyModel> logger) : PageModel
{
    public NavigationModel NavigationModel { get; } = new(logger);
    public FooterModel FooterModel { get; } = new(logger);
    private readonly ILogger<PrivacyModel> _logger = logger;

    public void OnGet()
    {
    }
}