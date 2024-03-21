namespace Application.Pages;

using AspNetCoreHero.ToastNotification.Abstractions;
using Common;
using Data;
using ILogger = Serilog.ILogger;
using Models;
using PageModel = Shared.PageModel;
using System;
using System.Diagnostics;

public class RegisterModel(ILogger logger, INotyfService notyf) : PageModel
{
    public void OnGet()
    {
        if (Session.Redirect(HttpContext.Session, Request, Response)) return;
        Session.TryCookieLogin(logger, HttpContext.Session, HttpContext.Connection, Request, Response);

        Form = this.GetForm();
    }

    public void OnPost()
    {
        Form = this.GetForm();
        this.GetFormData();

        var isEmailValid = Validate.Email(notyf, Form.Email);
        if (!isEmailValid)
        {
            Session.SetObject(HttpContext.Session, SessionVariables.RegistrationFormData, Form);
            return;
        }

        var isPasswordValid = Validate.Password(notyf, Form.PasswordFirst, Form.PasswordSecond);
        if (!isPasswordValid)
        {
            Session.SetObject(HttpContext.Session, SessionVariables.RegistrationFormData, Form);
            return;
        }

        var dbResponse = DatabaseManager.Database.Read(Role.GetProperty("Name"), "Default");
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
        {
            Session.SetObject(HttpContext.Session, SessionVariables.RegistrationFormData, Form);
            notyf.Error("Failed to register user.");
            logger.Information("Registration failure: unable to retrieve default role");
            return;
        }

        Debug.Assert(dbResponse.Value != null, "dbResponse.Value != null");
        var role = (Role)dbResponse.Value;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = Form.Email,
            Password = Secret.Hash(Form.PasswordFirst),
            FirstName = Form.FirstName,
            LastName = Form.LastName,
            RoleId = role.Id
        };

        dbResponse = DatabaseManager.Database.Create(user);
        if (dbResponse.Status is ResponseStatus.Error)
        {
            Session.SetObject(HttpContext.Session, SessionVariables.RegistrationFormData, Form);
            notyf.Error("Failed to register user.");
            logger.Information("Registration failure: unable to add user to database");
            return;
        }

        Session.DeleteObject(HttpContext.Session, SessionVariables.RegistrationFormData);
        Session.Login(HttpContext.Session, HttpContext.Connection, Request, Response, Form.RememberMe, Form.Email, user);
    }

    public void OnPostSwitch()
    {
        Session.DeleteObject(HttpContext.Session, SessionVariables.RegistrationSwitch);
        Session.DeleteObject(HttpContext.Session, SessionVariables.RegistrationFormData);

        Response.Redirect("/login");
    }

    private FormData GetForm()
    {
        return Session.GetObject<FormData>(HttpContext.Session, SessionVariables.RegistrationFormData)
               ?? FormData.Default();
    }

    private void GetFormData()
    {
        Form.Email = Request.Form["Email"].ToString();
        Form.FirstName = Request.Form["FirstName"].ToString();
        Form.LastName = Request.Form["LastName"].ToString();
        Form.PasswordFirst = Request.Form["PasswordFirst"].ToString();
        Form.PasswordSecond = Request.Form["PasswordSecond"].ToString();
        Form.RememberMe = bool.TryParse(Request.Form["RememberMe"].ToString(), out var isRemembered) && isRemembered;
    }

    public class FormData
    {

        public static FormData Default()
        {
            return new FormData
            {
                Email = string.Empty,
                FirstName = null,
                LastName = null,
                PasswordFirst = string.Empty,
                PasswordSecond = string.Empty,
                RememberMe = false
            };
        }

        public required string Email { get; set; }
        public required string? FirstName { get; set; }
        public required string? LastName { get; set; }
        public required string PasswordFirst { get; set; }
        public required string PasswordSecond { get; set; }
        public required bool RememberMe { get; set; }
    }

    public FormData Form { get; private set; } = FormData.Default();
}
