namespace Application.Models;

public class Result : IModel
{
    [PrimaryKey, NonNullable]
    public Guid Id { get; init; }
    [ForeignKey(TableName = "Users", ColumnName = "Id"), Index, NonNullable]
    public Guid UserId { get; init; }
    [NonNullable]
    public decimal TotalInvestment { get; init; }
    [NonNullable]
    public decimal ProjectedValue { get; init; }
}