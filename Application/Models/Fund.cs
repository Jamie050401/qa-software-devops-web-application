namespace Application.Models;

using System;

public class Fund : ModelBase<Fund>
{
    [PrimaryKey, NonNullable]
    public override Guid Id { get; init; }
    [Index, Unique, NonNullable]
    public required string Name { get; init; }
    [NonNullable]
    public required decimal GrowthRate { get; init; }
    [NonNullable]
    public required decimal Charge { get; init; }
}