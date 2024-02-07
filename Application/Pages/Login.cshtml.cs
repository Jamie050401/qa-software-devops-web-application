namespace Application.Pages;

using AspNetCoreHero.ToastNotification.Abstractions;
using Common;
using Data;
using ILogger = Serilog.ILogger;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

public class LoginModel(ILogger logger, INotyfService notyf) : PageModel
{
    public void OnGet()
    {
        Session.Redirect(HttpContext.Session, Response);
    }

    public void OnPost()
    {
        var email = Request.Form["email"].ToString();
        var password = Request.Form["password"].ToString();
        var hasRememberMe = bool.TryParse(Request.Form["remember-me"].ToString(), out var isRemembered) && isRemembered;

        var isEmailValid = Validate.Email(email);
        if (!isEmailValid)
        {
            notyf.Error("Please make sure to enter a valid email address.");
            return;
        }

        var dbResponse = DatabaseManager.Database.GetUserFromDatabase(userEmail: email);
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
        {
            notyf.Error("Email or password was incorrect, please try again.");
            logger.Information($"Login failure: {email} not found in database.");
            return;
        }

        Debug.Assert(dbResponse.Value != null, "dbResponse.Value != null");
        if (email != dbResponse.Value.Email || !SecretHasher.Verify(password, dbResponse.Value.Password))
        {
            notyf.Error("Email or password was incorrect, please try again.");
            logger.Information($"Login failure: supplied password does not match stored password hash.");
            return;
        }

        // TODO - Implement remember me functionality

        notyf.Success("Logged in successfully.");
        Session.Login(HttpContext.Session, Response);
    }
}
