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

        var formData = Session.GetObject<FormData>(HttpContext.Session, Session.Variables.ProjectionFormData);
        if (formData is not null)
        {
            // TODO - Call function to to populate form data using formData
        }

        CurrentUser =
            Session.GetObject<User>(HttpContext.Session, Session.Variables.CurrentUser)
            ?? Models.User.Default();
        Funds = formData is null
            ? DatabaseManager.Database.ReadAll<Fund>().Value?.Select(ConvertFundToSelectListItem).ToList() ?? []
            : formData.Funds;
        SelectedFunds = formData is null
            ? []
            : formData.SelectedFunds;
    }

    public void OnPost()
    {
        // This should be called to submit the projection form
        // ...

        Session.DeleteObject(HttpContext.Session, Session.Variables.ProjectionFormData);

        // TODO - Redirect to results page, with the result for the current projection automatically opened
    }

    public void OnPostAddFund()
    {
        var formData = new FormData
        {
            Title = Request.Form["title"].ToString(),
            FirstName = Request.Form["first_name"].ToString(),
            LastName = Request.Form["last_name"].ToString(),
            DateOfBirth = DateTime.Parse(Request.Form["date_of_birth"].ToString()),
            Investment = decimal.Parse(Request.Form["total_investment"].ToString()),
            Funds = Funds,
            SelectedFunds = SelectedFunds
        };

        var fund = JsonConvert.DeserializeObject<Fund>(Request.Form["selected_fund"].ToString());
        if (fund is null)
        {
            Response.Redirect("/projection");
            return;
        }

        SelectedFunds.Add(fund);
        Funds.Remove(new SelectListItem
        {
            Value = JsonConvert.SerializeObject(fund),
            Text = fund.Name
        });

        formData.Funds = Funds;
        formData.SelectedFunds = SelectedFunds;
        Session.SetObject(HttpContext.Session, Session.Variables.ProjectionFormData, formData);

        Response.Redirect("/projection");
    }

    public void OnPostDeleteFund()
    {
        // ...

        Response.Redirect("/projection");
    }

    private static SelectListItem ConvertFundToSelectListItem(IModel model)
    {
        var fund = (Fund)model;
        return new SelectListItem
        {
            Value = JsonConvert.SerializeObject(fund),
            Text = fund.Name
        };
    }

    public User CurrentUser { get; private set; } = Models.User.Default();
    public List<SelectListItem> Funds { get; private set; } = [];
    public List<Fund> SelectedFunds { get; private set; } = [];

    private class FormData
    {
        public required string Title { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateTime DateOfBirth { get; set; }
        public required decimal Investment { get; set; }
        public required List<SelectListItem> Funds { get; set; }
        public required List<Fund> SelectedFunds { get; set; }
    }
}