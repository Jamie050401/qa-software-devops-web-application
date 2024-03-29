﻿namespace Application.Common;

using Data;
using ILogger = Serilog.ILogger;
using Microsoft.AspNetCore.Http;
using Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

public struct SessionVariables
{
    public const string CurrentUser = "CurrentUser";
    public const string HasLoggedIn = "HasLoggedIn";
    public const string IsLoggedIn = "IsLoggedIn";
    public const string IsLogin = "IsLogin";
    public const string IsLogout = "IsLogout";
    public const string LoginFormData = "LoginFormData";
    public const string ProjectionFormData = "ProjectionFormData";
    public const string RegistrationFormData = "RegistrationFormData";
    public const string RegistrationSwitch = "RegistrationSwitch";
    public const string Result = "Result";
    public const string Results = "Results";
}

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

    public static void DeleteObject(ISession session, string key)
    {
        session.Remove(key);
    }

    public static T? GetObject<T>(ISession session, string key)
    {
        var json = GetString(session, key);
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static void SetObject(ISession session, string key, object value)
    {
        var json = JsonConvert.SerializeObject(value);
        SetString(session, key, json);
    }

    public static User GetCurrentUser(ISession session)
    {
        return GetObject<User>(session, SessionVariables.CurrentUser)
               ?? User.Default();
    }

    public static bool Authenticate(ISession session, HttpRequest request, HttpResponse response)
    {
        if (!HasValue(session, SessionVariables.HasLoggedIn))
        {
            var cookieResponse = Cookie.Retrieve<bool>(request, Cookies.HasLoggedIn);
            if (cookieResponse.Status is ResponseStatus.Success && cookieResponse.HasValue)
            {
                SetBoolean(session, SessionVariables.HasLoggedIn, cookieResponse.Value);
            }
        }

        var isLoggedIn = GetBoolean(session, SessionVariables.IsLoggedIn);
        var hasLoggedIn = GetBoolean(session, SessionVariables.HasLoggedIn);

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
        if (!HasValue(session, SessionVariables.HasLoggedIn))
        {
            var cookieResponse = Cookie.Retrieve<bool>(request, Cookies.HasLoggedIn);
            if (cookieResponse.Status is ResponseStatus.Success && cookieResponse.HasValue)
            {
                SetBoolean(session, SessionVariables.HasLoggedIn, cookieResponse.Value);
            }
        }

        var isLoggedIn = GetBoolean(session, SessionVariables.IsLoggedIn);
        var hasLoggedIn = GetBoolean(session, SessionVariables.HasLoggedIn);
        var isRegistrationSwitch = GetBoolean(session, SessionVariables.RegistrationSwitch);

        if (isLoggedIn)
        {
            response.Redirect("/dashboard", true);
            return true;
        }

        // ReSharper disable once InvertIf
        if (hasLoggedIn && !isRegistrationSwitch && request.Path.Value == "/register")
        {
            response.Redirect("/login", true);
            return true;
        }

        return false;
    }

    // TODO - Implement logic to regenerate the token everytime it is used?
    // TODO - Implement support for multiple cookies per user (i.e. 1 per device)
    public static bool TryCookieLogin(ILogger logger, ISession session, ConnectionInfo connectionInfo, HttpRequest request, HttpResponse response)
    {
        var cookieResponse = Cookie.Retrieve<AuthenticationData>(request, Cookies.AuthenticationData);
        if (cookieResponse.Status is ResponseStatus.Error || !cookieResponse.HasValue)
        {
            logger.Information("Login failure: unable to retrieve authentication data from cookies");
            return false;
        }

        Debug.Assert(cookieResponse.Value != null, "cookieResponse.Value != null");
        var dbResponse = DatabaseManager.Database.Read(User.GetProperty("Email"), cookieResponse.Value.Email);
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
        {
            logger.Information("Login failure: unable to find user matching authentication data stored in cookies");
            Cookie.Remove(response, Cookies.AuthenticationData);
            return false;
        }

        Debug.Assert(dbResponse.Value != null, "databaseResponse.Value != null");
        var userInDb = (User)dbResponse.Value;
        var isAuthenticated = userInDb.AuthenticationData is not null &&
                              Secret.Verify(cookieResponse.Value.Token, userInDb.AuthenticationData.Token) &&
                              connectionInfo.RemoteIpAddress?.ToString() == userInDb.AuthenticationData.Source &&
                              cookieResponse.Value.Timestamp == userInDb.AuthenticationData.Timestamp &&
                              cookieResponse.Value.Expires == userInDb.AuthenticationData.Expires &&
                              DateTime.UtcNow < cookieResponse.Value.Expires.DateTime;

        if (!isAuthenticated)
        {
            Cookie.Remove(response, Cookies.AuthenticationData);
            return false;
        }

        SetBoolean(session, SessionVariables.IsLoggedIn, true);
        SetBoolean(session, SessionVariables.HasLoggedIn, true);
        SetBoolean(session, SessionVariables.IsLogin, true);
        SetObject(session, SessionVariables.CurrentUser, userInDb);
        DeleteObject(session, SessionVariables.RegistrationSwitch);
        response.Redirect("/dashboard", true);
        return true;
    }

    public static void Login(ISession session, ConnectionInfo connectionInfo, HttpRequest request, HttpResponse response, bool hasRememberMe, string email, User userInDb)
    {
        if (hasRememberMe)
        {
            var cookieResponse = Cookie.Retrieve<AuthenticationData>(request, Cookies.AuthenticationData);
            if (cookieResponse.Status is ResponseStatus.Error && !cookieResponse.HasValue)
            {
                // TODO - Set 'Expires' via parameter such that the user can decide how long to be remembered for i.e. from 1 day up to 90 days
                var authenticationData = new AuthenticationData
                {
                    Email = email,
                    Token = Secret.Generate(),
                    Source = connectionInfo.RemoteIpAddress is null
                        ? ""
                        : connectionInfo.RemoteIpAddress.ToString(),
                    Timestamp = DateTime.UtcNow,
                    Expires = DateTimeOffset.UtcNow.AddDays(3)
                };
                Cookie.Store(response, Cookies.AuthenticationData, authenticationData, authenticationData.Expires, true);

                authenticationData.Token = Secret.Hash(authenticationData.Token);
                userInDb.AuthenticationData = authenticationData;
                DatabaseManager.Database.Update(userInDb);
            }
        }

        SetBoolean(session, SessionVariables.IsLoggedIn, true);
        SetBoolean(session, SessionVariables.HasLoggedIn, true);
        Cookie.Store(response, Cookies.HasLoggedIn, true, DateTimeOffset.UtcNow.AddDays(90), true);
        SetBoolean(session, SessionVariables.IsLogin, true);
        SetObject(session, SessionVariables.CurrentUser, userInDb);
        DeleteObject(session, SessionVariables.RegistrationSwitch);
        response.Redirect("/dashboard", true);
    }

    public static void Logout(ISession session, HttpResponse response)
    {
        Cookie.Remove(response, Cookies.AuthenticationData);

        SetBoolean(session, SessionVariables.IsLogout, true);
        DeleteObject(session, SessionVariables.IsLoggedIn);
        DeleteObject(session, SessionVariables.CurrentUser);
        response.Redirect("/login", true);
    }

    private static string GetString(ISession session, string key)
    {
        return session.GetString(key) ?? "";
    }

    private static void SetString(ISession session, string key, string value)
    {
        session.SetString(key, value);
    }
}
