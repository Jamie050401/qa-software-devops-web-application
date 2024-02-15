namespace Application.Data;

using Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using Models;
using Newtonsoft.Json;
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
        return this.Interact(OperationType.Create, value);
    }

    public Response<IModel, Error> Read(IModel value)
    {
        return this.Interact(OperationType.Read, value);
    }

    public Response<IModel, Error> Update(IModel value)
    {
        return this.Interact(OperationType.Update, value);
    }

    public Response<IModel, Error> Delete(IModel value)
    {
        return this.Interact(OperationType.Delete, value);
    }

    private Response<IModel, Error> Interact(OperationType operationType, IModel value)
    {
        var valueType = value.GetType();
        var properties = valueType.GetProperties().Where(propertyInfo =>
            propertyInfo.Name != "NonNullable"
            && propertyInfo.Name != "Indexes"
            && propertyInfo.Name != "ForeignKeys").ToArray();
        var id =
            properties.FirstOrDefault(propertyInfo => propertyInfo.Name == "Id")
            ?? properties.FirstOrDefault(propertyInfo => propertyInfo.Name == "Name");

        if (id is null) return Response<IModel, Error>.BadRequestResponse();

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
        if (affected <= 0) return Response<IModel, Error>.BadRequestResponse();
        this.AddToCache($"{id.GetValue(value)}", value);
        return Response<IModel, Error>.OkResponse();
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
        var columns = GetColumns(value, properties);
        return $"""
            UPDATE {tableName}
            SET {columns}
            WHERE {id.Name} = "{id.GetValue(value)}";
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
                : $"{columns},{Environment.NewLine}{propertyInfo.Name} = {propertyValue}";
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
