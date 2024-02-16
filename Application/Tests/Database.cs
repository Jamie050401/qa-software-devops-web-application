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

        var actual = _database.Create(fund);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
    }

    [Test, Order(2)]
    public void CreateRole()
    {
        var role = new Role
        {
            Name = RoleName
        };

        var actual = _database.Create(role);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
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

        var actual = _database.Create(user);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
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

        var actual = _database.Create(user);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
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

        var actual = _database.Create(result);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
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

        _database.Create(new Fund
        {
            Id = _fundGuid,
            Name = "Test Fund",
            GrowthRate = 0.0M,
            Charge = 0.0M
        });
        _database.Create(new Role
        {
            Name = RoleName
        });
        _database.Create(new User
        {
            Id = _userGuid,
            Email = "test@user.com",
            Password = "TestUser",
            RoleName = RoleName
        });
        _database.Create(new Result
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

        var actual = _database.Update(fund);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
    }
}

[TestFixture]
public class DatabaseRead
{
    private readonly DatabaseLogic _database;
    private readonly Guid _fundGuid = Guid.NewGuid();

    public DatabaseRead()
    {
        File.Delete("Tests/readDatabase.sqlite");
        _database = new DatabaseLogic("Tests", "readDatabase");
        _database.Create(new Fund
        {
            Id = _fundGuid,
            Name = "Test Fund",
            GrowthRate = 0.0M,
            Charge = 0.0M
        });
    }

    // [Test]
    // public void ReadFund()
    // {
    //     const string propertyName = "Id";
    //     const string modelTypeName = "Fund";
    //
    //     var actual = _database.Read(propertyName, _fundGuid, modelTypeName);
    //     
    //     Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
    // }
}
