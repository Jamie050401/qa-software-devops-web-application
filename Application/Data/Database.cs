namespace Application.Data;

using Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using Models;
using Newtonsoft.Json;
using System.Diagnostics;

public class Database
{
    public Database()
    {
        Directory.CreateDirectory("Data");
        ConnectionString = "Data Source=Data/database.sqlite;";
        this.CreateTables();
    }

    public Database(string path)
    {
        Directory.CreateDirectory(path);
        ConnectionString = $"Data Source={path}/database.sqlite";
        this.CreateTables();
    }

    public Response<TValue, Error> Create<TValue>(TValue value)
    {
        if (value is null)
        {
            return Response<TValue, Error>.BadRequestResponse();
        }

        // Capture type, properties and primary key of model
        var valueType = value.GetType();
        var properties = valueType.GetProperties();
        var id =
            properties.FirstOrDefault(propertyInfo => propertyInfo.Name == "Id")
            ?? properties.First(propertyInfo => propertyInfo.Name == "Name");

        // Derive table name, headers and values based on model type
        var tableName = $"{valueType.Name}s";
        var columnHeaders = properties.Aggregate("", (columnHeaders, propertyInfo) =>
            columnHeaders == "" ? $"{propertyInfo.Name}" : $"{columnHeaders}, {propertyInfo.Name}");
        var columnValues = properties.Aggregate("", (columnValues, propertyInfo) =>
        {
            var propertyValue = propertyInfo.GetValue(value);
            switch (propertyValue)
            {
                case null:
                    propertyValue = "NULL";
                    break;
                case string:
                case Guid:
                    propertyValue = $"\"{propertyValue}\"";
                    break;
            }

            return columnValues == ""
                ? $"{propertyValue}"
                : $"{columnValues}, {propertyValue}";
        });

        var sqlCreate = $"""
                INSERT INTO {tableName} ({columnHeaders})
                VALUES ({columnValues})
            """;

        var affected = this.ExecuteNonQuery(sqlCreate);
        if (affected <= 0) return Response<TValue, Error>.BadRequestResponse();
        this.AddToCache($"{id.GetValue(value)}", value);
        return Response<TValue, Error>.OkResponse();
    }

    public Response<Fund, Error> GetFundFromDatabase(Guid fundId)
    {
        var fundInCache = (Fund?)this.GetFromCache($"Fund{fundId}");
        if (fundInCache is not null)
        {
            return Response<Fund, Error>.OkValueResponse(fundInCache);
        }

        var sqlGetFromDatabase = $"SELECT * FROM Funds WHERE Id = \"{fundId}\";";
        var dbResponse = this.ExecuteReader(sqlGetFromDatabase);

        if (!dbResponse.Reader.Read())
        {
            return Response<Fund, Error>.NotFoundResponse();
        }

        var id = dbResponse.Reader.GetGuid(0);
        var name = dbResponse.Reader.GetString(1);
        var growthRate = dbResponse.Reader.GetDecimal(2);
        var charge = dbResponse.Reader.GetDecimal(3);
        dbResponse.Dispose();
        var fundInDb = new Fund
        {
            Id = id,
            Name = name,
            GrowthRate = growthRate,
            Charge = charge
        };
        this.AddToCache($"Fund{fundInDb.Id}", fundInDb);

        return Response<Fund, Error>.OkValueResponse(fundInDb);
    }

    public bool FundExistsInDatabase(Guid fundId)
    {
        var dbResponse = this.GetFundFromDatabase(fundId);
        return dbResponse.Status is ResponseStatus.Success && dbResponse.HasValue;
    }

    public void DeleteFundFromDatabase(Guid fundId)
    {
        var response = this.GetFundFromDatabase(fundId);

        if (response.Status == ResponseStatus.Error) return;

        var sqlDeleteFromDatabase = $"DELETE FROM Funds WHERE Id = \"{fundId}\";";
        var affected = this.ExecuteNonQuery(sqlDeleteFromDatabase);
        if (affected > 0)
        {
            this.DeleteFromCache($"Fund{fundId}");
        }
    }

