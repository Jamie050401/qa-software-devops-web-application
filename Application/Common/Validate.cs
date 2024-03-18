namespace Application.Common;

using AspNetCoreHero.ToastNotification.Abstractions;
using Pages;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public static partial class Validate
{
    [GeneratedRegex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase, "en-GB")]
    private static partial Regex EmailRegex();

    public static bool Email(INotyfService? notyf, string email)
    {
        var isValid = EmailRegex().IsMatch(email);
        if (!isValid) notyf?.Error("Please make sure to enter a valid email address.");
        return isValid;
    }

    public static bool Password(INotyfService? notyf, string password, string confirmPassword)
    {
        var isValid = password == confirmPassword;
        if (!isValid)
        {
            notyf?.Error("Please ensure both passwords match.");
            return isValid;
        }

        isValid = password.Length > 11;
        if (!isValid)
        {
            notyf?.Error("Password must be at least 12 characters long.");
            return isValid;
        }

        isValid = password.Length < 41;
        // ReSharper disable once InvertIf
        if (!isValid)
        {
            notyf?.Error("Password cannot exceed 40 characters.");
            return isValid;
        }

        return isValid;
    }

    // TODO - Add unit tests
    public static bool ProjectionFormData(INotyfService? notyf, Projection.FormData form)
    {
        var isValid = form.FirstName.Length is > 0 and < 41;
        if (!isValid)
        {
            notyf?.Error("Please ensure your first name is valid and less than 41 characters");
            return isValid;
        }

        isValid = form.LastName.Length is > 0 and < 41;
        if (!isValid)
        {
            notyf?.Error("Please ensure your last name is valid and less than 41 characters");
            return isValid;
        }

        isValid = form.DateOfBirth != DateOnly.MinValue;
        if (!isValid)
        {
            notyf?.Error("Please select a valid date of birth");
            return isValid;
        }

        isValid = form.Investment >= 10000.0M;
        if (!isValid)
        {
            notyf?.Error("Investment amount must be at least £10,000.0");
            return isValid;
        }

        isValid = form.InvestmentPercentages.Sum(investmentPercentage => investmentPercentage.Value) == 100.0M;
        // ReSharper disable once InvertIf
        if (!isValid)
        {
            notyf?.Error("Please ensure investment percentages total 100%");
            return isValid;
        }

        return isValid;
    }
}
