namespace Application.Pages;

using Common;
using Models;
using PageModel = Shared.PageModel;
using System;

public class Results : PageModel
{
    public void OnGet()
    {
        if (!Session.Authenticate(HttpContext.Session, Request, Response)) return;

        CurrentUser = Session.GetCurrentUser(HttpContext.Session);
    }

    public void OnGetResult(Guid id)
    {
        if (!Session.Authenticate(HttpContext.Session, Request, Response)) return;

        CurrentUser = Session.GetCurrentUser(HttpContext.Session);
    }

    public User CurrentUser { get; private set; } = Models.User.Default();
}