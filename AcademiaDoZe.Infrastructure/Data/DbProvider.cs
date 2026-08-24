using System.Data.Common;
using AcademiaDoZe.Infrastructure.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;

namespace AcademiaDoZe.Infrastructure.Data;

public static class DbProvider
{
    static DbProvider()
    {
        DbProviderFactories.RegisterFactory(GetProviderInvariantName(DatabaseType.SqlServer), SqlClientFactory.Instance);
        DbProviderFactories.RegisterFactory(GetProviderInvariantName(DatabaseType.Sqlite), SqliteFactory.Instance);
        DbProviderFactories.RegisterFactory(GetProviderInvariantName(DatabaseType.MySql), MySqlConnectorFactory.Instance);
    }

    public static DbConnection CreateConnection(string connectionString, DatabaseType databaseType)
    {
        var factory = DbProviderFactories.GetFactory(GetProviderInvariantName(databaseType));
        var connection = factory.CreateConnection()
            ?? throw new InfrastructureException("PROVEDOR_CONEXAO_INVALIDO", $"Nao foi possivel criar conexao para {databaseType}.");

        connection.ConnectionString = connectionString;
        return connection;
    }

    public static DbCommand CreateCommand(string commandText, DbConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = commandText;
        return command;
    }

    public static DbParameter AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
        return parameter;
    }

    public static string FormatInsertQuery(string insertSql, DatabaseType databaseType) => databaseType switch
    {
        DatabaseType.SqlServer => $"{insertSql}; SELECT CAST(SCOPE_IDENTITY() AS int);",
        DatabaseType.MySql => $"{insertSql}; SELECT LAST_INSERT_ID();",
        DatabaseType.Sqlite => $"{insertSql}; SELECT last_insert_rowid();",
        _ => throw new InfrastructureException("SGBD_NAO_SUPORTADO", $"SGBD nao suportado: {databaseType}.")
    };

    public static string GetScriptName(DatabaseType databaseType) => databaseType switch
    {
        DatabaseType.SqlServer => "script_sqlserver.sql",
        DatabaseType.MySql => "script_mysql.sql",
        DatabaseType.Sqlite => "script_sqlite.sql",
        _ => throw new InfrastructureException("SGBD_NAO_SUPORTADO", $"SGBD nao suportado: {databaseType}.")
    };

    public static string GetCaseInsensitiveEqualsExpression(string columnName, string parameterName, DatabaseType databaseType) => databaseType switch
    {
        DatabaseType.Sqlite => $"{columnName} = {parameterName} COLLATE NOCASE",
        _ => $"{columnName} = {parameterName}"
    };

    public static string GetCurrentDateFunction(DatabaseType databaseType) => databaseType switch
    {
        DatabaseType.SqlServer => "GETDATE()",
        DatabaseType.MySql => "CURRENT_TIMESTAMP",
        DatabaseType.Sqlite => "CURRENT_TIMESTAMP",
        _ => throw new InfrastructureException("SGBD_NAO_SUPORTADO", $"SGBD nao suportado: {databaseType}.")
    };

    public static string GetDateAddDaysExpression(string dateExpr, string daysParam, DatabaseType databaseType) => databaseType switch
    {
        DatabaseType.SqlServer => $"DATEADD(day, {daysParam}, {dateExpr})",
        DatabaseType.MySql => $"DATE_ADD({dateExpr}, INTERVAL {daysParam} DAY)",
        DatabaseType.Sqlite => $"datetime({dateExpr}, '+' || {daysParam} || ' days')",
        _ => throw new InfrastructureException("SGBD_NAO_SUPORTADO", $"SGBD nao suportado: {databaseType}.")
    };

    public static string GetDateHourExpression(string dateColumn, DatabaseType databaseType) => databaseType switch
    {
        DatabaseType.SqlServer => $"DATEPART(hour, {dateColumn})",
        DatabaseType.MySql => $"HOUR({dateColumn})",
        DatabaseType.Sqlite => $"CAST(strftime('%H', {dateColumn}) AS INTEGER)",
        _ => throw new InfrastructureException("SGBD_NAO_SUPORTADO", $"SGBD nao suportado: {databaseType}.")
    };

    public static string GetDateMonthExpression(string dateColumn, DatabaseType databaseType) => databaseType switch
    {
        DatabaseType.SqlServer => $"DATEPART(month, {dateColumn})",
        DatabaseType.MySql => $"MONTH({dateColumn})",
        DatabaseType.Sqlite => $"CAST(strftime('%m', {dateColumn}) AS INTEGER)",
        _ => throw new InfrastructureException("SGBD_NAO_SUPORTADO", $"SGBD nao suportado: {databaseType}.")
    };

    public static string GetDateDayExpression(string dateColumn, DatabaseType databaseType) => databaseType switch
    {
        DatabaseType.SqlServer => $"DATEPART(day, {dateColumn})",
        DatabaseType.MySql => $"DAY({dateColumn})",
        DatabaseType.Sqlite => $"CAST(strftime('%d', {dateColumn}) AS INTEGER)",
        _ => throw new InfrastructureException("SGBD_NAO_SUPORTADO", $"SGBD nao suportado: {databaseType}.")
    };

    private static string GetProviderInvariantName(DatabaseType databaseType) => databaseType switch
    {
        DatabaseType.SqlServer => "Microsoft.Data.SqlClient",
        DatabaseType.MySql => "MySqlConnector",
        DatabaseType.Sqlite => "Microsoft.Data.Sqlite",
        _ => throw new InfrastructureException("SGBD_NAO_SUPORTADO", $"SGBD nao suportado: {databaseType}.")
    };
}
