namespace Application.Data;

using Models;

public static class DatabaseManager
{
    public static void InitialiseDatabase()
    {
        Database.AddFundToDatabase(new Fund(0, "DummyFund", 0.0M, 0.0M));
        Database.AddRoleToDatabase(new Role("DummyRole"));
        Database.AddUserToDatabase(new User(0, "DummyUsername", "DummyPassword", "DummyRole"));
        Database.AddResultToDatabase(new Result(0, 0, 0.0M, 0.0M));
    }

    public static Database Database { get; } = new();
}
