namespace Application.Models;

public class User()
{
    public int Id { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? Token { get; set; }
    public string? TokenSource { get; set; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public required string RoleName { get; init; }
}