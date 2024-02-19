namespace Application.Models;

public class Role : IModel
{
    [PrimaryKey, NonNullable]
    public required Guid Id { get; init; }
    [Index, Unique, NonNullable]
    public required string Name { get; init; } = "";
}