namespace Application.Models;

public class User(int id, string username, string password, string? firstName, string? lastName, string roleName)
{
    public int Id { get; } = id;
    public string Username { get; } = username;
    public string Password { get; } = password;
    public string? FirstName { get; } = firstName;
    public string? LastName { get; } = lastName;
    public string RoleName { get; } = roleName;
}