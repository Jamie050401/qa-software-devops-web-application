namespace Application.Common;

using Newtonsoft.Json;

public struct Cookies
{
    public const string AuthenticationData = "QAWA-AuthenticationData";
    public const string HasLoggedIn = "QAWA-HasLoggedIn";
}

// TODO - Implement a means of detecting external manipulation of the encrypted cookie (invalidate it if it has been modified)
public static class Cookie
{
    private static readonly string PassPhrase = Environment.GetEnvironmentVariable("QAWA-Cookie-Secret") ?? throw new Exception("Failed to read cookie secret from environment variables");
    private static readonly CookieOptions CookieOptions = new()
    {
        Expires = DateTimeOffset.UtcNow.AddDays(7),
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        Secure = false // Ideally this would be true (need to setup HTTPS support)
    };

    public static void Store<TValue>(HttpResponse response, string key, TValue data, DateTimeOffset? expires = null, bool isEssential = false)
    {
        var cookieOptions = CookieOptions;
        cookieOptions.IsEssential = isEssential;
        if (expires is not null) cookieOptions.Expires = expires;

        var json = JsonConvert.SerializeObject(data);
        var encrypted = StringCipher.Encrypt(json, PassPhrase);
        response.Cookies.Append(key, encrypted, cookieOptions);
    }

    public static Response<TValue, Error> Retrieve<TValue>(HttpRequest request, string key)
    {
        var cookie = request.Cookies[key];

        if (cookie is null) return Response<TValue, Error>.NotFoundResponse();

        var decrypted = StringCipher.Decrypt(cookie, PassPhrase);
        var data = JsonConvert.DeserializeObject<TValue>(decrypted);
        return data is null
            ? Response<TValue, Error>.NotFoundResponse()
            : Response<TValue, Error>.OkValueResponse(data);
    }

    public static void Remove(HttpResponse response, string key)
    {
        response.Cookies.Delete(key);
    }
}
