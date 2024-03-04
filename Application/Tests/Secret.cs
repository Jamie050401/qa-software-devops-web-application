namespace Application.Tests;

using Common;
using NUnit.Framework;
using System.Linq;

[TestFixture]
public class SecretGenerate
{
    [Test]
    public void GeneratesSecret()
    {
        var actual = Secret.Generate();

        Assert.That(actual.Length, Is.EqualTo(24));
        Assert.That(actual.Distinct().Count(), Is.GreaterThanOrEqualTo(8));
        Assert.That(actual.Any(char.IsDigit), Is.EqualTo(true));
        Assert.That(actual.Any(char.IsLower), Is.EqualTo(true));
        Assert.That(actual.Any(char.IsUpper), Is.EqualTo(true));
        Assert.That(actual.Any(c => !char.IsLetter(c) && !char.IsDigit(c)));
    }
}

[TestFixture]
public class SecretEncrypt
{
    private const string PlainText = "This is a test string that should be encrypted!";
    private readonly string _passPhrase = Secret.Generate();

    [Test]
    public void EncryptsSecret()
    {
        var actual = Secret.Encrypt(PlainText, _passPhrase);

        Assert.That(actual, !Is.EqualTo(PlainText));
    }
}

[TestFixture]
public class SecretDecrypt
{
    private const string PlainText = "This is a test string that should be encrypted!";
    private readonly string _passPhrase = Secret.Generate();
    private string _cipherText = null!;

    [SetUp]
    public void SecretDecryptSetup()
    {
        _cipherText = Secret.Encrypt(PlainText, _passPhrase);
    }

    [Test]
    public void DecryptsSecret()
    {
        var actual = Secret.Decrypt(_cipherText, _passPhrase);

        Assert.That(actual, Is.EqualTo(PlainText));
    }
}

[TestFixture]
public class SecretHash
{
    private readonly string _credential = Secret.Generate();

    [Test]
    public void HashesSecret()
    {
        var actual = Secret.Hash(_credential);

        Assert.That(actual, !Is.EqualTo(_credential));
    }
}

[TestFixture]
public class SecretVerify
{
    private readonly string _credential = Secret.Generate();
    private string _hashedCredential = null!;

    [SetUp]
    public void SecretVerifySetup()
    {
        _hashedCredential = Secret.Hash(_credential);
    }

    [Test]
    public void VerifiesCorrectSecret()
    {
        var actual = Secret.Verify(_credential, _hashedCredential);

        Assert.That(actual, Is.EqualTo(true));
    }

    [Test]
    public void VerifiesIncorrectSecret()
    {
        var actual = Secret.Verify(Secret.Generate(), _hashedCredential);

        Assert.That(actual, Is.EqualTo(false));
    }
}
