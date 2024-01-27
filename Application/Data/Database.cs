namespace Application.Data;

using Microsoft.AspNetCore.Mvc;
using Models;
using System.Data.SQLite;

public class Database
{
    public Database()
    {
        ConnectionString = "Data Source=Data/database.sqlite;Version=3;";
        this.CreateTables();
    }

    public Database(string? connectionString)
    {
        ConnectionString = connectionString;
        this.CreateTables();
    }

    private string? ConnectionString { get; }

    public ActionResult<Fund> GetFundFromDatabase(int fundId)
    {
        var fundInCache = (Fund?)DatabaseManager.GetFromCache(fundId);
        if (fundInCache is not null)
        {
            return new OkObjectResult(fundInCache);
        }

        var sqlGetFromDatabase = $"SELECT * FROM Funds WHERE Id = {fundId};";
        var dbResponse = this.ExecuteReader(sqlGetFromDatabase);

        if (!dbResponse.Reader.Read())
        {
            return new NotFoundResult();
        }

        var id = dbResponse.Reader.GetInt32(0);
        var name = dbResponse.Reader.GetString(1);
        var growthRate = dbResponse.Reader.GetDecimal(2);
        var charge = dbResponse.Reader.GetDecimal(3);
        dbResponse.Dispose();
        var fund = new Fund(id, name, growthRate, charge);
        DatabaseManager.AddToCache(fund.Id, fund);
        return new OkObjectResult(fund);
    }

    public void DeleteFundFromDatabase(int fundId)
    {
        var fundInDb = this.GetFundFromDatabase(fundId);

        if (fundInDb.Result == new NotFoundResult()) return;

        var sqlDeleteFromDatabase = $"DELETE FROM Funds WHERE Id = {fundId};";
        var affected = this.ExecuteNonQuery(sqlDeleteFromDatabase);
        if (affected > 0)
        {
            DatabaseManager.DeleteFromCache(fundId);
        }
    }

    public void AddFundToDatabase(Fund fund)
    {
        var fundInDb = this.GetFundFromDatabase(fund.Id);

        if (fundInDb.Result != new NotFoundResult())
        {
            this.DeleteFundFromDatabase(fund.Id);
        }

        var sqlAddToDatabase = $"""
                                INSERT INTO Funds (Id, Name, GrowthRate, Charge)
                                VALUES ({fund.Id}, "{fund.Name}", {fund.GrowthRate}, {fund.Charge});
                                """;
        var affected = this.ExecuteNonQuery(sqlAddToDatabase);
        if (affected <= 0) return;
        DatabaseManager.AddToCache(fund.Id, fund);
    }

    public void AddFundsToDatabase(IEnumerable<Fund> funds)
    {
        foreach (var fund in funds)
        {
            this.AddFundToDatabase(fund);
        }
    }

    public ActionResult<Role> GetRoleFromDatabase(string roleName)
    {
        var roleInCache = (Role?)DatabaseManager.GetFromCache(roleName);
        if (roleInCache is not null)
        {
            return new OkObjectResult(roleInCache);
        }

        var sqlGetFromDatabase = $"SELECT * FROM Roles WHERE Name = {roleName};";
        var dbResponse = this.ExecuteReader(sqlGetFromDatabase);

        if (!dbResponse.Reader.Read())
        {
            return new NotFoundResult();
        }

        var name = dbResponse.Reader.GetString(0);
        dbResponse.Dispose();
        var role = new Role(name);
        DatabaseManager.AddToCache(role.Name, role);
        return new OkObjectResult(role);
    }

    public void DeleteRoleFromDatabase(string roleName)
    {
        var fundInDb = this.GetRoleFromDatabase(roleName);

        if (fundInDb.Result == new NotFoundResult()) return;

        var sqlDeleteFromDatabase = $"DELETE FROM Roles WHERE Name = {roleName};";
        var affected = this.ExecuteNonQuery(sqlDeleteFromDatabase);
        if (affected > 0)
        {
            DatabaseManager.DeleteFromCache(roleName);
        }
    }

    public void AddRoleToDatabase(Role role)
    {
        var fundInDb = this.GetRoleFromDatabase(role.Name);

        if (fundInDb.Result != new NotFoundResult())
        {
            this.DeleteRoleFromDatabase(role.Name);
        }

        var sqlAddToDatabase = $"""
                                INSERT INTO Roles (Name)
                                VALUES ({role.Name});
                                """;
        var affected = this.ExecuteNonQuery(sqlAddToDatabase);
        if (affected <= 0) return;
        DatabaseManager.AddToCache(role.Name, role);
    }

