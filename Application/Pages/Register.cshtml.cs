namespace Application.Pages;

using Common;
using ILogger = Serilog.ILogger;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class RegisterModel(ILogger logger) : PageModel
{
    public void OnGet()
    {
        Session.Redirect(HttpContext.Session, Response);
        Session.Login(logger, HttpContext.Session, Request, Response);
    }
}
