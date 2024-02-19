namespace Application.Models;

// TODO - Determine approach to dynamically generate this enum at compile time (with values for any type that implements IModel)
public enum Model
{
    Fund,
    Role,
    User,
    Result
}

public interface IModel
{
    public Guid Id { get; }
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
