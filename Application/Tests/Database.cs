namespace Application.Tests;

using Common;
using DatabaseLogic = Data.Database;
using NUnit.Framework;
using Models;

[TestFixture]
public class DatabaseCreate
{
    private readonly DatabaseLogic _database;
    private readonly Guid _userGuid = Guid.NewGuid();
    private const string RoleName = "Test Role";

    public DatabaseCreate()
    {
        File.Delete("Tests/createDatabase.sqlite");
        _database = new DatabaseLogic("Tests", "createDatabase");
    }

    [Test, Order(1)]
    public void CreateFund()
    {
        var fund = new Fund
        {
            Id = Guid.NewGuid(),
            Name = "Test Fund",
            GrowthRate = 0.0M,
            Charge = 0.0M
        };

        var actual = _database.CreateUpdate(fund);

        Assert.That(actual, Is.EqualTo(Response<Fund, Error>.OkResponse()));
    }

    [Test, Order(2)]
    public void CreateRole()
    {
        var role = new Role
        {
            Name = RoleName
        };

        var actual = _database.CreateUpdate(role);

        Assert.That(actual, Is.EqualTo(Response<Role, Error>.OkResponse()));
    }

    [Test, Order(3)]
    public void CreateUser()
    {
        var user = new User
        {
            Id = _userGuid,
            Email = "test@one.com",
            Password = "test",
            RoleName = RoleName
        };

        var actual = _database.CreateUpdate(user);

        Assert.That(actual, Is.EqualTo(Response<User, Error>.OkResponse()));
    }

    [Test, Order(4)]
    public void CreateAnotherUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@two.com",
            Password = "test",
            AuthenticationData = new AuthenticationData
            {
                Email = "test@two.com",
                Token = "TestToken",
                Source = "127.0.0.1",
                Timestamp = DateTime.UtcNow,
                Expires = DateTimeOffset.UtcNow.AddDays(3)
            },
            FirstName = "Test",
            LastName = "Two",
            RoleName = RoleName
        };

        var actual = _database.CreateUpdate(user);

        Assert.That(actual, Is.EqualTo(Response<User, Error>.OkResponse()));
    }

    [Test, Order(5)]
    public void CreateResult()
    {
        var result = new Result
        {
            Id = Guid.NewGuid(),
            UserId = _userGuid,
            TotalInvestment = 0.0M,
            ProjectedValue = 0.0M
        };

        var actual = _database.CreateUpdate(result);

        Assert.That(actual, Is.EqualTo(Response<Result, Error>.OkResponse()));
    }
}

[TestFixture]
public class DatabaseUpdate
{
    private readonly DatabaseLogic _database;
    private readonly Guid _fundGuid = Guid.NewGuid();
    private readonly Guid _userGuid = Guid.NewGuid();
    private readonly Guid _resultGuid = Guid.NewGuid();
    private const string RoleName = "Test Role";

    public DatabaseUpdate()
    {
        File.Delete("Tests/updateDatabase.sqlite");
        _database = new DatabaseLogic("Tests", "updateDatabase");

        _database.CreateUpdate(new Fund
        {
            Id = _fundGuid,
            Name = "Test Fund",
            GrowthRate = 0.0M,
            Charge = 0.0M
        });
        _database.CreateUpdate(new Role
        {
            Name = RoleName
        });
        _database.CreateUpdate(new User
        {
            Id = _userGuid,
            Email = "test@user.com",
            Password = "TestUser",
            RoleName = RoleName
        });
        _database.CreateUpdate(new Result
        {
            Id = _resultGuid,
            UserId = _userGuid,
            TotalInvestment = 0.0M,
            ProjectedValue = 0.0M
        });
    }

    [Test]
    public void UpdateFund()
    {
        var fund = new Fund
        {
            Id = _fundGuid,
            Name = "Test Fund",
            GrowthRate = 1.0M,
            Charge = 1.0M
        };

        var actual = _database.CreateUpdate(fund, true);

        Assert.That(actual, Is.EqualTo(Response<Fund, Error>.OkResponse()));
    }
}