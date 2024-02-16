namespace Application.Models;

public struct ForeignKey
{
    public string TableName { get; init; }
    public string ColumnName { get; init; }
}

public class Metadata
{
    public IList<string> Indexes { get; } = new List<string>();
    public IDictionary<string, bool> Nullable { get; } = new Dictionary<string, bool>();
    public IDictionary<string, ForeignKey> ForeignKeys { get; } = new Dictionary<string, ForeignKey>();
}

public interface IModel
{
    public Guid Id { get; }

    public Metadata Metadata { get; }
}