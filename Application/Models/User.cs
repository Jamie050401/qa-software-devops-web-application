namespace Application.Models;

using Common;

public class User : IModel
{
    public Guid Id { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public AuthenticationData? AuthenticationData { get; set; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public required string RoleName { get; init; }

    public IList<string> Indexes { get; } = new List<string>();
    public IDictionary<string, ForeignKey> ForeignKeys { get; } = new Dictionary<string, ForeignKey>();
}