    public void AddRolesToDatabase(IEnumerable<Role> roles)
    {
        foreach (var role in roles)
        {
            this.AddRoleToDatabase(role);
        }
    }

    public ActionResult<User> GetUserFromDatabase(int? userId = null, string? userName = null)
    {
        if (userId is null && userName is null)
        {
            return new StatusCodeResult(400);
        }

        var userInCache = (User?)DatabaseManager.GetFromCache(userId) ?? (User?)DatabaseManager.GetFromCache(userName);
        if (userInCache is not null)
        {
            return new OkObjectResult(userInCache);
        }

        var sqlGetFromDatabase = userId is null ? $"SELECT * FROM Users WHERE Username = {userName};" : $"SELECT * FROM Users WHERE Id = {userId};";
        var dbResponse = this.ExecuteReader(sqlGetFromDatabase);

        if (!dbResponse.Reader.Read())
        {
            return new NotFoundResult();
        }

        var id = dbResponse.Reader.GetInt32(0);
        var username = dbResponse.Reader.GetString(1);
        var password = dbResponse.Reader.GetString(2);
        var firstName = dbResponse.Reader.IsDBNull(3) ? null : dbResponse.Reader.GetString(3);
        var lastName = dbResponse.Reader.IsDBNull(4) ? null : dbResponse.Reader.GetString(4);
        var roleName = dbResponse.Reader.GetString(5);
        dbResponse.Dispose();
        var user = new User(id, username, password, firstName, lastName, roleName);
        DatabaseManager.AddToCache(user.Id, user);
        DatabaseManager.AddToCache(user.Username, user);
        return new OkObjectResult(user);
    }

    public void DeleteUserFromDatabase(int? userId = null, string? userName = null)
    {
        if (userId is null && userName is null) return;

        var fundInDb = this.GetUserFromDatabase(userId, userName);

        if (fundInDb.Result == new NotFoundResult()) return;

        var sqlDeleteFromDatabase = userId is null ? $"DELETE FROM Users WHERE Username = {userName};" : $"DELETE FROM Users WHERE Id = {userId};";
        var affected = this.ExecuteNonQuery(sqlDeleteFromDatabase);
        if (affected <= 0) return;
        DatabaseManager.DeleteFromCache(userId);
        DatabaseManager.DeleteFromCache(userName);
    }

    public void AddUserToDatabase(User user)
    {
        var fundInDb = this.GetUserFromDatabase(user.Id);

        if (fundInDb.Result != new NotFoundResult())
        {
            this.DeleteUserFromDatabase(user.Id);
        }

        var sqlAddToDatabase = $"""
                                INSERT INTO Users (Id, Username, Password, FirstName, LastName, RoleName)
                                VALUES ({user.Id}, {user.Username}, {user.Password}, {user.FirstName}, {user.LastName}, {user.RoleName});
                                """;
        var affected = this.ExecuteNonQuery(sqlAddToDatabase);
        if (affected <= 0) return;
        DatabaseManager.AddToCache(user.Id, user);
        DatabaseManager.AddToCache(user.Username, user);
    }

    public void AddUsersToDatabase(IEnumerable<User> users)
    {
        foreach (var user in users)
        {
            this.AddUserToDatabase(user);
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
                                              RoleName TEXT NOT NULL,
                                       
                                              FOREIGN KEY(RoleName) REFERENCES Roles(Name) ON DELETE NO ACTION
                                          );
                                          
                                          CREATE UNIQUE INDEX IF NOT EXISTS idx_Username ON Users(Username);
                                       """;

        this.ExecuteNonQuery(sqlCreateTables);
    }

    private int ExecuteNonQuery(string sqlCommand)
    {
        using var dbConnection = new SQLiteConnection(ConnectionString); dbConnection.Open();
        using var dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = sqlCommand;
        return dbCommand.ExecuteNonQuery();
    }

    private ExecuteReaderResponse ExecuteReader(string sqlCommand)
    {
        var dbConnection = new SQLiteConnection(ConnectionString); dbConnection.Open();
        var dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = sqlCommand;
        return new ExecuteReaderResponse(dbConnection, dbCommand, dbCommand.ExecuteReader());
    }

    private readonly struct ExecuteReaderResponse(SQLiteConnection connection, SQLiteCommand command, SQLiteDataReader reader)
    {
        public SQLiteDataReader Reader { get; } = reader;
        private SQLiteConnection Connection { get; } = connection;
        private SQLiteCommand Command { get; } = command;

        public void Dispose()
        {
            Reader.Close(); Command.Dispose(); Connection.Close();
        }
    }
}
