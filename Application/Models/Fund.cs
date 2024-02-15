namespace Application.Models;

public class Fund : IModel
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public decimal GrowthRate { get; init; }
    public decimal Charge { get; init; }

    public IList<string> NonNullable { get; } = new List<string>();
    public IList<string> Indexes { get; } = new List<string>();
    public IDictionary<string, ForeignKey> ForeignKeys { get; } = new Dictionary<string, ForeignKey>();
}