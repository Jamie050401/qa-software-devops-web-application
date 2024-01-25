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

    // TODO - Implement local caching
    public ActionResult<Fund> GetFundFromDatabase(int fundId)
    {
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
        return fund;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public int AddFundToDatabase(Fund fund)
    {
        var fundInDb = this.GetFundFromDatabase(fund.Id);

        if (fundInDb.Result != new NotFoundResult())
        {
            // TODO - Implement logic to delete and add new item in database
            return 0;
        }

        var sqlAddToDatabase = $"""
                                INSERT INTO Funds (Id, Name, GrowthRate, Charge)
                                VALUES ({fund.Id}, "{fund.Name}", {fund.GrowthRate}, {fund.Charge});
                                """;
        return this.ExecuteNonQuery(sqlAddToDatabase);
    }

    public int AddFundsToDatabase(IEnumerable<Fund> funds)
    {
        return funds.Sum(this.AddFundToDatabase);
    }

    // TODO - Implement local caching
    public ActionResult<Role> GetRoleFromDatabase(string roleName)
    {
        var sqlGetFromDatabase = $"SELECT * FROM Roles WHERE Name = {roleName};";
        var dbResponse = this.ExecuteReader(sqlGetFromDatabase);

        if (!dbResponse.Reader.Read())
        {
            return new NotFoundResult();
        }

        var name = dbResponse.Reader.GetString(0);
        dbResponse.Dispose();
        var role = new Role(name);
        return role;
    }

    // TODO - Implement local caching
    public ActionResult<User> GetUserFromDatabase(string? userId = null, string? userName = null)
    {
        if (userId is null && userName is null)
        {
            return new StatusCodeResult(400);
        }

        var sqlGetFromDatabase = userId is null ? $"SELECT * FROM Users WHERE Id = {userName};"
                                                     : $"SELECT * FROM Users WHERE Id = {userId};";
        var dbResponse = this.ExecuteReader(sqlGetFromDatabase);

        if (!dbResponse.Reader.Read())
        {
            return new NotFoundResult();
        }

        var id = dbResponse.Reader.GetInt32(0);
        var username = (string)dbResponse.Reader.GetString(1);
        var password = (string)dbResponse.Reader.GetString(2);
        var firstName = dbResponse.Reader.IsDBNull(3) ? null : dbResponse.Reader.GetString(3);
        var lastName = dbResponse.Reader.IsDBNull(4) ? null : dbResponse.Reader.GetString(4);
        var roleName = dbResponse.Reader.GetString(5);
        dbResponse.Dispose();
        var user = new User(id, username, password, firstName, lastName, roleName);
        return user;
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
