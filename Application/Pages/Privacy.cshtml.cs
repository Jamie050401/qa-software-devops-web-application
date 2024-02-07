namespace Application.Pages;

using ILogger = Serilog.ILogger;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class PrivacyModel(ILogger logger) : PageModel
{
    public void OnGet()
    {
        Session.Authenticate(HttpContext.Session, Response);
    }
}
