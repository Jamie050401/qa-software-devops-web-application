namespace Application.Pages;

using Common;
using PageModel = Shared.PageModel;

public class LogoutModel : PageModel
{
    public void OnGet()
    {
        Session.Logout(HttpContext.Session, Response);
    }
}