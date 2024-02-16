﻿namespace Application.Data;

using Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reflection;

public class Database
{
    public Database()
    {
        Directory.CreateDirectory("Data");
        ConnectionString = "Data Source=Data/database.sqlite;";
        this.CreateTables();
    }

    public Database(string path, string? name = null)
    {
        name ??= "database";
        Directory.CreateDirectory(path);
        ConnectionString = $"Data Source={path}/{name}.sqlite";
        this.CreateTables();
    }

    public Response<IModel, Error> Create(IModel value)
    {
        return this.Interact(value, OperationType.Create);
    }

    public Response<IModel, Error> Read(string propertyName, object propertyValue, string modelTypeName)
    {
        var valueResponse = GetValue(propertyName, propertyValue, modelTypeName);

        if (valueResponse.Status is ResponseStatus.Error || !valueResponse.HasValue) return valueResponse;

        Debug.Assert(valueResponse.Value != null, "valueResponse.Value != null");
        return this.Interact(valueResponse.Value, OperationType.Read);
    }

    public Response<IModel, Error> Update(IModel value)
    {
        return this.Interact(value, OperationType.Update);
    }

    public Response<IModel, Error> Delete(string propertyName, object propertyValue, string modelTypeName)
    {
        var valueResponse = GetValue(propertyName, propertyValue, modelTypeName);

        if (valueResponse.Status is ResponseStatus.Error || !valueResponse.HasValue) return valueResponse;

        Debug.Assert(valueResponse.Value != null, "valueResponse.Value != null");
        return this.Interact(valueResponse.Value, OperationType.Delete);
    }

    private static Response<IModel, Error> GetValue(string propertyName, object propertyValue, string modelTypeName)
    {
        var value = (IModel?)Activator.CreateInstance("Application", $"Application.Models.{modelTypeName}")?.Unwrap();

        if (value is null) return Response<IModel, Error>.BadRequestResponse($"Unable to instantiate {modelTypeName}");

        var propertyInfo = value.GetType().GetProperty(propertyName);

        if (propertyInfo is null || !propertyInfo.CanWrite) return Response<IModel, Error>.BadRequestResponse($"Unable to find property {propertyName}");

        propertyInfo.SetValue(value, propertyValue);

        return Response<IModel, Error>.OkValueResponse(value);
    }

    private Response<IModel, Error> Interact(IModel value, OperationType operationType)
    {
        var valueType = value.GetType();
        var properties = valueType.GetProperties().Where(propertyInfo =>
            propertyInfo.Name != "Metadata");
        properties = properties as PropertyInfo[] ?? properties.ToArray();
        var id = GetId(operationType, value, properties);

        if (id is null) return Response<IModel, Error>.BadRequestResponse("Unable to determine Id from supplied model");

        var tableName = $"{valueType.Name}s";
        var sql = operationType switch
        {
            OperationType.Create => GetCreateSql(value, properties, tableName),
            OperationType.Read => "",
            OperationType.Update => GetUpdateSql(value, properties, tableName, id),
            OperationType.Delete => "",
            _ => throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null)
        };

