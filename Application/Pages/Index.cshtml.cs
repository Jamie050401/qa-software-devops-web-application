namespace Application.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared;

public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    public NavigationModel NavigationModel { get; } = new(logger);
    public FooterModel FooterModel { get; } = new(logger);
    private readonly ILogger<IndexModel> _logger = logger;

    public void OnGet()
    {

    }
}