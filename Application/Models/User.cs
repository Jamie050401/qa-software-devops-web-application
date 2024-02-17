namespace Application.Models;

using Common;

public class User : IModel
{
    [PrimaryKey, NonNullable]
    public Guid Id { get; init; }
    [Index, Unique, NonNullable]
    public string Email { get; init; } = "";
    [NonNullable]
    public string Password { get; init; } = "";
    public AuthenticationData? AuthenticationData { get; set; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    [ForeignKey(TableName = "Roles", ColumnName = "Name"), NonNullable]
    public string RoleName { get; init; } = "";
}