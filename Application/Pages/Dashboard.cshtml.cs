namespace Application.Pages;

using AspNetCoreHero.ToastNotification.Abstractions;
using Common;
using ILogger = Serilog.ILogger;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DashboardModel(ILogger logger, INotyfService notyf) : PageModel
{
    public void OnGet()
    {
        Session.Authenticate(HttpContext.Session, Request, Response);

        // ReSharper disable once InvertIf
        if (Session.GetBoolean(HttpContext.Session, "IsFirstDashboardVisit"))
        {
            notyf.Success("Logged in successfully.");
            Session.SetBoolean(HttpContext.Session, "IsFirstDashboardVisit", false);
        }
    }

    public void OnPostLogout()
    {
        Session.SetBoolean(HttpContext.Session, "IsLogout", true);
        Session.Logout(HttpContext.Session, Request, Response);
    }
}
