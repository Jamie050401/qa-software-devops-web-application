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

    [SetUp]
    public void DatabaseCreateSetUp()
    {
        var roleResponse = _database.Read("Name", RoleName, "Role");
        if (roleResponse.Status is ResponseStatus.Error || !roleResponse.HasValue)
            _database.Create(new Role
            {
                Id = Guid.NewGuid(),
                Name = RoleName
            });

        var userResponse = _database.Read("Id", _userGuid, "User");
        if (userResponse.Status is ResponseStatus.Error || !userResponse.HasValue)
            _database.Create(new User
            {
                Id = _userGuid,
                Email = "test@one.com",
                Password = "test",
                RoleName = RoleName
            });
    }

    [Test]
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

    [Test]
    public void CreateRole()
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Test Role 2"
        };

        var actual = _database.Create(role);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
    }

    [Test]
    public void CreateUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@two.com",
            Password = "test",
            RoleName = RoleName
        };

        var actual = _database.Create(user);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
    }

    [Test]
    public void CreateAnotherUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@three.com",
            Password = "test",
            AuthenticationData = new AuthenticationData
            {
                Email = "test@three.com",
                Token = "TestToken",
                Source = "127.0.0.1",
                Timestamp = DateTime.UtcNow,
                Expires = DateTimeOffset.UtcNow.AddDays(3)
            },
            FirstName = "Test",
            LastName = "Three",
            RoleName = RoleName
        };

        var actual = _database.Create(user);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
    }

    [Test]
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
    private readonly Guid _roleGuid = Guid.NewGuid();
    private readonly Guid _userGuid = Guid.NewGuid();
    private readonly DateTime _timestamp = DateTime.UtcNow;
    private readonly DateTimeOffset _expires = DateTimeOffset.UtcNow.AddDays(3);
    private const string RoleName = "Test Role";

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
        _database.Create(new Role
        {
            Id = _roleGuid,
            Name = RoleName
        });
        _database.Create(new User
        {
            Id = _userGuid,
            Email = "test@one.com",
            Password = "test",
            AuthenticationData = new AuthenticationData
            {
                Email = "test@one.com",
                Token = "TestToken",
                Source = "127.0.0.1",
                Timestamp = _timestamp,
                Expires = _expires
            },
            FirstName = "Test",
            LastName = "One",
            RoleName = RoleName
        });
    }

    [Test]
    public void ReadFund()
    {
        var actual = _database.Read("Id", _fundGuid, "Fund");

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkValueResponse(new Fund
        {
            Id = _fundGuid,
            Name = "Test Fund",
            GrowthRate = 0.0M,
            Charge = 0.0M
        })));
    }

    [Test]
    public void ReadUser()
    {
        var actual = _database.Read("Id", _userGuid, "User");

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkValueResponse(new User
        {
            Id = _userGuid,
            Email = "test@one.com",
            Password = "test",
            AuthenticationData = new AuthenticationData
            {
                Email = "test@one.com",
                Token = "TestToken",
                Source = "127.0.0.1",
                Timestamp = _timestamp,
                Expires = _expires
            },
            FirstName = "Test",
            LastName = "One",
            RoleName = RoleName
        })));
    }
}
