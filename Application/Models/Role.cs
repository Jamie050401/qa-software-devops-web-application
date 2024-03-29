﻿namespace Application.Models;

using System;

public class Role : ModelBase<Role>
{
    [PrimaryKey, NonNullable]
    public override Guid Id { get; init; }
    [Index, Unique, NonNullable]
    public required string Name { get; init; }
}