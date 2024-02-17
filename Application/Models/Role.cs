namespace Application.Models;

public class Role : IModel
{
    [PrimaryKey, NonNullable]
    public Guid Id { get; init; }
    [Index, Unique, NonNullable]
    public string Name { get; init; } = "";
}