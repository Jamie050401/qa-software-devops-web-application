namespace Application.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class RegisterModel(ILogger<IndexModel> logger) : PageModel
{
    private readonly ILogger<IndexModel> _logger = logger;

    public void OnGet()
    {

    }
}