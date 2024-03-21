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
        if (Session.GetBoolean(HttpContext.Session, SessionVariables.IsLogout))
        {
            notyf.Success("Logged out successfully.");
            Session.DeleteObject(HttpContext.Session, SessionVariables.IsLogout);
        }

        if (Session.Redirect(HttpContext.Session, Request, Response)) return;
        if (Session.TryCookieLogin(logger, HttpContext.Session, HttpContext.Connection, Request, Response)) return;

        Form = this.GetForm();
    }

    public void OnPost()
    {
        Form = this.GetForm();
        this.GetFormData();

        var isEmailValid = Validate.Email(notyf, Form.Email);
        if (!isEmailValid)
        {
            Session.SetObject(HttpContext.Session, SessionVariables.LoginFormData, Form);
            return;
        }

        var dbResponse = DatabaseManager.Database.Read(Models.User.GetProperty("Email"), Form.Email);
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
        {
            Session.SetObject(HttpContext.Session, SessionVariables.LoginFormData, Form);
            notyf.Error("Email or password was incorrect, please try again.");
            logger.Information($"Login failure: {Form.Email} not found in database.");
            return;
        }

        Debug.Assert(dbResponse.Value != null, "dbResponse.Value != null");
        var userInDb = (User)dbResponse.Value;
        Debug.Assert(userInDb.Password != null, "userInDb.Password != null");
        if (Form.Email != userInDb.Email || !Secret.Verify(Form.Password, userInDb.Password))
        {
            Session.SetObject(HttpContext.Session, SessionVariables.LoginFormData, Form);
            notyf.Error("Email or password was incorrect, please try again.");
            logger.Information("Login failure: supplied password does not match stored password hash.");
            return;
        }

        Session.DeleteObject(HttpContext.Session, SessionVariables.LoginFormData);
        Session.Login(HttpContext.Session, HttpContext.Connection, Request, Response, Form.RememberMe, Form.Email, userInDb);
    }

    public void OnPostSwitch()
    {
        Session.SetBoolean(HttpContext.Session, SessionVariables.RegistrationSwitch, true);
        Session.DeleteObject(HttpContext.Session, SessionVariables.LoginFormData);

        Response.Redirect("/register");
    }

    private FormData GetForm()
    {
        return Session.GetObject<FormData>(HttpContext.Session, SessionVariables.LoginFormData)
               ?? FormData.Default();
    }

    private void GetFormData()
    {
        Form.Email = Request.Form["Email"].ToString();
        Form.Password = Request.Form["Password"].ToString();
        Form.RememberMe = Request.Form["RememberMe"].ToString() is "on";
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
