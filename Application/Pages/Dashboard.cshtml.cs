namespace Application.Pages;

using AspNetCoreHero.ToastNotification.Abstractions;
using Common;
using Models;
using PageModel = Shared.PageModel;

public class DashboardModel(INotyfService notyf) : PageModel
{
    public void OnGet()
    {
        if (!Session.Authenticate(HttpContext.Session, Request, Response)) return;

        // ReSharper disable once InvertIf
        if (Session.GetBoolean(HttpContext.Session, SessionVariables.IsLogin))
        {
            notyf.Success("Logged in successfully.");
            Session.DeleteObject(HttpContext.Session, SessionVariables.IsLogin);
        }

        CurrentUser =
            Session.GetObject<User>(HttpContext.Session, SessionVariables.CurrentUser)
            ?? Models.User.Default();
    }

    public void OnPostProjection()
    {
        Response.Redirect("/projection");
    }

    public void OnPostResults()
    {
        // ...
    }

    public User CurrentUser { get; private set; } = Models.User.Default();
}
