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
        if (Session.GetBoolean(HttpContext.Session, "IsLogout"))
        {
            notyf.Success("Logged out successfully.");
            Session.SetBoolean(HttpContext.Session, "IsLogout", false);
        }

        if (Session.Redirect(HttpContext.Session, Request, Response)) return;
        Session.Login(logger, HttpContext.Session, Request, Response);
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
        var userInDb = dbResponse.Value;
        if (email != userInDb.Email || !SecretHasher.Verify(password, userInDb.Password))
        {
            notyf.Error("Email or password was incorrect, please try again.");
            logger.Information($"Login failure: supplied password does not match stored password hash.");
            return;
        }

        if (hasRememberMe)
        {
            var cookieResponse = Cookie.Retrieve<AuthenticationData>(Request, "QAWA-AuthenticationData");
            if (cookieResponse.Status is ResponseStatus.Error)
            {
                if (!cookieResponse.HasValue)
                {
                    this.UpdateAuthenticationDataCookie(email, userInDb);
                }
            }
        }

        Session.Login(HttpContext.Session, Response);
    }

    // TODO - Set 'Expires' via parameter such that the user can decide how long to be remembered for i.e. from 1 day up to 90 days
    private void UpdateAuthenticationDataCookie(string email, User userInDb)
    {
        var authenticationData = new AuthenticationData
        {
            Email = email,
            Token = Password.Generate(),
            Source = HttpContext.Connection.RemoteIpAddress is null
                ? ""
                : HttpContext.Connection.RemoteIpAddress.ToString(),
            Timestamp = DateTime.UtcNow,
            Expires = DateTimeOffset.UtcNow.AddDays(3)
        };
        Cookie.Store(Response, "QAWA-AuthenticationData", authenticationData, authenticationData.Expires, true);

        authenticationData.Token = SecretHasher.Hash(authenticationData.Token);
        userInDb.AuthenticationData = authenticationData;
        DatabaseManager.Database.AddUserToDatabase(userInDb);
    }
}
