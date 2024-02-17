namespace Application.Models;

[AttributeUsage(AttributeTargets.Property)]
public class PrimaryKeyAttribute : Attribute
{
    public readonly bool IsPrimaryKey = true;
}

[AttributeUsage(AttributeTargets.Property)]
public class ForeignKeyAttribute : Attribute
{
    public required string TableName;
    public required string ColumnName;
    public required ForeignKeyDeleteAction DeleteAction;
}

public enum ForeignKeyDeleteAction
{
    None,
    Cascade
}

[AttributeUsage(AttributeTargets.Property)]
public class IndexAttribute : Attribute
{
    public readonly bool HasIndex = true;
}

[AttributeUsage(AttributeTargets.Property)]
public class UniqueAttribute : Attribute
{
    public readonly bool IsUnique = true;
}

[AttributeUsage(AttributeTargets.Property)]
public class NonNullableAttribute : Attribute
{
    public readonly bool NonNullable = true;
}

public interface IModel
{
    public Guid Id { get; }
}