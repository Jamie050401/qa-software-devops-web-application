namespace Application.Models;

public class Result : IModel
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public decimal TotalInvestment { get; init; }
    public decimal ProjectedValue { get; init; }

    public IList<string> NonNullable { get; } = new List<string>();
    public IList<string> Indexes { get; } = new List<string>();
    public IDictionary<string, ForeignKey> ForeignKeys { get; } = new Dictionary<string, ForeignKey>();
}