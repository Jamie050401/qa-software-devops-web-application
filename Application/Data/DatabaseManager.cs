namespace Application.Data;

using Common;
using Models;

public static class DatabaseManager
{
    private const string DefaultRoleName = "Default";
    private const string AdminRoleName = "Administrator";
    private static readonly string[] FundNames = [
        "Generic S2 Fund 01",
        "Generic S2 Fund 02",
        "Generic S2 Fund 03",
        "Generic S2 Fund 04"
    ];

    public static void InitialiseDatabase()
    {
        var dbResponse = Database.Read("Name", DefaultRoleName, Model.Role);
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
            Database.Create(new Role
            {
                Id = Guid.NewGuid(),
                Name = DefaultRoleName
            });

        dbResponse = Database.Read("Name", AdminRoleName, Model.Role);
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
            Database.Create(new Role
            {
                Id = Guid.NewGuid(),
                Name = AdminRoleName
            });

        foreach (var fundName in FundNames)
        {
            dbResponse = Database.Read("Name", fundName, Model.Fund);
            if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
            {
                Database.Create(new Fund
                {
                    Id = Guid.NewGuid(),
                    Name = fundName,
                    GrowthRate = (decimal)Random.Shared.Next(1, 9) / 100,
                    Charge = (decimal)Random.Shared.Next(1, 3) / 100
                });
            }
        }
    }

    public static Database Database { get; } = new();
}
