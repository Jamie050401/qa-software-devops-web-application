namespace Application.Models;

public class Fund : IModel
{
    [PrimaryKey, NonNullable]
    public Guid Id { get; init; }
    [Index, Unique, NonNullable]
    public string Name { get; init; } = "";
    [NonNullable]
    public decimal GrowthRate { get; init; }
    [NonNullable]
    public decimal Charge { get; init; }
}