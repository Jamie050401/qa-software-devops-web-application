namespace Application.Data;

using Models;

public static class DatabaseManager
{
    public static void InitialiseDatabase()
    {
        if (Database.RoleExistsInDatabase("Default") ||
            Database.RoleExistsInDatabase("Administrator")) return;

        Database.AddRoleToDatabase(new Role
        {
            Name = "Default"
        });
        Database.AddRoleToDatabase(new Role
        {
            Name = "Administrator"
        });

        // TODO - Populate database with initial funds
    }

    public static Database Database { get; } = new();
}