    public void AddFundToDatabase(Fund fund)
    {
        string sqlAddToDatabase;
        var response = this.GetFundFromDatabase(fund.Id);
        if (response.Status is ResponseStatus.Success && response.HasValue)
        {
            sqlAddToDatabase = $"""
                UPDATE Funds
                SET Name = "{fund.Id.ToString()}",
                    GrowthRate = {fund.GrowthRate},
                    Charge = {fund.Charge}
                WHERE Id = "{fund.Id.ToString()}";
            """;
        }
        else
        {
            sqlAddToDatabase = $"""
                INSERT INTO Funds (Id, Name, GrowthRate, Charge)
                VALUES ("{fund.Id.ToString()}", "{fund.Name}", {fund.GrowthRate}, {fund.Charge});
            """;
        }
        var affected = this.ExecuteNonQuery(sqlAddToDatabase);
        if (affected <= 0) return;
        this.AddToCache($"Fund{fund.Id}", fund);
    }

    public Response<Role, Error> GetRoleFromDatabase(string roleName)
    {
        var roleInCache = (Role?)this.GetFromCache($"Role{roleName}");
        if (roleInCache is not null)
        {
            return Response<Role, Error>.OkValueResponse(roleInCache);
        }

        var sqlGetFromDatabase = $"SELECT * FROM Roles WHERE Name = \"{roleName}\";";
        var dbResponse = this.ExecuteReader(sqlGetFromDatabase);

        if (!dbResponse.Reader.Read())
        {
            return Response<Role, Error>.NotFoundResponse();
        }

        var name = dbResponse.Reader.GetString(0);
        dbResponse.Dispose();
        var roleInDb = new Role
        {
            Name = name
        };
        this.AddToCache($"Role{roleInDb.Name}", roleInDb);

        return Response<Role, Error>.OkValueResponse(roleInDb);
    }

    public bool RoleExistsInDatabase(string roleName)
    {
        var dbResponse = this.GetRoleFromDatabase(roleName);
        return dbResponse.Status is ResponseStatus.Success && dbResponse.HasValue;
    }

    public void DeleteRoleFromDatabase(string roleName)
    {
        var roleInDb = this.GetRoleFromDatabase(roleName);

        if (roleInDb.Status == ResponseStatus.Error) return;

        var sqlDeleteFromDatabase = $"DELETE FROM Roles WHERE Name = \"{roleName}\";";
        var affected = this.ExecuteNonQuery(sqlDeleteFromDatabase);
        if (affected > 0)
        {
            this.DeleteFromCache($"Role{roleName}");
        }
    }

    public void AddRoleToDatabase(Role role)
    {
        string sqlAddToDatabase;
        var response = this.GetRoleFromDatabase(role.Name);
        if (response.Status is ResponseStatus.Success && response.HasValue)
        {
            sqlAddToDatabase = $"""
                UPDATE Roles
                SET Name = "{role.Name}"
                WHERE Name = "{role.Name}";
            """;
        }
        else
        {
            sqlAddToDatabase = $"""
                INSERT INTO Roles (Name)
                VALUES ("{role.Name}");
            """;
        }
        var affected = this.ExecuteNonQuery(sqlAddToDatabase);
        if (affected <= 0) return;
        this.AddToCache($"Role{role.Name}", role);
    }

    public Response<User, Error> GetUserFromDatabase(Guid? userId = null, string? userEmail = null)
    {
        if (userId is null && userEmail is null)
        {
            return Response<User, Error>.BadRequestResponse();
        }

        var userInCache = (User?)(this.GetFromCache($"User{userId}") ?? this.GetFromCache($"User{userEmail}"));
        if (userInCache is not null)
        {
            return Response<User, Error>.OkValueResponse(userInCache);
        }

        var sqlGetFromDatabase = userId is null ? $"SELECT * FROM Users WHERE Email = \"{userEmail}\";" : $"SELECT * FROM Users WHERE Id = \"{userId}\";";
        var dbResponse = this.ExecuteReader(sqlGetFromDatabase);

        if (!dbResponse.Reader.Read())
        {
            return Response<User, Error>.NotFoundResponse();
        }

        var id = dbResponse.Reader.GetGuid(0);
        var email = dbResponse.Reader.GetString(1);
        var password = dbResponse.Reader.GetString(2);
        var authenticationData = dbResponse.Reader.IsDBNull(3) ? null : JsonConvert.DeserializeObject<AuthenticationData>(dbResponse.Reader.GetString(3));
        var firstName = dbResponse.Reader.IsDBNull(4) ? null : dbResponse.Reader.GetString(4);
        var lastName = dbResponse.Reader.IsDBNull(5) ? null : dbResponse.Reader.GetString(5);
        var roleName = dbResponse.Reader.GetString(6);
        dbResponse.Dispose();
        var userInDb = new User
        {
            Id = id,
            Email = email,
            Password = password,
            AuthenticationData = authenticationData,
            FirstName = firstName,
            LastName = lastName,
            RoleName = roleName
        };
        this.AddToCache($"User{userInDb.Id}", userInDb);
        this.AddToCache($"User{userInDb.Email}", userInDb);

        return Response<User, Error>.OkValueResponse(userInDb);
    }

