namespace Application.Common;

using Newtonsoft.Json;

// TODO - Implement some kind of encoding for cookies (since they are stored in plain text)
public static class Cookie
{
    private static readonly CookieOptions CookieOptions = new()
    {
        Expires = DateTimeOffset.UtcNow.AddDays(7),
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        Secure = false // Ideally this would be true (need to setup HTTPS support)
    };

    public static void Store(HttpResponse response, string key, IDictionary<string, object> dictionary, bool isEssential = false)
    {
        var cookieOptions = CookieOptions;
        cookieOptions.IsEssential = isEssential;

        response.Cookies.Append(key, JsonConvert.SerializeObject(dictionary), cookieOptions);
    }

    public static IDictionary<string, object> Retrieve(HttpRequest request, string key)
    {
        var cookie = request.Cookies[key];

        if (cookie is null) return new Dictionary<string, object>();

        return JsonConvert.DeserializeObject<IDictionary<string, object>>(cookie) ?? new Dictionary<string, object>();
    }
}