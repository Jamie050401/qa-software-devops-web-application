namespace Application.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class PrivacyModel(ILogger<PrivacyModel> logger) : PageModel
{
    private readonly ILogger<PrivacyModel> _logger = logger;

    public void OnGet()
    {
        Session.Authenticate(HttpContext.Session, Response);
    }
}
