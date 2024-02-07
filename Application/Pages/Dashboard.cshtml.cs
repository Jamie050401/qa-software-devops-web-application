namespace Application.Pages;

using Common;
using ILogger = Serilog.ILogger;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DashboardModel(ILogger logger) : PageModel
{
    public void OnGet()
    {
        Session.Authenticate(HttpContext.Session, Response);
    }
}
