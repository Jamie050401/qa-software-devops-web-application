namespace Application.Data;

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

        if (value is null) return Response<IModel, Error>.BadRequestResponse($"Unable to instantiate Application.Models.{modelTypeName}");

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
            OperationType.Read => GetReadSql(value, tableName, id),
            OperationType.Update => GetUpdateSql(value, properties, tableName, id),
            OperationType.Delete => GetDeleteSql(value, tableName, id),
            _ => throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null)
        };

        switch (operationType)
        {
            case OperationType.Create:
            case OperationType.Update:
            case OperationType.Delete:
                var affected = this.ExecuteNonQuery(sql);
                if (affected <= 0)
                    return Response<IModel, Error>.BadRequestResponse("Database query failed");

                if (operationType is OperationType.Delete)
                    this.DeleteFromCache($"{id.GetValue(value)}");
                else
                    this.AddToCache($"{id.GetValue(value)}", value);
                return Response<IModel, Error>.OkResponse();
            case OperationType.Read:
                var valueInCache = (IModel?)this.GetFromCache($"{id.GetValue(value)}");
                if (valueInCache is not null)
                    return Response<IModel, Error>.OkValueResponse(valueInCache);

                var dbResponse = this.ExecuteReader(sql);
                if (!dbResponse.Reader.Read())
                    return Response<IModel, Error>.NotFoundResponse($"{id.GetValue(value)} does not exist within the database");

                var propertyValues = new object[properties.Count()];
                dbResponse.Reader.GetValues(propertyValues);
                dbResponse.Dispose();

                var valueInDb = (IModel?)Activator.CreateInstance("Application", $"Application.Models.{valueType.Name}")?.Unwrap();
                if (valueInDb is null)
                    return Response<IModel, Error>.BadRequestResponse($"Unable to instantiate Application.Models.{valueType.Name}");

                foreach (var property in properties.Zip(propertyValues, Tuple.Create))
                {
                    GetDatabaseValue(valueInDb, property);
                }
                this.AddToCache($"{valueInDb.Id}", valueInDb);
                return Response<IModel, Error>.OkValueResponse(valueInDb);
            default:
                throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null);
        }
    }

    private static PropertyInfo? GetId(OperationType operationType, IModel value, IEnumerable<PropertyInfo> properties)
    {
        properties = properties as PropertyInfo[] ?? properties.ToArray();
        var id = properties.FirstOrDefault(propertyInfo => propertyInfo.Name == "Id");

        if (id is not null)
        {
            var idValue = id.GetValue(value);
            if (idValue is null) return null;

            var defaultValue = Activator.CreateInstance(id.PropertyType);
            if (idValue == defaultValue) id = null;
        }

        if (operationType is not (OperationType.Read or OperationType.Delete)) return id;

        PropertyInfo? subId = null;
        subId = value.GetType().Name switch
        {
            "Role" => properties.FirstOrDefault(propertyInfo => propertyInfo.Name == "Name"),
            "User" => properties.FirstOrDefault(propertyInfo => propertyInfo.Name == "Email"),
            _ => subId
        };

        // ReSharper disable once InvertIf
        if (subId is not null)
        {
            var subIdValue = subId.GetValue(value);
            if (subIdValue is null) return id;

            switch (subIdValue)
            {
                case string val:
                    subId = string.IsNullOrEmpty(val) ? null : subId;
                    break;
                default:
                    var defaultValue = Activator.CreateInstance(subId.PropertyType);
                    if (subIdValue == defaultValue) subId = null;
                    break;
            }
        }

        return subId ?? id;
    }

    private static string GetCreateSql(IModel value, IEnumerable<PropertyInfo> properties, string tableName)
    {
        properties = properties as PropertyInfo[] ?? properties.ToArray();
        var columnHeaders = GetColumnHeaders(properties);
        var columnValues = GetColumnValues(value, properties);
        return $"INSERT INTO {tableName} ({columnHeaders}) VALUES ({columnValues});";
    }

    private static string GetReadSql(IModel value, string tableName, PropertyInfo id)
    {
        var conditionValue = GetConditionValue(value, id);
        return $"SELECT * FROM {tableName} WHERE {id.Name} = {conditionValue};";
    }

    private static string GetUpdateSql(IModel value, IEnumerable<PropertyInfo> properties, string tableName, PropertyInfo id)
    {
        var conditionValue = GetConditionValue(value, id);
        var columns = GetColumns(value, properties);
        return $"UPDATE {tableName} SET {columns} WHERE {id.Name} = {conditionValue};";
    }

    private static string GetDeleteSql(IModel value, string tableName, PropertyInfo id)
    {
        var conditionValue = GetConditionValue(value, id);
        return $"DELETE FROM {tableName} WHERE {id.Name} = {conditionValue}";
    }

    private static object? GetConditionValue(IModel value, PropertyInfo property)
    {
        var conditionValue = property.GetValue(value);
        conditionValue = conditionValue switch
        {
            string or Guid => $"\"{conditionValue}\"",
            _ => conditionValue
        };

        return conditionValue;
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

    private static void GetDatabaseValue(IModel valueInDb, Tuple<PropertyInfo, object> property)
    {
        if (property.Item2 is DBNull)
        {
            property.Item1.SetValue(valueInDb, null);
        }
        else
        {
            var propertyValue = property.Item1.PropertyType.Name switch
            {
                "Guid" => Guid.Parse((string)property.Item2),
                "AuthenticationData" => JsonConvert.DeserializeObject<AuthenticationData>((string)property.Item2),
                _ => Convert.ChangeType(property.Item2, property.Item1.PropertyType)
            };
            property.Item1.SetValue(valueInDb, propertyValue);
        }
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
