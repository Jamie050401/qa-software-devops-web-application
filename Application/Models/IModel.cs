namespace Application.Models;

public struct ForeignKey
{
    public string TableName { get; init; }
    public string ColumnName { get; init; }
}

public interface IModel
{
    public Guid Id { get; }

    public IList<string> Indexes { get; }
    public IDictionary<string, ForeignKey> ForeignKeys { get; }
}