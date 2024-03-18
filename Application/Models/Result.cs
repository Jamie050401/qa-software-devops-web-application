namespace Application.Models;

using System;

public class Result : ModelBase<Result>
{
    public static Result Default()
    {
        return new Result
        {
            Id = Guid.Empty,
            UserId = Guid.Empty,
            TotalInvestment = 0.0M,
            ProjectedValue = 0.0M
        };
    }

    [PrimaryKey, NonNullable]
    public override Guid Id { get; init; }
    [ForeignKey(TableName = "Users", ColumnName = "Id", DeleteAction = ForeignKeyDeleteAction.Cascade), Index, NonNullable]
    public required Guid UserId { get; init; }
    [NonNullable]
    public required decimal TotalInvestment { get; init; }
    [NonNullable]
    public required decimal ProjectedValue { get; init; }
}