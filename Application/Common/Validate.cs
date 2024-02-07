namespace Application.Common;

using System.Text.RegularExpressions;

public static partial class Validate
{
    public static bool Email(string email)
    {
        return EmailRegex().IsMatch(email);
    }

    public static bool Password(string password)
    {
        var isValid = true;

        // TODO - Enforce complexity requirements ...

        return isValid;
    }

    [GeneratedRegex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase, "en-GB")]
    private static partial Regex EmailRegex();
}