namespace Application.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    private readonly ILogger<IndexModel> _logger = logger;

    public void OnGet()
    {
        Session.Authenticate(HttpContext.Session, Response);
    }
}
