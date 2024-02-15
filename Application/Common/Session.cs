namespace Application.Common;

using Data;
using ILogger = Serilog.ILogger;
using Models;
using System.Diagnostics;

public static class Session
{
    public static bool HasValue(ISession session, string key)
    {
        return session.TryGetValue(key, out _);
    }

    public static bool GetBoolean(ISession session, string key)
    {
        return session.TryGetValue(key, out var boolean) && BitConverter.ToBoolean(boolean);
    }

    public static void SetBoolean(ISession session, string key, bool value)
    {
        session.Set(key, BitConverter.GetBytes(value));
    }

    public static bool Authenticate(ISession session, HttpRequest request, HttpResponse response)
    {
        if (!HasValue(session, "HasLoggedIn"))
        {
            var loginCookieResponse = Cookie.Retrieve<bool>(request, "QAWA-HasLoggedIn");
            if (loginCookieResponse.Status is ResponseStatus.Success && loginCookieResponse.HasValue)
            {
                SetBoolean(session, "HasLoggedIn", loginCookieResponse.Value);
            }
        }

        var isLoggedIn = GetBoolean(session, "IsLoggedIn");
        var hasLoggedIn = GetBoolean(session, "HasLoggedIn");

        if (isLoggedIn)
        {
            return true;
        }

        if (hasLoggedIn)
        {
            response.Redirect("/login", true);
            return false;
        }

        response.Redirect("/register", true);
        return false;
    }

    public static bool Redirect(ISession session, HttpRequest request, HttpResponse response)
    {
        if (!HasValue(session, "HasLoggedIn"))
        {
            var loginCookieResponse = Cookie.Retrieve<bool>(request, "QAWA-HasLoggedIn");
            if (loginCookieResponse.Status is ResponseStatus.Success && loginCookieResponse.HasValue)
            {
                SetBoolean(session, "HasLoggedIn", loginCookieResponse.Value);
            }
        }

        var isLoggedIn = GetBoolean(session, "IsLoggedIn");
        var hasLoggedIn = GetBoolean(session, "HasLoggedIn");

        if (isLoggedIn)
        {
            response.Redirect("/dashboard", true);
            return true;
        }

        // ReSharper disable once InvertIf
        if (hasLoggedIn && request.Path.Value == "/register")
        {
            response.Redirect("/login", true);
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
        var dbResponse = DatabaseManager.Database.Read(new User
        {
            Email = authenticationData.Email
        });
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
        {
            logger.Information($"Login failure: unable to find user matching authentication data stored in cookies");
            Cookie.Remove(response, "QAWA-AuthenticationData");
            return;
        }
        Debug.Assert(dbResponse.Value != null, "databaseResponse.Value != null");
        var userInDb = (User)dbResponse.Value;
        var isAuthenticated = userInDb.AuthenticationData is not null &&
                              SecretHasher.Verify(authenticationData.Token, userInDb.AuthenticationData.Token) &&
                              authenticationData.Source == userInDb.AuthenticationData.Source &&
                              authenticationData.Timestamp == userInDb.AuthenticationData.Timestamp &&
                              authenticationData.Expires == userInDb.AuthenticationData.Expires &&
                              DateTime.UtcNow < authenticationData.Expires.DateTime;

        if (!isAuthenticated)
        {
            Cookie.Remove(response, "QAWA-AuthenticationData");
            return;
        }

        SetBoolean(session, "IsLoggedIn", true);
        SetBoolean(session, "HasLoggedIn", true);
        SetBoolean(session, "IsFirstDashboardVisit", true);
        response.Redirect("/dashboard", true);
    }

    public static void Login(ISession session, ConnectionInfo connectionInfo, HttpRequest request, HttpResponse response, bool hasRememberMe, string email, User userInDb)
    {
        if (hasRememberMe)
        {
            var cookieResponse = Cookie.Retrieve<AuthenticationData>(request, "QAWA-AuthenticationData");
            if (cookieResponse.Status is ResponseStatus.Error)
            {
                if (!cookieResponse.HasValue)
                {
                    // TODO - Set 'Expires' via parameter such that the user can decide how long to be remembered for i.e. from 1 day up to 90 days
                    var authenticationData = new AuthenticationData
                    {
                        Email = email,
                        Token = Password.Generate(),
                        Source = connectionInfo.RemoteIpAddress is null
                            ? ""
                            : connectionInfo.RemoteIpAddress.ToString(),
                        Timestamp = DateTime.UtcNow,
                        Expires = DateTimeOffset.UtcNow.AddDays(3)
                    };
                    Cookie.Store(response, "QAWA-AuthenticationData", authenticationData, authenticationData.Expires, true);

                    authenticationData.Token = SecretHasher.Hash(authenticationData.Token);
                    userInDb.AuthenticationData = authenticationData;
                    DatabaseManager.Database.Update(userInDb);
                }
            }
        }

        SetBoolean(session, "IsLoggedIn", true);
        SetBoolean(session, "HasLoggedIn", true);
        Cookie.Store(response, "QAWA-HasLoggedIn", true, DateTimeOffset.UtcNow.AddDays(90), true);
        SetBoolean(session, "IsFirstDashboardVisit", true);
        response.Redirect("/dashboard", true);
    }

    public static void Logout(ISession session, HttpRequest request, HttpResponse response)
    {
        Cookie.Remove(response, "QAWA-AuthenticationData");

        SetBoolean(session, "IsLogout", true);
        SetBoolean(session, "IsLoggedIn", false);
        response.Redirect("/login", true);
    }
}
