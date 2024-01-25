namespace Application.Data;

using CommandLine;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Data.SQLite;

public class Database
{
    public Database(IConfiguration configuration)
    {
        Configuration = configuration;
        ConnectionString = Configuration["ConnectionStrings:Sqlite"];
        this.CreateTables();
    }

    private IConfiguration Configuration { get; }
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
        var sqlGetFromDatabase = $"SELECT * FROM Funds WHERE Id = {fundId}";
        var dbResponse = this.ExecuteReader(sqlGetFromDatabase);

        if (!dbResponse.Reader.HasRows)
        {
            return new NotFoundResult();
        }

        var id = dbResponse.Reader["Id"].Cast<int>();
        var name = dbResponse.Reader["Name"].Cast<string>();
        var growthRate = dbResponse.Reader["GrowthRate"].Cast<decimal>();
        var charge = dbResponse.Reader["Charge"].Cast<decimal>();
        dbResponse.Dispose();
        var fund = new Fund(id, name, growthRate, charge);
        return fund;
    }

    // TODO - Implement local caching
    public ActionResult<Role> GetRoleFromDatabase(string roleName)
    {
        var sqlGetFromDatabase = $"SELECT * FROM Roles WHERE Name = {roleName}";
        var dbResponse = this.ExecuteReader(sqlGetFromDatabase);

        if (!dbResponse.Reader.HasRows)
        {
            return new NotFoundResult();
        }

        var name = dbResponse.Reader["Name"].Cast<string>();
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

        var sqlGetFromDatabase = userId is null ? $"SELECT * FROM Users WHERE Id = {userName}" : $"SELECT * FROM Users WHERE Id = {userId}";
        var dbResponse = this.ExecuteReader(sqlGetFromDatabase);

        if (!dbResponse.Reader.HasRows)
        {
            return new NotFoundResult();
        }

        var id = dbResponse.Reader["Id"].Cast<int>();
        var username = dbResponse.Reader["Username"].Cast<string>();
        var password = dbResponse.Reader["Password"].Cast<string>();
        var firstName = dbResponse.Reader["FirstName"].Cast<string?>();
        var lastName = dbResponse.Reader["LastName"].Cast<string?>();
        var roleName = dbResponse.Reader["Role"].Cast<string>();
        dbResponse.Dispose();
        var user = new User(id, username, password, firstName, lastName, roleName);
        return user;
    }

    private void ExecuteNonQuery(string sqlCommand)
    {
        using var dbConnection = new SQLiteConnection(ConnectionString);
        dbConnection.Open();
        using var dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = sqlCommand;
        dbCommand.ExecuteNonQuery();
    }

    private ExecuteReaderResponse ExecuteReader(string sqlCommand)
    {
        var dbConnection = new SQLiteConnection(ConnectionString);
        dbConnection.Open();
        var dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = sqlCommand;
        return new ExecuteReaderResponse(dbConnection, dbCommand, dbCommand.ExecuteReader());
    }

    private readonly struct ExecuteReaderResponse(SQLiteConnection connection, SQLiteCommand command, SQLiteDataReader reader)
    {
        public SQLiteConnection Connection { get; } = connection;
        public SQLiteCommand Command { get; } = command;
        public SQLiteDataReader Reader { get; } = reader;

        public void Dispose()
        {
            Reader.Close(); Command.Dispose(); Connection.Close();
        }
    }
}
