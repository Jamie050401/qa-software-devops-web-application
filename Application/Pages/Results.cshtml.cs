namespace Application.Pages;

using AspNetCoreHero.ToastNotification.Abstractions;
using Common;
using Data;
using Microsoft.AspNetCore.Mvc;
using Models;
using PageModel = Shared.PageModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Results(INotyfService notyf) : PageModel
{
    public void OnGet(Guid? id)
    {
        if (!Session.Authenticate(HttpContext.Session, Request, Response))
            return;

        CurrentUser = Session.GetCurrentUser(HttpContext.Session);
        this.GetUserResults(id);
    }

    public IActionResult OnPost()
    {
        CurrentUser = Session.GetCurrentUser(HttpContext.Session);
        UserResults = this.GetUserResults();
        UserResult = this.GetUserResult();

        var resultId = Guid.Parse(Request.Form["DeleteResultId"].ToString());
        DatabaseManager.Database.Delete(Result.GetProperty("Id"), resultId);
        this.GetUserResults(null);

        return this.RedirectToPage("/results");
    }

    private List<Result> GetUserResults()
    {
        return Session.GetObject<List<Result>>(HttpContext.Session, SessionVariables.Results)
            ?? [];
    }

    private Guid? GetUserResult()
    {
        return Session.GetObject<Guid?>(HttpContext.Session, SessionVariables.Result);
    }

    private void GetUserResults(Guid? id)
    {
        var dbResponse = DatabaseManager.Database.ReadAll<Result>();
        if (dbResponse.Status is ResponseStatus.Success && dbResponse.HasValue)
        {
            Debug.Assert(dbResponse.Value != null, "dbResponse.Value != null");
            UserResults = dbResponse.Value.Select(model => (Result)model).ToList();
        }

        // ReSharper disable once InvertIf
        if (id.HasValue)
        {
            var result = UserResults.Find(result => result.Id == id);

            if (result?.UserId == CurrentUser.Id)
            {
                UserResult = result.Id;
            }
            else
            {
                notyf.Error("The specified result could not be found.");
            }
        }

        UserResults.Sort((x, y) => DateTime.Compare(x.ProjectionDate, y.ProjectionDate));
        UserResults.Reverse();
        UserResults = UserResults.Take(5).ToList();

        Session.SetObject(HttpContext.Session, SessionVariables.Result, UserResult ?? Guid.Empty);
        Session.SetObject(HttpContext.Session, SessionVariables.Results, UserResults);
    }

    public User CurrentUser { get; private set; } = Models.User.Default();
    public List<Result> UserResults { get; private set; } = [];
    public Guid? UserResult { get; private set; }
}
