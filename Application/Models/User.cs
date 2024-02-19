namespace Application.Models;

using Common;

public class User : ModelBase<User>
{
    [PrimaryKey, NonNullable]
    public override Guid Id { get; init; }
    [ForeignKey(TableName = "Roles", ColumnName = "Id", DeleteAction = ForeignKeyDeleteAction.None), NonNullable]
    public required Guid RoleId { get; init; }
    [Index, Unique, NonNullable]
    public required string Email { get; init; } = "";
    [NonNullable]
    public required string Password { get; init; } = "";
    public AuthenticationData? AuthenticationData { get; set; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}