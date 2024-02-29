namespace Application.Pages;

using AspNetCoreHero.ToastNotification.Abstractions;
using Common;
using Data;
using ILogger = Serilog.ILogger;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using System.Diagnostics;

public class LoginModel(ILogger logger, INotyfService notyf) : PageModel
{
    public void OnGet()
    {
        if (Session.GetBoolean(HttpContext.Session, Session.Variables.IsLogout))
        {
            notyf.Success("Logged out successfully.");
            Session.SetBoolean(HttpContext.Session, Session.Variables.IsLogout, false);
        }

        if (Session.Redirect(HttpContext.Session, Request, Response)) return;
        Session.Login(logger, HttpContext.Session, Request, Response);
    }

    public void OnPost()
    {
        var email = Request.Form["email"].ToString();
        var password = Request.Form["password"].ToString();
        var hasRememberMe = bool.TryParse(Request.Form["remember-me"].ToString(), out var isRemembered) && isRemembered;

        var isEmailValid = Validate.Email(notyf, email);
        if (!isEmailValid) return;

        var dbResponse = DatabaseManager.Database.Read(Models.User.GetProperty("Email"), email);
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
        {
            notyf.Error("Email or password was incorrect, please try again.");
            logger.Information($"Login failure: {email} not found in database.");
            return;
        }

        Debug.Assert(dbResponse.Value != null, "dbResponse.Value != null");
        var userInDb = (User)dbResponse.Value;
        Debug.Assert(userInDb.Password != null, "userInDb.Password != null");
        if (email != userInDb.Email || !Secret.Verify(password, userInDb.Password))
        {
            notyf.Error("Email or password was incorrect, please try again.");
            logger.Information("Login failure: supplied password does not match stored password hash.");
            return;
        }

        Session.Login(HttpContext.Session, HttpContext.Connection, Request, Response, hasRememberMe, email, userInDb);
    }

    public void OnPostSwitch()
    {
        if (Session.GetBoolean(HttpContext.Session, Session.Variables.HasLoggedIn))
        {
            Session.SetBoolean(HttpContext.Session, Session.Variables.HasLoggedIn, false);
        }

        Response.Redirect("/register");
    }
}
