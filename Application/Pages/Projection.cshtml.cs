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

        CurrentUser =
            Session.GetObject<User>(HttpContext.Session, Session.Variables.CurrentUser)
            ?? Models.User.Default();

        Title = formData?.Title ?? string.Empty;
        FirstName = formData?.FirstName ?? CurrentUser.FirstName ?? string.Empty;
        LastName = formData?.LastName ?? CurrentUser.LastName ?? string.Empty;
        DateOfBirth = formData?.DateOfBirth ?? DateOnly.MinValue;
        Investment = formData?.Investment ?? 0.0M;
        Funds = formData is null
            ? DatabaseManager.Database.ReadAll<Fund>().Value?.Select(ConvertFundToSelectListItem).ToList() ?? []
            : formData.Funds;
        SelectedFunds = formData is null
            ? []
            : formData.SelectedFunds;

        formData = new FormData
        {
            Title = Title,
            FirstName = FirstName,
            LastName = LastName,
            DateOfBirth = DateOfBirth,
            Investment = Investment,
            Funds = Funds,
            SelectedFunds = SelectedFunds
        };
        Session.SetObject(HttpContext.Session, Session.Variables.ProjectionFormData, formData);
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
        var formData = Session.GetObject<FormData>(HttpContext.Session, Session.Variables.ProjectionFormData);
        Funds = formData?.Funds ?? Funds;
        SelectedFunds = formData?.SelectedFunds ?? SelectedFunds;

        var fund = JsonConvert.DeserializeObject<Fund>(Request.Form["SelectedFund"].ToString());
        if (fund is null)
        {
            Response.Redirect("/projection");
            return;
        }

        SelectedFunds.Add(fund);
        Funds = Funds.Where(element => element.Text != fund.Name).ToList();

        formData = new FormData
        {
            Title = Request.Form["Title"].ToString(),
            FirstName = Request.Form["FirstName"].ToString(),
            LastName = Request.Form["LastName"].ToString(),
            DateOfBirth = DateOnly.Parse(Request.Form["DateOfBirth"].ToString()),
            Investment = decimal.Parse(Request.Form["Investment"].ToString()),
            Funds = Funds,
            SelectedFunds = SelectedFunds
        };
        Session.SetObject(HttpContext.Session, Session.Variables.ProjectionFormData, formData);

        Response.Redirect("/projection");
    }

    public void OnPostDeleteFund()
    {
        // ...

        Response.Redirect("/projection");
    }

    public string GetDateOfBirthAsString()
    {
        return $"{DateOfBirth.Year}-{DateOfBirth.Month.ToString().PadLeft(2, '0')}-{DateOfBirth.Day.ToString().PadLeft(2, '0')}";
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
    public string Title { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public decimal Investment { get; private set; }
    public List<SelectListItem> Funds { get; private set; } = [];
    public List<Fund> SelectedFunds { get; private set; } = [];

    private class FormData
    {
        public required string Title { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required DateOnly DateOfBirth { get; init; }
        public required decimal Investment { get; init; }
        public required List<SelectListItem> Funds { get; init; }
        public required List<Fund> SelectedFunds { get; init; }
    }
}
