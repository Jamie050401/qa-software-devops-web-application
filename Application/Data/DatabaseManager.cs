namespace Application.Data;

using Common;
using Models;

public static class DatabaseManager
{
    public static void InitialiseDatabase()
    {
        var dbResponse = Database.Read("Name", "Default", Model.Role);
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
            Database.Create(new Role
            {
                Id = Guid.NewGuid(),
                Name = "Default"
            });

        dbResponse = Database.Read("Name", "Administrator", Model.Role);
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
            Database.Create(new Role
            {
                Id = Guid.NewGuid(),
                Name = "Administrator"
            });

        // TODO - Populate database with initial funds
    }

    public static Database Database { get; } = new();
}
