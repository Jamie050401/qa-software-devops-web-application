﻿namespace Application.Data;

using Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using Models;
using System.Diagnostics;

public class Database
{
    public Database()
    {
        Directory.CreateDirectory("Data");
        ConnectionString = "Data Source=Data/database.sqlite;";
        this.CreateTables();
    }

    public Response<Fund, Error> GetFundFromDatabase(int fundId)
    {
        var fundInCache = (Fund?)this.GetFromCache($"Fund{fundId}");
        if (fundInCache is not null)
        {
            return Response<Fund, Error>.OkValueResponse(fundInCache);
        }

        var sqlGetFromDatabase = $"SELECT * FROM Funds WHERE Id = {fundId};";
        var dbResponse = this.ExecuteReader(sqlGetFromDatabase);

        if (!dbResponse.Reader.Read())
        {
            return Response<Fund, Error>.NotFoundResponse();
        }

        var id = dbResponse.Reader.GetInt32(0);
        var name = dbResponse.Reader.GetString(1);
        var growthRate = dbResponse.Reader.GetDecimal(2);
        var charge = dbResponse.Reader.GetDecimal(3);
        dbResponse.Dispose();
        var fundInDb = new Fund(id, name, growthRate, charge);
        this.AddToCache($"Fund{fundInDb.Id}", fundInDb);

        return Response<Fund, Error>.OkValueResponse(fundInDb);
    }

