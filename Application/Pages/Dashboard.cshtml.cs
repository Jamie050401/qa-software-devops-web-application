namespace Application.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class DashboardModel(ILogger<DashboardModel> logger) : PageModel
{
    private readonly ILogger<DashboardModel> _logger = logger;

    public void OnGet()
    {
        Session.Authenticate(HttpContext.Session, Response);
    }
}
