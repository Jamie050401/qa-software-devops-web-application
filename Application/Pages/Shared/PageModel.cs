namespace Application.Pages.Shared;

using Microsoft.AspNetCore.Mvc;

public class PageModel : Microsoft.AspNetCore.Mvc.RazorPages.PageModel
{
    public IActionResult OnPostDashboard()
    {
        return this.RedirectToPage("/dashboard");
    }

    public IActionResult OnPostLogout()
    {
        return this.RedirectToPage("/logout");
    }
}