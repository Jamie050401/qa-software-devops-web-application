namespace Application.Tests;

using Common;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;

[TestFixture]
public class ValidateEmail
{
    [Test]
    public void ValidEmailAllowed()
    {
        const string email = "test@email.com";

        var actual = Validate.Email(null, email);

        Assert.That(actual, Is.EqualTo(true));
    }

    [Test]
    public void InvalidEmailDenied()
    {
        // ReSharper disable once StringLiteralTypo
        const string email = "testemail.com";

        var actual = Validate.Email(null, email);

        Assert.That(actual, Is.EqualTo(false));
    }
}

[TestFixture]
public class ValidatePassword
{
    [Test]
    public void ValidPasswordAllowed()
    {
        // ReSharper disable once StringLiteralTypo
        const string password = "thisisavalidpassword!";

        var actual = Validate.Password(null, password, password);

        Assert.That(actual, Is.EqualTo(true));
    }

    [Test]
    public void MismatchedPasswordDenied()
    {
        // ReSharper disable once StringLiteralTypo
        const string password = "thisisavalidpassword!";

        var actual = Validate.Password(null, password, $"{password}!");

        Assert.That(actual, Is.EqualTo(false));
    }

    [Test]
    public void ShortPasswordDenied()
    {
        // ReSharper disable once StringLiteralTypo
        const string password = "tooshort";

        var actual = Validate.Password(null, password, password);

        Assert.That(actual, Is.EqualTo(false));
    }

    [Test]
    public void LongPasswordDenied()
    {
        var passwordOptions = new PasswordOptions
        {
            RequiredLength = 60,
            RequiredUniqueChars = 8,
            RequireDigit = true,
            RequireLowercase = true,
            RequireNonAlphanumeric = true,
            RequireUppercase = true
        };

        // ReSharper disable once StringLiteralTypo
        var password = Secret.Generate(passwordOptions);

        var actual = Validate.Password(null, password, password);

        Assert.That(actual, Is.EqualTo(false));
    }
}