namespace Application.Common;

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

    public static void Login(ISession session, HttpResponse response)
    {
        SetBoolean(session, "IsLoggedIn", true);
        SetBoolean(session, "HasLoggedIn", true);
        SetBoolean(session, "IsFirstDashboardVisit", true);
        response.Redirect("/Dashboard", true);
    }

    public static void Logout(ISession session, HttpResponse response)
    {
        SetBoolean(session, "IsLoggedIn", false);
        response.Redirect("/Login", true);
    }
}
