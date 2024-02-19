namespace Application.Models;

public class Fund : IModel
{
    [PrimaryKey, NonNullable]
    public required Guid Id { get; init; }
    [Index, Unique, NonNullable]
    public required string Name { get; init; } = "";
    [NonNullable]
    public required decimal GrowthRate { get; init; }
    [NonNullable]
    public required decimal Charge { get; init; }
}