namespace Application.Pages;

using AspNetCoreHero.ToastNotification.Abstractions;
using Cookie = Common.Cookie;
using Common;
using Data;
using ILogger = Serilog.ILogger;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Net;

public class LoginModel(ILogger logger, INotyfService notyf) : PageModel
{
    public void OnGet()
    {
        Session.Redirect(HttpContext.Session, Response);

        // TODO - Implement logic to log in automatically if relevant cookie has valid data
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
            var authenticationData = Cookie.Retrieve(Request, "QAWA-AuthenticationData");
            if (!authenticationData.ContainsKey("Token"))
            {
                var token = SecretHasher.Hash(Password.Generate());
                authenticationData.Add("Token", token);
                authenticationData.Add("Source", (HttpContext.Connection.RemoteIpAddress ?? IPAddress.None).ToString());
                Cookie.Store(Response, "QAWA-AuthenticationData", authenticationData, true);

                userInDb.Token = token;
                DatabaseManager.Database.AddUserToDatabase(userInDb);
            }
        }

        notyf.Success("Logged in successfully.");
        Session.Login(HttpContext.Session, Response);
    }
}
