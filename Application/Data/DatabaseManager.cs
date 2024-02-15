namespace Application.Data;

using Models;

public static class DatabaseManager
{
    public static void InitialiseDatabase()
    {
        Database.Create(new Role
        {
            Name = "Default"
        });
        Database.Create(new Role
        {
            Name = "Administrator"
        });

        // TODO - Populate database with initial funds
    }

    public static Database Database { get; } = new();
}
