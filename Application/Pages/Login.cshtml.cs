namespace Application.Pages;

using Common;
using Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

public class LoginModel(ILogger<LoginModel> logger) : PageModel
{
    private readonly ILogger<LoginModel> _logger = logger;

    public void OnGet()
    {

    }

    public void OnPost()
    {
        var email = Request.Form["email"].ToString();
        var password = Request.Form["password"].ToString();
        var hasRememberMe = bool.TryParse(Request.Form["remember-me"].ToString(), out var isRemembered) &&
                            isRemembered; // TODO - Implement remember me functionality

        // TODO - Validate the above ...

        var dbResponse = DatabaseManager.Database.GetUserFromDatabase(userEmail: email);
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
        {
            // TODO - Display some kind of error for the user (and log)
            return;
        }

        Debug.Assert(dbResponse.Value != null, "dbResponse.Value != null");
        if (email != dbResponse.Value.Email || password != dbResponse.Value.Password)
        {
            // TODO - Display some kind of error for the user (and log)
            return;
        }

        Session.Login(HttpContext.Session, Response);
    }
}
