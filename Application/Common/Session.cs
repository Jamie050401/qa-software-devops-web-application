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

    public static void Authenticate(ISession session, HttpRequest request, HttpResponse response)
    {
        var loginCookieResponse = Cookie.Retrieve<bool>(request, "QAWA-HasLoggedIn");
        if (loginCookieResponse.Status is ResponseStatus.Success && loginCookieResponse.HasValue)
        {
            SetBoolean(session, "HasLoggedIn", loginCookieResponse.Value);
        }

        var isLoggedIn = GetBoolean(session, "IsLoggedIn");
        var hasLoggedIn = GetBoolean(session, "HasLoggedIn");

        if (isLoggedIn) return;
        if (hasLoggedIn) response.Redirect("/Login", true);
        response.Redirect("/Register", true);
    }

    public static bool Redirect(ISession session, HttpRequest request, HttpResponse response)
    {
        var loginCookieResponse = Cookie.Retrieve<bool>(request, "QAWA-HasLoggedIn");
        if (loginCookieResponse.Status is ResponseStatus.Success && loginCookieResponse.HasValue)
        {
            SetBoolean(session, "HasLoggedIn", loginCookieResponse.Value);
        }

        var isLoggedIn = GetBoolean(session, "IsLoggedIn");
        var hasLoggedIn = GetBoolean(session, "HasLoggedIn");

        if (isLoggedIn)
        {
            response.Redirect("/Dashboard", true);
            return true;
        }

        // ReSharper disable once InvertIf
        if (hasLoggedIn && request.Path.Value == "/Register")
        {
            response.Redirect("/Login", true);
            return true;
        }

        return false;
    }

    // TODO - Implement logic to regenerate the token everytime it is used?
    // TODO - Implement support for users to have multiple valid tokens (i.e. for different locations or devices)
    public static void Login(ILogger logger, ISession session, HttpRequest request, HttpResponse response)
    {
        var authCookieResponse = Cookie.Retrieve<AuthenticationData>(request, "QAWA-AuthenticationData");
        if (authCookieResponse.Status is ResponseStatus.Error || !authCookieResponse.HasValue)
        {
            logger.Information($"Login failure: unable to retrieve authentication data from cookies");
            return;
        }
        Debug.Assert(authCookieResponse.Value != null, "cookieResponse.Value != null");
        var authenticationData = authCookieResponse.Value;
        var dbResponse = DatabaseManager.Database.GetUserFromDatabase(userEmail: authenticationData.Email);
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
        {
            logger.Information($"Login failure: unable to find user matching authentication data stored in cookies");
            return;
        }
        Debug.Assert(dbResponse.Value != null, "databaseResponse.Value != null");
        var userInDb = dbResponse.Value;
        var isAuthenticated = userInDb.AuthenticationData is not null &&
                              SecretHasher.Verify(authenticationData.Token, userInDb.AuthenticationData.Token) &&
                              authenticationData.Source == userInDb.AuthenticationData.Source &&
                              authenticationData.Timestamp == userInDb.AuthenticationData.Timestamp &&
                              authenticationData.Expires == userInDb.AuthenticationData.Expires &&
                              DateTime.UtcNow < authenticationData.Expires.DateTime;
        SetBoolean(session, "IsLoggedIn", isAuthenticated);
        if (isAuthenticated) SetBoolean(session, "HasLoggedIn", true);
        if (isAuthenticated) SetBoolean(session, "IsFirstDashboardVisit", true);
        if (isAuthenticated) response.Redirect("/Dashboard", true);
    }

    public static void Login(ISession session, HttpResponse response)
    {
        SetBoolean(session, "IsLoggedIn", true);
        SetBoolean(session, "HasLoggedIn", true);
        Cookie.Store(response, "QAWA-HasLoggedIn", true, true);
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
