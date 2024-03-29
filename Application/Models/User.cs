﻿namespace Application.Models;

using Common;
using System;

public class User : ModelBase<User>
{
    public static User Default()
    {
        return new User
        {
            Id = Guid.Empty,
            RoleId = Guid.Empty,
            Email = string.Empty,
            Password = string.Empty
        };
    }

    [PrimaryKey, NonNullable]
    public override Guid Id { get; init; }
    [ForeignKey(TableName = "Roles", ColumnName = "Id", DeleteAction = ForeignKeyDeleteAction.None), NonNullable]
    public required Guid RoleId { get; init; }
    [Index, Unique, NonNullable]
    public required string Email { get; init; }
    [NonNullable]
    public required string Password { get; init; }
    public AuthenticationData? AuthenticationData { get; set; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}