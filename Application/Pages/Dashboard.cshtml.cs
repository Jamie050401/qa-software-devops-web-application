namespace Application.Pages;

using AspNetCoreHero.ToastNotification.Abstractions;
using Common;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DashboardModel(INotyfService notyf) : PageModel
{
    public void OnGet()
    {
        if (!Session.Authenticate(HttpContext.Session, Request, Response)) return;

        // ReSharper disable once InvertIf
        if (Session.GetBoolean(HttpContext.Session, Session.Variables.IsLogin))
        {
            notyf.Success("Logged in successfully.");
            Session.SetBoolean(HttpContext.Session, Session.Variables.IsLogin, false);
        }
    }

    public void OnPostLogout()
    {
        Session.Logout(HttpContext.Session, Response);
    }
}
