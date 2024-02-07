namespace Application.Common;

public static class Session
{
    public static void Authenticate(ISession session, HttpResponse response)
    {
        var isLoggedIn = session.TryGetValue("IsLoggedIn", out var isAuthenticated) && BitConverter.ToBoolean(isAuthenticated);
        var hasLoggedIn = session.TryGetValue("HasLoggedIn", out var hasLogged) && BitConverter.ToBoolean(hasLogged);

        if (isLoggedIn) return;
        if (hasLoggedIn) response.Redirect("/Login", true);
        response.Redirect("/Register", true);
    }

    public static void Redirect(ISession session, HttpResponse response)
    {
        var isLoggedIn = session.TryGetValue("IsLoggedIn", out var isAuthenticated) && BitConverter.ToBoolean(isAuthenticated);

        if (!isLoggedIn) return;
        response.Redirect("/Dashboard", true);
    }

    public static void Login(ISession session, HttpResponse response)
    {
        session.Set("IsLoggedIn", BitConverter.GetBytes(true));
        session.Set("HasLoggedIn", BitConverter.GetBytes(true));
        response.Redirect("/Dashboard", true);
    }

    public static void Logout(ISession session, HttpResponse response)
    {
        session.Set("IsLoggedIn", BitConverter.GetBytes(false));
        response.Redirect("/Login", true);
    }
}
