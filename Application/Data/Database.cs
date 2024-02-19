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
    public Database(string? path = null, string? name = null)
    {
        path ??= "Data";
        name ??= "database";
        Directory.CreateDirectory(path);
        ConnectionString = $"Data Source={path}/{name}.sqlite";
        this.CreateTables();
    }

    public Response<IModel, Error> Create(IModel value)
    {
        return this.CreateUpdate(OperationType.Create, value);
    }

    public Response<IModel, Error> Read(string propertyName, object propertyValue, Model model)
    {
        var inputs = new Inputs
        {
            PropertyName = propertyName,
            PropertyValue = propertyValue,
            Model = model
        };

        return this.ReadDelete(OperationType.Read, inputs);
    }

    public Response<IModel, Error> Update(IModel value)
    {
        return this.CreateUpdate(OperationType.Update, value);
    }

    public Response<IModel, Error> Delete(string propertyName, object propertyValue, Model model)
    {
        var inputs = new Inputs
        {
            PropertyName = propertyName,
            PropertyValue = propertyValue,
            Model = model
        };

        return this.ReadDelete(OperationType.Delete, inputs);
    }

    private Response<IModel, Error> CreateUpdate(OperationType operationType, IModel value)
    {
        if (operationType is OperationType.Read or OperationType.Delete)
            return Response<IModel, Error>.BadRequestResponse($"{operationType} is not supported");

        var valueType = GetValueType(value, null);
        if (valueType is null)
            return Response<IModel, Error>.ServerErrorResponse("Failed to determine value type");

        var properties = valueType.GetProperties();

        var tableName = $"{valueType.Name}s";
        var sql = operationType is OperationType.Create
            ? GetCreateSql(value, properties, tableName)
            : GetUpdateSql(value, properties, tableName);

        int createUpdateAffected;
        try
        {
            createUpdateAffected = this.ExecuteNonQuery(sql);
        }
        catch (Exception ex)
        {
            return Response<IModel, Error>.BadRequestResponse($"Database query failed - {ex.Message}");
        }

        if (createUpdateAffected <= 0)
            return Response<IModel, Error>.NotFoundResponse($"{value.Id} does not exist within the database");

        // For Create/Update scenarios, id should always be the Guid enforced by the IModel interface
        this.AddToCache($"{value.Id}", value);
        return Response<IModel, Error>.OkResponse();
    }

    private Response<IModel, Error> ReadDelete(OperationType operationType, Inputs inputs)
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (operationType is OperationType.Create or OperationType.Update)
            return Response<IModel, Error>.BadRequestResponse($"{operationType} is not supported");

        if (operationType is OperationType.Read)
        {
            var valueInCache = (IModel?)this.GetFromCache($"{inputs.PropertyValue}");
            if (valueInCache is not null)
                return Response<IModel, Error>.OkValueResponse(valueInCache);
        }

        var valueType = GetValueType(null, inputs);
        if (valueType is null)
            return Response<IModel, Error>.ServerErrorResponse("Failed to determine value type");

        var properties = valueType.GetProperties();

        var tableName = $"{valueType.Name}s";
        var sql = operationType is OperationType.Read
            ? GetReadSql(inputs, tableName)
            : GetDeleteSql(inputs, tableName);

        if (operationType is OperationType.Read)
        {
            ExecuteReaderResponse readerResponse;
            try
            {
                readerResponse = this.ExecuteReader(sql);
            }
            catch (Exception ex)
            {
                return Response<IModel, Error>.ServerErrorResponse($"Database query failed - {ex.Message}");
            }

            if (!readerResponse.Reader.Read())
                return Response<IModel, Error>.NotFoundResponse($"{inputs.PropertyValue} does not exist within the database");

            var propertyValues = new object[properties.Length];
            readerResponse.Reader.GetValues(propertyValues);
            readerResponse.Dispose();

            var valueInDb = (IModel?)Activator.CreateInstance("Application", $"Application.Models.{valueType.Name}")?.Unwrap();
            if (valueInDb is null)
                return Response<IModel, Error>.BadRequestResponse($"Unable to instantiate Application.Models.{valueType.Name}");

            foreach (var property in properties.Zip(propertyValues, Tuple.Create))
            {
                GetDatabaseValue(valueInDb, property);
            }
            this.AddToCache($"{valueInDb.Id}", valueInDb);
            return Response<IModel, Error>.OkValueResponse(valueInDb);
        }

        // Recursive call to ensure I have the Guid to remove the item from the cache
        var dbResponse = this.ReadDelete(OperationType.Read, inputs);
        if (dbResponse.Status is ResponseStatus.Error || !dbResponse.HasValue)
            return Response<IModel, Error>.BadRequestResponse($"{inputs.PropertyValue} does not exist within the database");

        int deleteAffected;
        try
        {
            deleteAffected = this.ExecuteNonQuery(sql);
        }
        catch (Exception ex)
        {
            return Response<IModel, Error>.ServerErrorResponse($"Database query failed - {ex.Message}");
        }

        if (deleteAffected <= 0)
            return Response<IModel, Error>.NotFoundResponse($"{inputs.PropertyValue} does not exist within the database");

        Debug.Assert(dbResponse.Value != null, "dbResponse.Value != null");
        this.DeleteFromCache(dbResponse.Value.Id);
        return Response<IModel, Error>.OkResponse();
    }

    private static Type? GetValueType(IModel? value, Inputs? inputs)
    {
        try
        {
            if (value is null && inputs is not null)
                return Type.GetType($"Application.Models.{inputs.Value.ModelTypeName}")
                    ?? throw new Exception($"Failed to find type for {inputs.Value.ModelTypeName}");

            return value?.GetType();
        }
        catch
        {
            return null;
        }
    }

    private static string GetCreateSql(IModel value, IEnumerable<PropertyInfo> properties, string tableName)
    {
        properties = properties as PropertyInfo[] ?? properties.ToArray();
        var columnHeaders = GetColumnHeaders(properties);
        var columnValues = GetColumnValues(value, properties);
        return $"INSERT INTO {tableName} ({columnHeaders}) VALUES ({columnValues});";
    }

    private static string GetReadSql(Inputs inputs, string tableName)
    {
        var conditionValue = GetConditionValue(inputs);
        return $"SELECT * FROM {tableName} WHERE {inputs.PropertyName} = {conditionValue};";
    }

    private static string GetUpdateSql(IModel value, IEnumerable<PropertyInfo> properties, string tableName)
    {
        var columns = GetColumns(value, properties);
        return $"UPDATE {tableName} SET {columns} WHERE Id = \"{value.Id}\";";
    }

    private static string GetDeleteSql(Inputs inputs, string tableName)
    {
        var conditionValue = GetConditionValue(inputs);
        return $"DELETE FROM {tableName} WHERE {inputs.PropertyName} = {conditionValue}";
    }

    private static object GetConditionValue(Inputs inputs)
    {
        var conditionValue = inputs.PropertyValue switch
        {
            string or Guid => $"\"{inputs.PropertyValue}\"",
            _ => inputs.PropertyValue
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
        var models =
            from type in Assembly.GetExecutingAssembly().GetTypes()
            where type.GetInterfaces().Contains(typeof(IModel)) && type.GetConstructor(Type.EmptyTypes) != null
            select Activator.CreateInstance(type) as IModel;

        var tablesSql = "";
        var indexesSql = "";
        foreach (var model in models)
        {
            var type = model.GetType();
            var properties = type.GetProperties();

            var tableName = $"{type.Name}s";

            var columns = "";
            foreach (var property in properties)
            {
                var dbType = property.PropertyType.Name switch
                {
                    "decimal" => "REAL",
                    _ => "TEXT"
                };

                var primaryKeyAttribute = property.GetCustomAttribute<PrimaryKeyAttribute>();
                var primaryKey = primaryKeyAttribute is null
                    ? ""
                    : " PRIMARY KEY";

                var uniqueAttribute = property.GetCustomAttribute<UniqueAttribute>();
                var unique = uniqueAttribute is null
                    ? ""
                    : " UNIQUE";

                var nonNullableAttribute = property.GetCustomAttribute<NonNullableAttribute>();
                var nonNullable = nonNullableAttribute is null
                    ? ""
                    : " NOT NULL";

                var foreignKeyAttribute = property.GetCustomAttribute<ForeignKeyAttribute>();
                var foreignKey = foreignKeyAttribute is null
                    ? ""
                    : $" REFERENCES {foreignKeyAttribute.TableName}({foreignKeyAttribute.ColumnName}) ON DELETE {GetSqlForDeleteAction(foreignKeyAttribute.DeleteAction)}";

                columns = columns == ""
                    ? $"{property.Name} {dbType}{primaryKey}{unique}{nonNullable}{foreignKey}"
                    : $"{columns}, {property.Name} {dbType}{primaryKey}{unique}{nonNullable}{foreignKey}";

                var indexAttribute = property.GetCustomAttribute<IndexAttribute>();
                if (indexAttribute is not null)
                {
                    indexesSql = indexesSql == ""
                        ? $"CREATE UNIQUE INDEX IF NOT EXISTS idx_{property.Name} on {tableName}({property.Name});"
                        : $"{indexesSql} CREATE UNIQUE INDEX IF NOT EXISTS idx_{property.Name} on {tableName}({property.Name});";
                }
            }
            tablesSql = tablesSql == ""
                ? $"CREATE TABLE IF NOT EXISTS {tableName} ({columns});"
                : $"{tablesSql} CREATE TABLE IF NOT EXISTS {tableName} ({columns});";
        }
        this.ExecuteNonQuery(tablesSql);
        this.ExecuteNonQuery(indexesSql);
    }

    private static string GetSqlForDeleteAction(ForeignKeyDeleteAction deleteAction)
    {
        return deleteAction switch
        {
            ForeignKeyDeleteAction.Cascade => "CASCADE",
            _ => "NO ACTION"
        };
    }

    private int ExecuteNonQuery(string sqlCommand)
    {
        using var dbConnection = new SqliteConnection(ConnectionString);
        dbConnection.Open();
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

    private readonly struct Inputs
    {
        public string PropertyName { get; init; }
        public object PropertyValue { get; init; }
        public Model Model { get; init; }
        public string ModelTypeName => this.Model.ToString();
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
