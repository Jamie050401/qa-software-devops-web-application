namespace Application.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class RegisterModel(ILogger<RegisterModel> logger) : PageModel
{
    private readonly ILogger<RegisterModel> _logger = logger;

    public void OnGet()
    {
        Session.Redirect(HttpContext.Session, Response);
    }
}
