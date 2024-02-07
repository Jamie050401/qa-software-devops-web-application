namespace Application.Data;

using Models;

public static class DatabaseManager
{
    public static void InitialiseDatabase()
    {
        // ReSharper disable once StringLiteralTypo
        const string dummyPasswordHash = "2018F6578C204B187CEDFA7EFC8069FF70602E9FA1F4E77DFC068B190F95C493:A6DC1A645E1ADA26D2EB26E47838BFCC:50000:SHA256";
        Database.AddFundToDatabase(new Fund(0, "DummyFund", 0.0M, 0.0M));
        Database.AddRoleToDatabase(new Role("DummyRole"));
        Database.AddUserToDatabase(new User(0, "dummyemail@email.com", dummyPasswordHash, "DummyRole"));
        Database.AddResultToDatabase(new Result(0, 0, 0.0M, 0.0M));
    }

    public static Database Database { get; } = new();
}
