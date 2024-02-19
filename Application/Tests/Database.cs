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
    private readonly Guid _roleGuid = Guid.NewGuid();

    public DatabaseCreate()
    {
        Directory.CreateDirectory("Tests");
        File.Delete("Tests/createDatabase.sqlite");
        _database = new DatabaseLogic("Tests", "createDatabase");
    }

    [SetUp]
    public void DatabaseCreateSetUp()
    {
        var roleResponse = _database.Read(Role.GetProperty("Name"), "Test Role");
        if (roleResponse.Status is ResponseStatus.Error || !roleResponse.HasValue)
            _database.Create(new Role
            {
                Id = _roleGuid,
                Name = "Test Role"
            });

        var userResponse = _database.Read(User.GetProperty("Id"), _userGuid);
        if (userResponse.Status is ResponseStatus.Error || !userResponse.HasValue)
            _database.Create(new User
            {
                Id = _userGuid,
                RoleId = _roleGuid,
                Email = "test@one.com",
                Password = "test"
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
            RoleId = _roleGuid,
            Email = "test@two.com",
            Password = "test"
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
            RoleId = _roleGuid,
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
            LastName = "Three"
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
    private readonly Guid _roleGuid = Guid.NewGuid();
    private readonly Guid _userGuid = Guid.NewGuid();
    private readonly Guid _resultGuid = Guid.NewGuid();

    public DatabaseUpdate()
    {
        Directory.CreateDirectory("Tests");
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
            Id = _roleGuid,
            Name = "Test Role"
        });
        _database.Create(new User
        {
            Id = _userGuid,
            RoleId = _roleGuid,
            Email = "test@user.com",
            Password = "TestUser"
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

    [Test]
    public void UpdateRole()
    {
        var role = new Role
        {
            Id = _roleGuid,
            Name = "New Test Role"
        };

        var actual = _database.Update(role);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
    }

    [Test]
    public void UpdateUser()
    {
        var user = new User
        {
            Id = _userGuid,
            RoleId = _roleGuid,
            Email = "test@user.com",
            Password = "TestUser",
            FirstName = "Test",
            LastName = "User"
        };

        var actual = _database.Update(user);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
    }

    [Test]
    public void UpdateResult()
    {
        var result = new Result
        {
            Id = _resultGuid,
            UserId = _userGuid,
            ProjectedValue = 1000.0M,
            TotalInvestment = 1000.0M
        };

        var actual = _database.Update(result);

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
    private readonly Guid _resultGuid = Guid.NewGuid();
    private readonly DateTime _timestamp = DateTime.UtcNow;
    private readonly DateTimeOffset _expires = DateTimeOffset.UtcNow.AddDays(3);

    public DatabaseRead()
    {
        Directory.CreateDirectory("Tests");
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
            Name = "Test Role"
        });
        _database.Create(new User
        {
            Id = _userGuid,
            RoleId = _roleGuid,
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
            LastName = "One"
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
    public void ReadFund()
    {
        var actual = _database.Read(Fund.GetProperty("Id"), _fundGuid);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkValueResponse(new Fund
        {
            Id = _fundGuid,
            Name = "Test Fund",
            GrowthRate = 0.0M,
            Charge = 0.0M
        })));
    }

    [Test]
    public void ReadRole()
    {
        var actual = _database.Read(Role.GetProperty("Name"), "Test Role");

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkValueResponse(new Role
        {
            Id = _roleGuid,
            Name = "Test Role"
        })));
    }

    [Test]
    public void ReadUser()
    {
        var actual = _database.Read(User.GetProperty("Id"), _userGuid);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkValueResponse(new User
        {
            Id = _userGuid,
            RoleId = _roleGuid,
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
            LastName = "One"
        })));
    }

    [Test]
    public void ReadResult()
    {
        var actual = _database.Read(Result.GetProperty("Id"), _resultGuid);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkValueResponse(new Result
        {
            Id = _resultGuid,
            UserId = _userGuid,
            TotalInvestment = 0.0M,
            ProjectedValue = 0.0M
        })));
    }
}

[TestFixture]
public class DatabaseDelete
{
    private readonly DatabaseLogic _database;
    private readonly Guid _fundGuid = Guid.NewGuid();
    private readonly Guid _roleGuid = Guid.NewGuid();
    private readonly Guid _userGuid = Guid.NewGuid();
    private readonly Guid _resultGuid = Guid.NewGuid();

    public DatabaseDelete()
    {
        Directory.CreateDirectory("Tests");
        File.Delete("Tests/deleteDatabase.sqlite");
        _database = new DatabaseLogic("Tests", "deleteDatabase");
    }

    [SetUp]
    public void DatabaseSetup()
    {
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
            Name = "Test Role"
        });
        _database.Create(new User
        {
            Id = _userGuid,
            RoleId = _roleGuid,
            Email = "test@one.com",
            Password = "test",
            FirstName = "Test",
            LastName = "One"
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
    public void DeleteFund()
    {
        var actual = _database.Delete(Fund.GetProperty("Name"), "Test Fund");

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
    }

    [Test]
    public void TryDeleteRole()
    {
        var actual = _database.Delete(Role.GetProperty("Name"), "Test Role");

        Assert.That(actual.Status, Is.EqualTo(ResponseStatus.Error));
        Assert.That(actual.Errors[0].ErrorCode, Is.EqualTo(500));
    }

    [Test]
    public void DeleteRole()
    {
        _database.Delete(User.GetProperty("Id"), _userGuid);
        var actual = _database.Delete(Role.GetProperty("Name"), "Test Role");

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
    }

    [Test]
    public void DeleteUser()
    {
        var actual = _database.Delete(User.GetProperty("Id"), _userGuid);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
    }

    [Test]
    public void DeleteResult()
    {
        var actual = _database.Delete(Result.GetProperty("Id"), _resultGuid);

        Assert.That(actual, Is.EqualTo(Response<IModel, Error>.OkResponse()));
    }
}
