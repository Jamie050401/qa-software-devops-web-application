namespace Application.Models;

public class Role : IModel
{
    public Guid Id { get; init; }
    public required string Name { get; init; }

    public IList<string> Indexes { get; } = new List<string>();
    public IDictionary<string, ForeignKey> ForeignKeys { get; } = new Dictionary<string, ForeignKey>();
}