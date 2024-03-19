namespace Application.Pages;

using AspNetCoreHero.ToastNotification.Abstractions;
using Common;
using Data;
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
                UserResult = result;
            }
            else
            {
                notyf.Error("The specified result could not be found.");
            }
        }
    }

    public User CurrentUser { get; private set; } = Models.User.Default();
    public Result? UserResult { get; private set; }
    public List<Result> UserResults { get; private set; } = [];
}
