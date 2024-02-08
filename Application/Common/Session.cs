namespace Application.Common;

using Data;
using ILogger = Serilog.ILogger;
using System.Diagnostics;

public static class Session
{
    public static bool GetBoolean(ISession session, string key)
    {
        return session.TryGetValue(key, out var boolean) && BitConverter.ToBoolean(boolean);
    }

    public static void SetBoolean(ISession session, string key, bool value)
    {
        session.Set(key, BitConverter.GetBytes(value));
    }

    public static void Authenticate(ISession session, HttpResponse response)
    {
        var isLoggedIn = GetBoolean(session, "IsLoggedIn");
        var hasLoggedIn = GetBoolean(session, "HasLoggedIn");

        if (isLoggedIn) return;
        if (hasLoggedIn) response.Redirect("/Login", true);
        response.Redirect("/Register", true);
    }

    public static void Redirect(ISession session, HttpResponse response)
    {
        var isLoggedIn = GetBoolean(session, "IsLoggedIn");

        if (!isLoggedIn) return;
        response.Redirect("/Dashboard", true);
    }

    public static void Login(ILogger logger, ISession session, HttpRequest request, HttpResponse response)
    {
        var cookieResponse = Cookie.Retrieve<AuthenticationData>(request, "QAWA-AuthenticationData");
        if (cookieResponse.Status is ResponseStatus.Error || !cookieResponse.HasValue)
        {
            logger.Information($"Login failure: unable to retrieve authentication data from cookies");
            return;
        }
        Debug.Assert(cookieResponse.Value != null, "cookieResponse.Value != null");
        var authenticationData = cookieResponse.Value;
        var dbResponse = DatabaseManager.Database.GetUserFromDatabase(userEmail: authenticationData.Email);
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
        {
            logger.Information($"Login failure: unable to find user matching authentication data stored in cookies");
            return;
        }
        Debug.Assert(dbResponse.Value != null, "databaseResponse.Value != null");
        var userInDb = dbResponse.Value;
        var isAuthenticated = userInDb.AuthenticationData is not null &&
                              authenticationData.Token == userInDb.AuthenticationData.Token &&
                              authenticationData.Source == userInDb.AuthenticationData.Source &&
                              authenticationData.Timestamp == userInDb.AuthenticationData.Timestamp &&
                              authenticationData.Expires == userInDb.AuthenticationData.Expires &&
                              DateTime.UtcNow < authenticationData.Expires.DateTime;
        SetBoolean(session, "IsLoggedIn", isAuthenticated);
        SetBoolean(session, "HasLoggedIn", isAuthenticated);
        SetBoolean(session, "IsFirstDashboardVisit", isAuthenticated);
        if (isAuthenticated) response.Redirect("/Dashboard", true);
    }

    public static void Login(ISession session, HttpResponse response)
    {
        SetBoolean(session, "IsLoggedIn", true);
        SetBoolean(session, "HasLoggedIn", true);
        SetBoolean(session, "IsFirstDashboardVisit", true);
        response.Redirect("/Dashboard", true);
    }

    public static void Logout(ISession session, HttpRequest request, HttpResponse response)
    {
        Cookie.Remove(response, "QAWA-AuthenticationData");

        SetBoolean(session, "IsLoggedIn", false);
        response.Redirect("/Login", true);
    }
}
