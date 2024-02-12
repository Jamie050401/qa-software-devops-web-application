namespace Application.Pages;

using Common;
using ILogger = Serilog.ILogger;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class RegisterModel(ILogger logger) : PageModel
{
    public void OnGet()
    {
        if (Session.Redirect(HttpContext.Session, Request, Response)) return;
        Session.Login(logger, HttpContext.Session, Request, Response);
    }

    public void OnPost()
    {
        var email = Request.Form["email"].ToString();
        var firstName = Request.Form["first_name"].ToString();
        var lastName = Request.Form["last_name"].ToString();
        var password = Request.Form["password_first"].ToString();
        var confirmPassword = Request.Form["password_second"].ToString();

        // TODO - Finish implementing registration functionality (need to amend database to autoincrement primary key IDs)
    }
}
