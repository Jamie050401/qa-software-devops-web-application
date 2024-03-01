namespace Application.Pages;

using Common;
using Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Newtonsoft.Json;
using PageModel = Shared.PageModel;

public class Projection : PageModel
{
    public void OnGet()
    {
        if (!Session.Authenticate(HttpContext.Session, Request, Response)) return;

        CurrentUser = Session.GetObject<User>(HttpContext.Session, Session.Variables.CurrentUser);
        Funds = DatabaseManager.Database.ReadAll<Fund>().Value?.Select(model =>
        {
            var fund = (Fund)model;
            return new SelectListItem
            {
                Value = JsonConvert.SerializeObject(fund),
                Text = fund.Name
            };
        }).ToList() ?? [];
    }

    public void OnPost()
    {
        // ...
    }

    public void OnPostFunds()
    {
        // ...
    }

    public User? CurrentUser { get; private set; }
    public List<SelectListItem>? Funds { get; private set; }
}