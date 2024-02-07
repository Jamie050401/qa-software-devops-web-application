namespace Application.Pages;

using ILogger = Serilog.ILogger;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class RegisterModel(ILogger logger) : PageModel
{
    public void OnGet()
    {
        Session.Redirect(HttpContext.Session, Response);
    }
}
