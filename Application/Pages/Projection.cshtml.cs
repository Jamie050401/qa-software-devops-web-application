﻿namespace Application.Pages;

using Common;
using Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using PageModel = Shared.PageModel;

// TODO - Look into model binding with ASP.NET Razor Pages
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
                Value = fund.Id.ToString(),
                Text = fund.Name
            };
        }).ToList() ?? [];
        SelectedFunds = [];
    }

    public void OnPost()
    {
        // This should be called to submit the projection form
        // ...
    }

    public void OnPostFunds()
    {
        // This should be called to add a new fund to SelectedFunds
        // ...
    }

    public User? CurrentUser { get; private set; }
    public List<SelectListItem>? Funds { get; private set; }
    public List<SelectListItem>? SelectedFunds { get; private set; }
}