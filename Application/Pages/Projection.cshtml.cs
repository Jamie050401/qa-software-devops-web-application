namespace Application.Pages;

using Common;
using Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Newtonsoft.Json;
using PageModel = Shared.PageModel;
using System;
using System.Collections.Generic;
using System.Linq;

public class Projection : PageModel
{
    public void OnGet()
    {
        if (!Session.Authenticate(HttpContext.Session, Request, Response)) return;

        Form = this.GetForm();
        CurrentUser =
            Session.GetObject<User>(HttpContext.Session, SessionVariables.CurrentUser)
            ?? Models.User.Default();

        if (string.IsNullOrEmpty(Form.FirstName)) Form.FirstName = CurrentUser.FirstName ?? string.Empty;
        if (string.IsNullOrEmpty(Form.LastName)) Form.LastName = CurrentUser.LastName ?? string.Empty;
        Form.Funds = Form.Funds.Count is 0 && Form.SelectedFunds.Count is 0
            ? DatabaseManager.Database.ReadAll<Fund>().Value?.Select(ConvertFundToSelectListItem).ToList() ?? []
            : Form.Funds;
        Form.SelectedFunds = Form.SelectedFunds;
        Session.SetObject(HttpContext.Session, SessionVariables.ProjectionFormData, Form);
    }

    public void OnPost()
    {
        Form = this.GetForm();
        this.GetFormData();

        var result = Result.Default(); // TODO - Perform calculation and return 'Result'

        // TODO - Store 'Result' in database

        Session.DeleteObject(HttpContext.Session, SessionVariables.ProjectionFormData);

        Response.Redirect($"/results?id={result.Id}");
    }

    public void OnPostAddFund()
    {
        Form = this.GetForm();
        this.GetFormData();

        var fund = JsonConvert.DeserializeObject<Fund>(Request.Form["SelectedFund"].ToString());
        if (fund is null)
        {
            Response.Redirect("/projection");
            return;
        }

        Form.SelectedFunds.Add(fund);
        Form.Funds = Form.Funds.Where(element => element.Text != fund.Name).ToList();
        Session.SetObject(HttpContext.Session, SessionVariables.ProjectionFormData, Form);

        Response.Redirect("/projection");
    }

    public void OnPostDeleteFund()
    {
        Form = this.GetForm();
        this.GetFormData();

        // ...

        Response.Redirect("/projection");
    }

    private FormData GetForm()
    {
        return Session.GetObject<FormData>(HttpContext.Session, SessionVariables.ProjectionFormData)
               ?? FormData.Default();
    }

    private void GetFormData()
    {
        Form.Title = Request.Form["Title"].ToString();
        Form.FirstName = Request.Form["FirstName"].ToString();
        Form.LastName = Request.Form["LastName"].ToString();
        Form.DateOfBirth = DateOnly.Parse(Request.Form["DateOfBirth"].ToString());
        Form.Investment = decimal.Parse(Request.Form["Investment"].ToString());
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

    public class FormData
    {
        public string GetDateOfBirthAsString()
        {
            var day = DateOfBirth.Day.ToString().PadLeft(2, '0');
            var month = DateOfBirth.Month.ToString().PadLeft(2, '0');
            return $"{DateOfBirth.Year}-{month}-{day}";
        }

        public static FormData Default()
        {
            return new FormData
            {
                Title = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                DateOfBirth = DateOnly.MinValue,
                Investment = 0.0M,
                Funds = [],
                SelectedFunds = []
            };
        }

        public required string Title { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateOnly DateOfBirth { get; set; }
        public required decimal Investment { get; set; }
        public required List<SelectListItem> Funds { get; set; }
        public required List<Fund> SelectedFunds { get; set; }
    }

    public User CurrentUser { get; private set; } = Models.User.Default();
    public FormData Form { get; private set; } = FormData.Default();
}
