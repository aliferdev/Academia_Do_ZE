// Alifer Granemannn

using System.Data;
using System.Data.Common;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public abstract class BaseRepository : IDisposable, IAsyncDisposable
{
    protected readonly string _connectionString;
    protected readonly DatabaseType _databaseType;
    private DbConnection? _connection;
    private bool _disposed;

    protected BaseRepository(string connectionString, DatabaseType databaseType)
    {
        _connectionString = connectionString ?? throw new InfrastructureException("STRING_CONEXAO_NULA", $"String de conexao nao pode ser nula: {nameof(connectionString)}");
        _databaseType = databaseType;
    }

    protected virtual async Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await DbInitializer.InicializarAsync(_connectionString, _databaseType, cancellationToken);

            if (_connection is null)
            {
                _connection = DbProvider.CreateConnection(_connectionString, _databaseType);
                await _connection.OpenAsync(cancellationToken);
            }
            else if (_connection.State is ConnectionState.Broken or ConnectionState.Closed)
            {
                await _connection.CloseAsync();
                await _connection.OpenAsync(cancellationToken);
            }

            return _connection;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("FALHA_ABRIR_CONEXAO", "Falha ao abrir conexao com o banco de dados.", ex);
        }
    }

    protected virtual async Task<DbCommand> CreateCommandAsync(string commandText, CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);
        return DbProvider.CreateCommand(commandText, connection);
    }

    protected string FormatInsertQuery(string insertSql) => DbProvider.FormatInsertQuery(insertSql, _databaseType);

    protected string GetCurrentDateFunction() => DbProvider.GetCurrentDateFunction(_databaseType);

    protected string GetDateAddDaysExpression(string dateExpr, string daysParam) => DbProvider.GetDateAddDaysExpression(dateExpr, daysParam, _databaseType);

    protected string GetDateHourExpression(string dateColumn) => DbProvider.GetDateHourExpression(dateColumn, _databaseType);

    protected string GetDateMonthExpression(string dateColumn) => DbProvider.GetDateMonthExpression(dateColumn, _databaseType);

    protected string GetDateDayExpression(string dateColumn) => DbProvider.GetDateDayExpression(dateColumn, _databaseType);

    protected string GetCaseInsensitiveEqualsExpression(string columnName, string parameterName) => DbProvider.GetCaseInsensitiveEqualsExpression(columnName, parameterName, _databaseType);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _connection?.Dispose();
        }

        _disposed = true;
    }
}