    public bool UserExistsInDatabase(Guid? userId = null, string? userEmail = null)
    {
        var dbResponse = this.GetUserFromDatabase(userId, userEmail);
        return dbResponse.Status is ResponseStatus.Success && dbResponse.HasValue;
    }

    public void DeleteUserFromDatabase(Guid? userId = null, string? userEmail = null)
    {
        if (userId is null && userEmail is null) return;

        var userInDb = this.GetUserFromDatabase(userId, userEmail);

        if (userInDb.Status == ResponseStatus.Error) return;

        var sqlDeleteFromDatabase = userId is null ? $"DELETE FROM Users WHERE Email = \"{userEmail}\";" : $"DELETE FROM Users WHERE Id = \"{userId}\";";
        var affected = this.ExecuteNonQuery(sqlDeleteFromDatabase);
        if (affected <= 0) return;
        Debug.Assert(userInDb.Value != null, "userInDb.Value != null");
        this.DeleteFromCache($"User{userInDb.Value.Id}");
        this.DeleteFromCache($"User{userInDb.Value.Email}");
    }

    public void AddUserToDatabase(User user)
    {
        string sqlAddToDatabase;
        var response = this.GetUserFromDatabase(user.Id);
        if (response.Status is ResponseStatus.Success && response.HasValue)
        {
            sqlAddToDatabase = $"""
                UPDATE Users
                SET Email = "{user.Email}",
                    Password = "{user.Password}",
                    AuthenticationData = json_set('{JsonConvert.SerializeObject(user.AuthenticationData)}'),
                    FirstName = "{user.FirstName}",
                    LastName = "{user.LastName}",
                    RoleName = "{user.RoleName}"
                WHERE Id = "{user.Id.ToString()}";
            """;
        }
        else
        {
            sqlAddToDatabase = $"""
                INSERT INTO Users (Id, Email, Password, AuthenticationData, FirstName, LastName, RoleName)
                VALUES ("{user.Id.ToString()}", "{user.Email}", "{user.Password}", json_set('{JsonConvert.SerializeObject(user.AuthenticationData)}'), "{user.FirstName}", "{user.LastName}", "{user.RoleName}");
            """;
        }
        var affected = this.ExecuteNonQuery(sqlAddToDatabase);
        if (affected <= 0) return;
        this.AddToCache($"User{user.Id}", user);
        this.AddToCache($"User{user.Email}", user);
    }

    public Response<Result, Error> GetResultFromDatabase(Guid resultId)
    {
        var resultInCache = (Result?)this.GetFromCache($"Result{resultId}");
        if (resultInCache is not null)
        {
            return Response<Result, Error>.OkValueResponse(resultInCache);
        }

        var sqlGetFromDatabase = $"SELECT * FROM Results WHERE Id = \"{resultId}\";";
        var dbResponse = this.ExecuteReader(sqlGetFromDatabase);

        if (!dbResponse.Reader.Read())
        {
            return Response<Result, Error>.NotFoundResponse();
        }

        var id = dbResponse.Reader.GetGuid(0);
        var userId = dbResponse.Reader.GetGuid(1);
        var totalInvestment = dbResponse.Reader.GetDecimal(2);
        var projectedValue = dbResponse.Reader.GetDecimal(3);
        dbResponse.Dispose();
        var resultInDb = new Result
        {
            Id = id,
            UserId = userId,
            TotalInvestment = totalInvestment,
            ProjectedValue = projectedValue
        };
        this.AddToCache($"Result{resultInDb.Id}", resultInDb);

        return Response<Result, Error>.OkValueResponse(resultInDb);
    }

