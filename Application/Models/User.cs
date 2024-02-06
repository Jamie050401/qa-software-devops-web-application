namespace Application.Models;

public class User(int id, string email, string password, string roleName, string? firstName = null, string? lastName = null)
{
    public int Id { get; } = id;
    public string Email { get; } = email;
    public string Password { get; } = password;
    public string? FirstName { get; } = firstName;
    public string? LastName { get; } = lastName;
    public string RoleName { get; } = roleName;
}