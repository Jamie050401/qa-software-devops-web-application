namespace Application.Models;

public class Role : IModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";

    public Metadata Metadata { get; } = new();
}