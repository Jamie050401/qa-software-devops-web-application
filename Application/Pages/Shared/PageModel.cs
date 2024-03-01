namespace Application.Pages.Shared;

public class PageModel : Microsoft.AspNetCore.Mvc.RazorPages.PageModel
{
    public void OnPostLogout()
    {
        Response.Redirect("/logout");
    }
}