namespace Application.Common;

using Data;
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

    public static void Login(ISession session, HttpRequest request, HttpResponse response)
    {
        var authenticationData = Cookie.Retrieve(request, "QAWA-AuthenticationData");
        if (!authenticationData.TryGetValue("Email", out var email) ||
            !authenticationData.TryGetValue("Token", out var token) ||
            !authenticationData.TryGetValue("Source", out var source)) return;
        var userInDb = DatabaseManager.Database.GetUserFromDatabase(userEmail: (string)email);
        if (userInDb.Status is ResponseStatus.Error || !userInDb.HasValue) return;
        Debug.Assert(userInDb.Value != null, "userInDb.Value != null");
        var isAuthenticated = (string)token == userInDb.Value.Token && (string)source == userInDb.Value.TokenSource;
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
        var authenticationData = Cookie.Retrieve(request, "QAWA-AuthenticationData");
        authenticationData.Remove("Email");
        authenticationData.Remove("Token");
        authenticationData.Remove("Source");
        Cookie.Store(response, "QAWA-AuthenticationData", authenticationData, true);

        SetBoolean(session, "IsLoggedIn", false);
        response.Redirect("/Login", true);
    }
}