    public bool ResultExistsInDatabase(Guid resultId)
    {
        var dbResponse = this.GetResultFromDatabase(resultId);
        return dbResponse.Status is ResponseStatus.Success && dbResponse.HasValue;
    }

    public void DeleteResultFromDatabase(Guid resultId)
    {
        var resultInDb = this.GetResultFromDatabase(resultId);

        if (resultInDb.Status == ResponseStatus.Error) return;

        var sqlDeleteFromDatabase = $"DELETE FROM Results WHERE Id = \"{resultId}\";";
        var affected = this.ExecuteNonQuery(sqlDeleteFromDatabase);
        if (affected > 0)
        {
            this.DeleteFromCache($"Result{resultId}");
        }
    }

    public void AddResultToDatabase(Result result)
    {
        string sqlAddToDatabase;
        var response = this.GetResultFromDatabase(result.Id);
        if (response.Status is ResponseStatus.Success && response.HasValue)
        {
            sqlAddToDatabase = $"""
                UPDATE Results
                SET UserId = "{result.UserId.ToString()}",
                    TotalInvestment = {result.TotalInvestment},
                    ProjectedValue = {result.ProjectedValue}
                WHERE Id = "{result.Id.ToString()}";
            """;
        }
        else
        {
            sqlAddToDatabase = $"""
                INSERT INTO Results (Id, UserId, TotalInvestment, ProjectedValue)
                VALUES ("{result.Id.ToString()}", {result.UserId.ToString()}, {result.TotalInvestment}, {result.ProjectedValue});
            """;
        }
        var affected = this.ExecuteNonQuery(sqlAddToDatabase);
        if (affected <= 0) return;
        this.AddToCache($"Result{result.Id}", result);
    }

    private void CreateTables()
    {
        const string sqlCreateTables = """
            CREATE TABLE IF NOT EXISTS Funds (
                Id TEXT PRIMARY KEY NOT NULL,
                Name TEXT NOT NULL,
                GrowthRate REAL NOT NULL,
                Charge REAL NOT NULL
            );
           
            CREATE TABLE IF NOT EXISTS Roles (
                Name TEXT PRIMARY KEY NOT NULL
            );
           
            CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY NOT NULL,
                Email TEXT NOT NULL,
                Password TEXT NOT NULL,
                AuthenticationData TEXT,
                FirstName TEXT,
                LastName TEXT,
                RoleName TEXT NOT NULL REFERENCES Roles(Name) ON DELETE NO ACTION
            );

            CREATE UNIQUE INDEX IF NOT EXISTS idx_Email ON Users(Email);
            
            CREATE TABLE IF NOT EXISTS Results (
                Id TEXT PRIMARY KEY NOT NULL,
                UserId INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
                TotalInvestment REAL NOT NULL,
                ProjectedValue REAL NOT NULL
            );
        """;

        this.ExecuteNonQuery(sqlCreateTables);
    }

    private int ExecuteNonQuery(string sqlCommand)
    {
        using var dbConnection = new SqliteConnection(ConnectionString); dbConnection.Open();
        using var dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = sqlCommand;
        return dbCommand.ExecuteNonQuery();
    }

    private ExecuteReaderResponse ExecuteReader(string sqlCommand)
    {
        var dbConnection = new SqliteConnection(ConnectionString); dbConnection.Open();
        var dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = sqlCommand;
        return new ExecuteReaderResponse(dbConnection, dbCommand, dbCommand.ExecuteReader());
    }

    private readonly struct ExecuteReaderResponse(SqliteConnection connection, SqliteCommand command, SqliteDataReader reader)
    {
        public SqliteDataReader Reader { get; } = reader;
        private SqliteConnection Connection { get; } = connection;
        private SqliteCommand Command { get; } = command;

        public void Dispose()
        {
            Reader.Close(); Command.Dispose(); Connection.Close();
        }
    }

    private void DeleteFromCache(object? key)
    {
        if (key is not null) Cache.Remove(key);
    }

    private void AddToCache(object key, object value)
    {
        Cache.Set(key, value, CacheEntryOptions);
    }

    private object? GetFromCache(object? key)
    {
        if (key is null) return null;

        Cache.TryGetValue(key, out var value);
        return value;
    }

    private string? ConnectionString { get; }
    private MemoryCache Cache { get; } = new(new MemoryCacheOptions());
    private MemoryCacheEntryOptions CacheEntryOptions { get; } = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(30)
    };
}
