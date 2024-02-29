namespace Application.Models;

using System.Reflection;

public interface IModel
{
    public Guid Id { get; }
}

public abstract class ModelBase<T> : IModel where T : ModelBase<T>
{
    public abstract Guid Id { get; init; }

    public static PropertyInfo? GetProperty(string propertyName)
    {
        return typeof(T).GetProperty(propertyName);
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class PrimaryKeyAttribute : Attribute;

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
public class IndexAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public class UniqueAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public class NonNullableAttribute : Attribute;
