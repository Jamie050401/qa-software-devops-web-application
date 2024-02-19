namespace Application.Pages;

using AspNetCoreHero.ToastNotification.Abstractions;
using Common;
using Data;
using ILogger = Serilog.ILogger;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using System.Diagnostics;

public class RegisterModel(ILogger logger, INotyfService notyf) : PageModel
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
        var hasRememberMe = bool.TryParse(Request.Form["remember-me"].ToString(), out var isRemembered) && isRemembered;

        var isEmailValid = Validate.Email(notyf, email);
        if (!isEmailValid) return;

        var isPasswordValid = Validate.Password(notyf, password, confirmPassword);
        if (!isPasswordValid) return;

        var dbResponse = DatabaseManager.Database.Read(Role.GetProperty("Name"), "Default");
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
        {
            notyf.Error("Failed to register user.");
            logger.Information("Registration failure: unable to retrieve default role");
            return;
        }

        Debug.Assert(dbResponse.Value != null, "dbResponse.Value != null");
        var role = (Role)dbResponse.Value;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Password = Secret.Hash(password),
            FirstName = firstName,
            LastName = lastName,
            RoleId = role.Id
        };

        dbResponse = DatabaseManager.Database.Create(user);
        if (dbResponse.Status is ResponseStatus.Error)
        {
            notyf.Error("Failed to register user.");
            logger.Information("Registration failure: unable to add user to database");
            return;
        }

        Session.Login(HttpContext.Session, HttpContext.Connection, Request, Response, hasRememberMe, email, user);
    }

    public void OnPostSwitch()
    {
        Response.Redirect("/login");
    }
}
