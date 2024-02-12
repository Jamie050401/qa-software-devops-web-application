namespace Application.Models;

public class Fund
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public decimal GrowthRate { get; init; }
    public decimal Charge { get; init; }
}