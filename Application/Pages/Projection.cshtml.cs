namespace Application.Pages;

using AspNetCoreHero.ToastNotification.Abstractions;
using Common;
using Data;
using Engine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Newtonsoft.Json;
using PageModel = Shared.PageModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Projection(INotyfService notyf) : PageModel
{
    public void OnGet()
    {
        if (!Session.Authenticate(HttpContext.Session, Request, Response)) return;

        CurrentUser = Session.GetCurrentUser(HttpContext.Session);
        Form = this.GetForm();

        if (string.IsNullOrEmpty(Form.FirstName)) Form.FirstName = CurrentUser.FirstName ?? string.Empty;
        if (string.IsNullOrEmpty(Form.LastName)) Form.LastName = CurrentUser.LastName ?? string.Empty;
        Form.Funds = Form.Funds.Count is 0 && Form.SelectedFunds.Count is 0
            ? DatabaseManager.Database.ReadAll<Fund>().Value?.Select(ConvertFundToSelectListItem).ToList() ?? []
            : Form.Funds;
        Session.SetObject(HttpContext.Session, SessionVariables.ProjectionFormData, Form);
    }

    public IActionResult OnPost()
    {
        CurrentUser = Session.GetCurrentUser(HttpContext.Session);
        Form = this.GetForm();
        this.GetFormData();

        var isValid = Validate.ProjectionFormData(notyf, Form);
        if (!isValid)
        {
            Session.SetObject(HttpContext.Session, SessionVariables.ProjectionFormData, Form);
            return this.RedirectToPage("/projection");
        }

        List<Tuple<Guid, decimal>> investmentPercentages = [];
        investmentPercentages.AddRange(Form.InvestmentPercentages.Select(keyValuePair =>
            Tuple.Create(keyValuePair.Key, keyValuePair.Value)));

        List<Domain.InputData.Fund> funds = [];
        funds.AddRange(Form.SelectedFunds.Select(fund =>
            new Domain.InputData.Fund(fund.Id, fund.GrowthRate, fund.Charge)));

        var inputs = new Domain.Inputs(Form.Investment, investmentPercentages, funds);
        var projectedValue = Calculation.Calculate(inputs);

        var result = new Result
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUser.Id,
            TotalInvestment = Form.Investment,
            ProjectedValue = projectedValue
        };

        var dbResponse = DatabaseManager.Database.Create(result);
        if (dbResponse.Status is ResponseStatus.Error)
        {
            Session.SetObject(HttpContext.Session, SessionVariables.ProjectionFormData, Form);
            notyf.Error("Failed to save projection.");
            return this.RedirectToPage("/projection");
        }
        Session.DeleteObject(HttpContext.Session, SessionVariables.ProjectionFormData);

        return this.RedirectToPage("/results", new { id = result.Id });
    }

    public IActionResult OnPostAddFund()
    {
        CurrentUser = Session.GetCurrentUser(HttpContext.Session);
        Form = this.GetForm();
        this.GetFormData();

        var fund = JsonConvert.DeserializeObject<Fund>(Request.Form["SelectedFund"].ToString());
        if (fund is null)
        {
            notyf.Error("Failed to add selected fund.");
            return new EmptyResult();
        }

        Form.SelectedFunds.Add(fund);
        Form.Funds = Form.Funds.Where(element => element.Text != fund.Name).ToList();
        Session.SetObject(HttpContext.Session, SessionVariables.ProjectionFormData, Form);

        return this.RedirectToPage("/projection");
    }

    public IActionResult OnPostDeleteFund()
    {
        CurrentUser = Session.GetCurrentUser(HttpContext.Session);
        Form = this.GetForm();
        this.GetFormData();

        var fundId = Guid.Parse(Request.Form["DeleteFundId"].ToString());
        var fund = Form.SelectedFunds.Find(fund => fund.Id == fundId);

        Debug.Assert(fund != null, nameof(fund) + " != null");
        Form.SelectedFunds.Remove(fund);
        Form.Funds.Add(ConvertFundToSelectListItem(fund));
        Session.SetObject(HttpContext.Session, SessionVariables.ProjectionFormData, Form);

        return this.RedirectToPage("/projection");
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

        if (DateOnly.TryParse(Request.Form["DateOfBirth"].ToString(), out var dateOfBirth))
        {
            Form.DateOfBirth = dateOfBirth;
        }

        if (decimal.TryParse(Request.Form["Investment"].ToString(), out var investment))
        {
            Form.Investment = investment;
        }

        foreach (var fund in Form.SelectedFunds)
        {
            if (decimal.TryParse(Request.Form[$"InvestmentPercentage-{fund.Id}"].ToString(), out var investmentPercentage))
            {
                Form.InvestmentPercentages.ForceAdd(fund.Id, investmentPercentage);
            }
        }
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

        public decimal GetInvestmentPercentage(Guid id)
        {
            return InvestmentPercentages.TryGetValue(id, out var investmentPercentage)
                ? investmentPercentage
                : 0M;
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
                SelectedFunds = [],
                InvestmentPercentages = []
            };
        }

        public required string Title { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateOnly DateOfBirth { get; set; }
        public required decimal Investment { get; set; }
        public required List<SelectListItem> Funds { get; set; }
        public required List<Fund> SelectedFunds { get; init; }
        public required Dictionary<Guid, decimal> InvestmentPercentages { get; init; }
    }

    public User CurrentUser { get; private set; } = Models.User.Default();
    public FormData Form { get; private set; } = FormData.Default();
}
