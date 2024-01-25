namespace Application.Data;

using CommandLine;
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
                                              Role TEXT NOT NULL,
                                       
                                              FOREIGN KEY(Role) REFERENCES Roles(Name) ON DELETE NO ACTION
                                          );
                                       """;

        this.ExecuteNonQuery(sqlCreateTables);
    }

    // TODO - Implement local caching
    public Fund GetFundFromDatabase(int fundId)
    {
        var sqlGetFromDatabase = $"SELECT * FROM Funds WHERE Id = {fundId}";
        using var reader = this.ExecuteReader(sqlGetFromDatabase);

        if (!reader.HasRows)
        {
            // Need to return nothing if no entries found
        }

        var fund = new Fund
        {
            Id = reader["Id"].Cast<int>(),
            Name = reader["Name"].Cast<string>(),
            GrowthRate = reader["GrowthRate"].Cast<decimal>(),
            Charge = reader["Charge"].Cast<decimal>()
        };
        return fund;
    }

    // TODO - Implement local caching
    public Role GetRoleFromDatabase(string roleName)
    {
        var sqlGetFromDatabase = $"SELECT * FROM Roles WHERE Name = {roleName}";
        using var reader = this.ExecuteReader(sqlGetFromDatabase);

        if (!reader.HasRows)
        {
            // Need to return nothing if no entries found
        }

        var role = new Role
        {
            Name = reader["Name"].Cast<string>()
        };
        return role;
    }

    // TODO - Implement local caching
    public User GetUserFromDatabase(string? userId = null, string? userName = null)
    {
        var sqlGetFromDatabase = userId is null ? $"SELECT * FROM Users WHERE Id = {userName}" : $"SELECT * FROM Users WHERE Id = {userId}";
        using var reader = this.ExecuteReader(sqlGetFromDatabase);

        if (!reader.HasRows)
        {
            // Need to return nothing if no entries found
        }

        var user = new User
        {
            Id = reader["Id"].Cast<int>(),
            Username = reader["Username"].Cast<string>(),
            Password = reader["Password"].Cast<string>(),
            FirstName = reader["FirstName"].Cast<string>(),
            LastName = reader["LastName"].Cast<string>(),
            Role = reader["Role"].Cast<Role>()
        };
        return user;
    }

    private SQLiteDataReader ExecuteReader(string sqlCommand)
    {
        using var dbConnection = new SQLiteConnection(ConnectionString);
        dbConnection.Open();
        using var dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = sqlCommand;
        return dbCommand.ExecuteReader();
    }

    private void ExecuteNonQuery(string sqlCommand)
    {
        using var dbConnection = new SQLiteConnection(ConnectionString);
        dbConnection.Open();
        using var dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = sqlCommand;
        dbCommand.ExecuteNonQuery();
    }
}
