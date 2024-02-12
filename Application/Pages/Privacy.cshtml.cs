namespace Application.Pages;

using Common;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class PrivacyModel : PageModel
{
    public void OnGet()
    {
        // ReSharper disable once RedundantJumpStatement
        if (!Session.Authenticate(HttpContext.Session, Request, Response)) return;
    }
}
