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
        File.Delete("Tests/database.sqlite");
        _database = new DatabaseLogic("Tests");
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

        var actual = _database.Create(fund);

        Assert.That(actual, Is.EqualTo(Response<Fund, Error>.OkResponse()));
    }

    [Test, Order(2)]
    public void CreateRole()
    {
        var role = new Role
        {
            Name = RoleName
        };

        var actual = _database.Create(role);

        Assert.That(actual, Is.EqualTo(Response<Role, Error>.OkResponse()));
    }

    [Test, Order(3)]
    public void CreateUser()
    {
        var user = new User
        {
            Id = _userGuid,
            Email = "test@email.com",
            Password = "test",
            RoleName = RoleName
        };

        var actual = _database.Create(user);

        Assert.That(actual, Is.EqualTo(Response<User, Error>.OkResponse()));
    }

    [Test, Order(4)]
    public void CreateResult()
    {
        var result = new Result
        {
            Id = Guid.NewGuid(),
            UserId = _userGuid,
            TotalInvestment = 0.0M,
            ProjectedValue = 0.0M
        };

        var actual = _database.Create(result);

        Assert.That(actual, Is.EqualTo(Response<Result, Error>.OkResponse()));
    }
}