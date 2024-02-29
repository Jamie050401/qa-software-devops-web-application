namespace Application.Models;

public class Result : ModelBase<Result>
{
    [PrimaryKey, NonNullable]
    public override Guid Id { get; init; }
    [ForeignKey(TableName = "Users", ColumnName = "Id", DeleteAction = ForeignKeyDeleteAction.Cascade), Index, NonNullable]
    public required Guid UserId { get; init; }
    [NonNullable]
    public required decimal TotalInvestment { get; init; }
    [NonNullable]
    public required decimal ProjectedValue { get; init; }
}