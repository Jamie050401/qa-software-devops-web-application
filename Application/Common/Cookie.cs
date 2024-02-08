namespace Application.Common;

using Newtonsoft.Json;

// TODO - Implement some kind of encoding for cookies (since they are stored in plain text - would also prevent HTTP listeners)
public static class Cookie
{
    private static readonly CookieOptions CookieOptions = new()
    {
        Expires = DateTimeOffset.UtcNow.AddDays(7),
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        Secure = false // Ideally this would be true (need to setup HTTPS support)
    };

    public static void Store<TValue>(HttpResponse response, string key, TValue authenticationData, bool isEssential = false, DateTimeOffset? expires = null)
    {
        var cookieOptions = CookieOptions;
        cookieOptions.IsEssential = isEssential;
        if (expires is not null) cookieOptions.Expires = expires;

        var json = JsonConvert.SerializeObject(authenticationData);
        response.Cookies.Append(key, json, cookieOptions);
    }

    public static Response<TValue, Error> Retrieve<TValue>(HttpRequest request, string key)
    {
        var cookie = request.Cookies[key];

        if (cookie is null) return Response<TValue, Error>.OkResponse();

        var authenticationData = JsonConvert.DeserializeObject<TValue>(cookie);
        return authenticationData is null
            ? Response<TValue, Error>.NotFoundResponse()
            : Response<TValue, Error>.OkValueResponse(authenticationData);
    }

    public static void Remove(HttpResponse response, string key)
    {
        response.Cookies.Delete(key);
    }
}
