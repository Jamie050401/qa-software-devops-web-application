namespace Application.Pages;

using Common;
using Models;
using PageModel = Shared.PageModel;

public class Results : PageModel
{
    public void OnGet()
    {
        if (!Session.Authenticate(HttpContext.Session, Request, Response)) return;

        CurrentUser = Session.GetCurrentUser(HttpContext.Session);
    }

    public User CurrentUser { get; private set; } = Models.User.Default();
}