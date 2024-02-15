namespace Application.Models;

public class Fund : IModel
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public decimal GrowthRate { get; init; }
    public decimal Charge { get; init; }

    public Metadata Metadata { get; } = new();
}