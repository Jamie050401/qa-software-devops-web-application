namespace Application.Models;

using Common;

public class User : IModel
{
    public Guid Id { get; init; }
    public string? Email { get; init; }
    public string? Password { get; init; }
    public AuthenticationData? AuthenticationData { get; set; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? RoleName { get; init; }

    public Metadata Metadata { get; } = new();
}