namespace Application.Common;

using AspNetCoreHero.ToastNotification.Abstractions;
using System.Text.RegularExpressions;

public static partial class Validate
{
    [GeneratedRegex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase, "en-GB")]
    private static partial Regex EmailRegex();

    public static bool Email(INotyfService notyf, string email)
    {
        var isValid = EmailRegex().IsMatch(email);
        if (!isValid) notyf.Error("Please make sure to enter a valid email address.");
        return isValid;
    }

    public static bool Password(INotyfService notyf, string password, string confirmPassword)
    {
        var isValid = password == confirmPassword;
        if (!isValid)
        {
            notyf.Error("Please ensure both passwords match.");
            return isValid;
        }

        isValid = password.Length > 11;
        if (!isValid)
        {
            notyf.Error("Password must be at least 12 characters long.");
            return isValid;
        }

        // TODO - Enforce complexity requirements ...

        return isValid;
    }
}