    public void DeleteFundFromDatabase(int fundId)
    {
        var response = this.GetFundFromDatabase(fundId);

        if (response.Status == ResponseStatus.Error) return;

        var sqlDeleteFromDatabase = $"DELETE FROM Funds WHERE Id = {fundId};";
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
                SET Name = "{fund.Id}",
                    GrowthRate = {fund.GrowthRate},
                    Charge = {fund.Charge}
                WHERE Id = {fund.Id};
            """;
        }
        else
        {
            sqlAddToDatabase = $"""
                INSERT INTO Funds (Id, Name, GrowthRate, Charge)
                VALUES ({fund.Id}, "{fund.Name}", {fund.GrowthRate}, {fund.Charge});
            """;
        }
        var affected = this.ExecuteNonQuery(sqlAddToDatabase);
        if (affected <= 0) return;
        this.AddToCache($"Fund{fund.Id}", fund);
    }

    public void AddFundsToDatabase(IEnumerable<Fund> funds)
    {
        foreach (var fund in funds)
        {
            this.AddFundToDatabase(fund);
        }
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
        var roleInDb = new Role(name);
        this.AddToCache($"Role{roleInDb.Name}", roleInDb);

        return Response<Role, Error>.OkValueResponse(roleInDb);
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

    public void AddRolesToDatabase(IEnumerable<Role> roles)
    {
        foreach (var role in roles)
        {
            this.AddRoleToDatabase(role);
        }
    }

    public Response<User, Error> GetUserFromDatabase(int? userId = null, string? userName = null)
    {
        if (userId is null && userName is null)
        {
            return Response<User, Error>.BadRequestResponse();
        }

        var userInCache = (User?)this.GetFromCache($"User{userId}") ?? (User?)this.GetFromCache($"User{userName}");
        if (userInCache is not null)
        {
            return Response<User, Error>.OkValueResponse(userInCache);
        }

        var sqlGetFromDatabase = userId is null ? $"SELECT * FROM Users WHERE Username = \"{userName}\";" : $"SELECT * FROM Users WHERE Id = {userId};";
        var dbResponse = this.ExecuteReader(sqlGetFromDatabase);

        if (!dbResponse.Reader.Read())
        {
            return Response<User, Error>.NotFoundResponse();
        }

        var id = dbResponse.Reader.GetInt32(0);
        var username = dbResponse.Reader.GetString(1);
        var password = dbResponse.Reader.GetString(2);
        var firstName = dbResponse.Reader.IsDBNull(3) ? null : dbResponse.Reader.GetString(3);
        var lastName = dbResponse.Reader.IsDBNull(4) ? null : dbResponse.Reader.GetString(4);
        var roleName = dbResponse.Reader.GetString(5);
        dbResponse.Dispose();
        var userInDb = new User(id, username, password, roleName, firstName, lastName);
        this.AddToCache($"User{userInDb.Id}", userInDb);
        this.AddToCache($"User{userInDb.Username}", userInDb);
        return Response<User, Error>.OkValueResponse(userInDb);
    }

    public void DeleteUserFromDatabase(int? userId = null, string? userName = null)
    {
        if (userId is null && userName is null) return;

        var userInDb = this.GetUserFromDatabase(userId, userName);

        if (userInDb.Status == ResponseStatus.Error) return;

        var sqlDeleteFromDatabase = userId is null ? $"DELETE FROM Users WHERE Username = \"{userName}\";" : $"DELETE FROM Users WHERE Id = {userId};";
        var affected = this.ExecuteNonQuery(sqlDeleteFromDatabase);
        if (affected <= 0) return;
        Debug.Assert(userInDb.Value != null, "fundInDb.Value != null");
        this.DeleteFromCache($"User{userInDb.Value.Id}");
        this.DeleteFromCache($"User{userInDb.Value.Username}");
    }

    public void AddUserToDatabase(User user)
    {
        string sqlAddToDatabase;
        var response = this.GetUserFromDatabase(user.Id);
        if (response.Status is ResponseStatus.Success && response.HasValue)
        {
            sqlAddToDatabase = $"""
                UPDATE Users
                SET Username = "{user.Username}",
                    Password = "{user.Password}",
                    FirstName = "{user.FirstName}",
                    LastName = "{user.LastName}",
                    RoleName = "{user.RoleName}"
                WHERE Id = {user.Id};
            """;
        }
        else
        {
            sqlAddToDatabase = $"""
                INSERT INTO Users (Id, Username, Password, FirstName, LastName, RoleName)
                VALUES ({user.Id}, "{user.Username}", "{user.Password}", "{user.FirstName}", "{user.LastName}", "{user.RoleName}");
            """;
        }
        var affected = this.ExecuteNonQuery(sqlAddToDatabase);
        if (affected <= 0) return;
        this.AddToCache($"User{user.Id}", user);
        this.AddToCache($"User{user.Username}", user);
    }

    public void AddUsersToDatabase(IEnumerable<User> users)
    {
        foreach (var user in users)
        {
            this.AddUserToDatabase(user);
        }
    }

    public Response<Result, Error> GetResultFromDatabase(int resultId)
    {
        var resultInCache = (Result?)this.GetFromCache($"Result{resultId}");
        if (resultInCache is not null)
        {
            return Response<Result, Error>.OkValueResponse(resultInCache);
        }

        var sqlGetFromDatabase = $"SELECT * FROM Results WHERE Id = {resultId};";
        var dbResponse = this.ExecuteReader(sqlGetFromDatabase);

        if (!dbResponse.Reader.Read())
        {
            return Response<Result, Error>.NotFoundResponse();
        }

        var id = dbResponse.Reader.GetInt32(0);
        var userId = dbResponse.Reader.GetInt32(1);
        var totalInvestment = dbResponse.Reader.GetDecimal(2);
        var projectedValue = dbResponse.Reader.GetDecimal(3);
        dbResponse.Dispose();
        var resultInDb = new Result(id, userId, totalInvestment, projectedValue);
        this.AddToCache($"Result{resultInDb.Id}", resultInDb);
        return Response<Result, Error>.OkValueResponse(resultInDb);
    }

    public void DeleteResultFromDatabase(int resultId)
    {
        var resultInDb = this.GetResultFromDatabase(resultId);

        if (resultInDb.Status == ResponseStatus.Error) return;

        var sqlDeleteFromDatabase = $"DELETE FROM Results WHERE Id = {resultId};";
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
                SET UserId = {result.UserId},
                    TotalInvestment = {result.TotalInvestment},
                    ProjectedValue = {result.ProjectedValue}
                WHERE Id = {result.Id};
            """;
        }
        else
        {
            sqlAddToDatabase = $"""
                INSERT INTO Results (Id, UserId, TotalInvestment, ProjectedValue)
                VALUES ({result.Id}, {result.UserId}, {result.TotalInvestment}, {result.ProjectedValue});
            """;
        }
        var affected = this.ExecuteNonQuery(sqlAddToDatabase);
        if (affected <= 0) return;
        this.AddToCache($"Result{result.Id}", result);
    }

    public void AddResultsToDatabase(IEnumerable<Result> results)
    {
        foreach (var result in results)
        {
            this.AddResultToDatabase(result);
        }
    }

    private void CreateTables()
    {
        const string sqlCreateTables = """
            CREATE TABLE IF NOT EXISTS Funds (
                Id INTEGER PRIMARY KEY NOT NULL,
                Name TEXT NOT NULL,
                GrowthRate REAL NOT NULL,
                Charge REAL NOT NULL
            );
           
            CREATE TABLE IF NOT EXISTS Roles (
                Name TEXT PRIMARY KEY NOT NULL
            );
           
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY NOT NULL,
                Username TEXT NOT NULL,
                Password TEXT NOT NULL,
                FirstName TEXT,
                LastName TEXT,
                RoleName TEXT NOT NULL REFERENCES Roles(Name) ON DELETE NO ACTION
            );

            CREATE UNIQUE INDEX IF NOT EXISTS idx_Username ON Users(Username);
            
            CREATE TABLE IF NOT EXISTS Results (
                Id INTEGER PRIMARY KEY NOT NULL,
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
        SlidingExpiration = TimeSpan.FromHours(1)
    };
}