        var affected = this.ExecuteNonQuery(sql);
        if (affected <= 0) return Response<IModel, Error>.BadRequestResponse("Database query failed");
        this.AddToCache($"{id.GetValue(value)}", value);
        return Response<IModel, Error>.OkResponse();
    }

    private static PropertyInfo? GetId(OperationType operationType, IModel value, IEnumerable<PropertyInfo> properties)
    {
        properties = properties as PropertyInfo[] ?? properties.ToArray();
        var id = properties.FirstOrDefault(propertyInfo => propertyInfo.Name == "Id");

        if (id is not null)
        {
            var propertyDefault = Activator.CreateInstance(id.PropertyType);
            var propertyValue = id.GetValue(value);
            if (propertyValue == propertyDefault) id = null;
        }

        if (operationType is not (OperationType.Read or OperationType.Delete)) return id;

        PropertyInfo? subId = null;
        subId = value.GetType().Name switch
        {
            "Application.Models.Role" => properties.FirstOrDefault(propertyInfo => propertyInfo.Name == "Name"),
            "Application.Models.User" => properties.FirstOrDefault(propertyInfo => propertyInfo.Name == "Email"),
            _ => subId
        };

        // ReSharper disable once InvertIf
        if (subId is not null)
        {
            var propertyDefault = Activator.CreateInstance(subId.PropertyType);
            var propertyValue = subId.GetValue(value);
            if (propertyValue == propertyDefault) subId = null;
        }

        return subId ?? id;
    }

    private static string GetCreateSql(IModel value, IEnumerable<PropertyInfo> properties, string tableName)
    {
        properties = properties as PropertyInfo[] ?? properties.ToArray();
        var columnHeaders = GetColumnHeaders(properties);
        var columnValues = GetColumnValues(value, properties);
        return $"""
            INSERT INTO {tableName} ({columnHeaders})
            VALUES ({columnValues});
        """;
    }

    private static string GetUpdateSql(IModel value, IEnumerable<PropertyInfo> properties, string tableName, PropertyInfo id)
    {
        var conditionValue = id.GetValue(value);
        conditionValue = conditionValue switch
        {
            string or Guid => $"\"{conditionValue}\"",
            _ => conditionValue
        };
        var columns = GetColumns(value, properties);
        return $"""
            UPDATE {tableName}
            SET {columns}
            WHERE {id.Name} = {conditionValue};
        """;
    }

    private static string GetColumnHeaders(IEnumerable<PropertyInfo> properties)
    {
        return properties.Aggregate("", (columnHeaders, propertyInfo) =>
            columnHeaders == "" ? $"{propertyInfo.Name}" : $"{columnHeaders}, {propertyInfo.Name}");
    }

    private static string GetColumnValues(IModel value, IEnumerable<PropertyInfo> properties)
    {
        return properties.Aggregate("", (columnValues, propertyInfo) =>
        {
            var propertyValue = GetPropertyValue(value, propertyInfo);
            return columnValues == ""
                ? $"{propertyValue}"
                : $"{columnValues}, {propertyValue}";
        });
    }

    private static string GetColumns(IModel value, IEnumerable<PropertyInfo> properties)
    {
        return properties.Aggregate("", (columns, propertyInfo) =>
        {
            var propertyValue = GetPropertyValue(value, propertyInfo);
            return columns == ""
                ? $"{propertyInfo.Name} = {propertyValue}"
                : $"{columns}, {propertyInfo.Name} = {propertyValue}";
        });
    }

    private static object GetPropertyValue(IModel value, PropertyInfo propertyInfo)
    {
        var propertyValue = propertyInfo.GetValue(value);
        switch (propertyValue)
        {
            case null:
                propertyValue = "NULL";
                break;
            case string:
            case Guid:
                propertyValue = $"\"{propertyValue}\"";
                break;
            case AuthenticationData:
                propertyValue = $"json_set('{JsonConvert.SerializeObject(propertyValue)}')";
                break;
        }
        return propertyValue;
    }

    private void CreateTables()
    {
        const string sqlCreateTables = """
            CREATE TABLE IF NOT EXISTS Funds (
                Id TEXT PRIMARY KEY NOT NULL,
                Name TEXT NOT NULL,
                GrowthRate REAL NOT NULL,
                Charge REAL NOT NULL
            );
           
            CREATE TABLE IF NOT EXISTS Roles (
                Id TEXT PRIMARY KEY NOT NULL,
                Name TEXT NOT NULL
            );
            
            CREATE UNIQUE INDEX IF NOT EXISTS idx_Name on Roles(Name);
           
            CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY NOT NULL,
                Email TEXT NOT NULL,
                Password TEXT NOT NULL,
                AuthenticationData TEXT,
                FirstName TEXT,
                LastName TEXT,
                RoleName TEXT NOT NULL REFERENCES Roles(Name) ON DELETE NO ACTION
            );

            CREATE UNIQUE INDEX IF NOT EXISTS idx_Email ON Users(Email);
            
            CREATE TABLE IF NOT EXISTS Results (
                Id TEXT PRIMARY KEY NOT NULL,
                UserId INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
                TotalInvestment REAL NOT NULL,
                ProjectedValue REAL NOT NULL
            );
        """;

        this.ExecuteNonQuery(sqlCreateTables);
    }

    private int ExecuteNonQuery(string sqlCommand)
    {
        using var dbConnection = new SqliteConnection(ConnectionString); dbConnection.Open();
        using var dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = sqlCommand;
        return dbCommand.ExecuteNonQuery();
    }

    private ExecuteReaderResponse ExecuteReader(string sqlCommand)
    {
        var dbConnection = new SqliteConnection(ConnectionString); dbConnection.Open();
        var dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = sqlCommand;
        return new ExecuteReaderResponse(dbConnection, dbCommand, dbCommand.ExecuteReader());
    }

    private readonly struct ExecuteReaderResponse(SqliteConnection connection, SqliteCommand command, SqliteDataReader reader)
    {
        public SqliteDataReader Reader { get; } = reader;
        private SqliteConnection Connection { get; } = connection;
        private SqliteCommand Command { get; } = command;

        public void Dispose()
        {
            Reader.Close(); Command.Dispose(); Connection.Close();
        }
    }

    private void DeleteFromCache(object? key)
    {
        if (key is not null) Cache.Remove(key);
    }

    private void AddToCache(object key, object value)
    {
        Cache.Set(key, value, CacheEntryOptions);
    }

    private object? GetFromCache(object? key)
    {
        if (key is null) return null;

        Cache.TryGetValue(key, out var value);
        return value;
    }

    private enum OperationType
    {
        Create,
        Read,
        Update,
        Delete
    }

    private string? ConnectionString { get; }
    private MemoryCache Cache { get; } = new(new MemoryCacheOptions());
    private MemoryCacheEntryOptions CacheEntryOptions { get; } = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(30)
    };
}
