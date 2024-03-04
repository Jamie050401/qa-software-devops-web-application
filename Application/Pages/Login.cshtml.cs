namespace Application.Pages;

using AspNetCoreHero.ToastNotification.Abstractions;
using Common;
using Data;
using ILogger = Serilog.ILogger;
using Models;
using PageModel = Shared.PageModel;
using System.Diagnostics;

public class LoginModel(ILogger logger, INotyfService notyf) : PageModel
{
    public void OnGet()
    {
        if (Session.GetBoolean(HttpContext.Session, Session.Variables.IsLogout))
        {
            notyf.Success("Logged out successfully.");
            Session.DeleteObject(HttpContext.Session, Session.Variables.IsLogout);
        }

        if (Session.Redirect(HttpContext.Session, Request, Response)) return;
        if (Session.TryCookieLogin(logger, HttpContext.Session, Request, Response)) return;

        Form = this.GetForm();
    }

    public void OnPost()
    {
        Form = this.GetForm();

        Form.Email = Request.Form["Email"].ToString();
        Form.Password = Request.Form["Password"].ToString();
        Form.RememberMe = bool.TryParse(Request.Form["RememberMe"].ToString(), out var hasRememberMe) && hasRememberMe;

        var isEmailValid = Validate.Email(notyf, Form.Email); // TODO - Validate maximum length of email
        if (!isEmailValid) return;

        // TODO - Validate maximum length of password

        var dbResponse = DatabaseManager.Database.Read(Models.User.GetProperty("Email"), Form.Email);
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
        {
            notyf.Error("Email or password was incorrect, please try again.");
            logger.Information($"Login failure: {Form.Email} not found in database.");
            return;
        }

        Debug.Assert(dbResponse.Value != null, "dbResponse.Value != null");
        var userInDb = (User)dbResponse.Value;
        Debug.Assert(userInDb.Password != null, "userInDb.Password != null");
        if (Form.Email != userInDb.Email || !Secret.Verify(Form.Password, userInDb.Password))
        {
            notyf.Error("Email or password was incorrect, please try again.");
            logger.Information("Login failure: supplied password does not match stored password hash.");
            return;
        }

        Session.DeleteObject(HttpContext.Session, Session.Variables.LoginFormData);
        Session.Login(HttpContext.Session, HttpContext.Connection, Request, Response, Form.RememberMe, Form.Email, userInDb);
    }

    public void OnPostSwitch()
    {
        Session.SetBoolean(HttpContext.Session, Session.Variables.RegistrationSwitch, true);
        Session.DeleteObject(HttpContext.Session, Session.Variables.LoginFormData);

        Response.Redirect("/register");
    }

    private FormData GetForm()
    {
        return Session.GetObject<FormData>(HttpContext.Session, Session.Variables.LoginFormData)
               ?? FormData.Default();
    }

    public class FormData
    {
        public static FormData Default()
        {
            return new FormData
            {
                Email = string.Empty,
                Password = string.Empty,
                RememberMe = false
            };
        }

        public required string Email { get; set; }
        public required string Password { get; set; }
        public required bool RememberMe { get; set; }
    }

    public FormData Form { get; private set; } = FormData.Default();
}
