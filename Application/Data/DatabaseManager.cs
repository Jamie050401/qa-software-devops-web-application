namespace Application.Data;

using Models;

public static class DatabaseManager
{
    public static void InitialiseDatabase()
    {
        const int id = 0;
        const string roleName = "DummyRole";
        if (DatabaseManager.Database.FundExistsInDatabase(id) ||
            DatabaseManager.Database.RoleExistsInDatabase(roleName) ||
            DatabaseManager.Database.UserExistsInDatabase(id) ||
            DatabaseManager.Database.ResultExistsInDatabase(id)) return;

        // ReSharper disable once StringLiteralTypo
        const string dummyPasswordHash = "2018F6578C204B187CEDFA7EFC8069FF70602E9FA1F4E77DFC068B190F95C493:A6DC1A645E1ADA26D2EB26E47838BFCC:50000:SHA256";
        Database.AddFundToDatabase(new Fund(id, "DummyFund", 0.0M, 0.0M));
        Database.AddRoleToDatabase(new Role(roleName));
        Database.AddUserToDatabase(new User
        {
            Id = id,
            Email = "dummyemail@email.com",
            Password = dummyPasswordHash,
            RoleName = "DummyRole"
        });
        Database.AddResultToDatabase(new Result(id, 0, 0.0M, 0.0M));
    }

    public static Database Database { get; } = new();
}
