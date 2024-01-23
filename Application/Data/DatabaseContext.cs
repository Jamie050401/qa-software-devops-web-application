namespace Application.Data;

using Models;
using Microsoft.EntityFrameworkCore;
using System.Data.SQLite;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        base.Database.EnsureCreated();
    }

    public required DbSet<Fund> Funds { get; set; }
    public required DbSet<Role> Roles { get; set; }
    public required DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        CreateTables(this.Database.GetConnectionString());

        modelBuilder.Entity<Fund>().ToTable("Funds");
        modelBuilder.Entity<Role>().ToTable("Roles");
        modelBuilder.Entity<User>().ToTable("Users");

        base.OnModelCreating(modelBuilder);
    }

    private static void CreateTables(string? connectionString)
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

        using var dbConnection = new SQLiteConnection(connectionString); dbConnection.Open();
        using var dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = sqlCreateTables;
        dbCommand.ExecuteNonQuery();
    }
}