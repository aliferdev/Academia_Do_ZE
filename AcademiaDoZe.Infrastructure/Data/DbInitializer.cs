
// Alifer Granemannn

using System.Collections.Concurrent;
using System.Data.Common;
using System.Reflection;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Data;

public static class DbInitializer
{
    private static readonly ConcurrentDictionary<string, bool> BancosInicializados = new();

    public static async Task InicializarAsync(
        string connectionString,
        DatabaseType databaseType,
        CancellationToken cancellationToken = default)
    {
        var key = $"{databaseType}:{connectionString}";

        if (BancosInicializados.ContainsKey(key))
        {
            return;
        }

        try
        {
            var scriptSql = ObterScript(databaseType);

            await using var connection =
                DbProvider.CreateConnection(connectionString, databaseType);

            await connection.OpenAsync(cancellationToken);

            await using (var command = DbProvider.CreateCommand(scriptSql, connection))
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            // Migração para bancos SQLite que já existiam.
            if (databaseType == DatabaseType.Sqlite)
            {
                var colunaExiste = false;

                await using (var command = DbProvider.CreateCommand(
                    "PRAGMA table_info(tb_acesso);", connection))
                {
                    await using var reader =
                        await command.ExecuteReaderAsync(cancellationToken);

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        if (string.Equals(
                            reader["name"]?.ToString(),
                            "tipo_evento",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            colunaExiste = true;
                            break;
                        }
                    }
                }

                if (!colunaExiste)
                {
                    await using var alterCommand = DbProvider.CreateCommand(
                        "ALTER TABLE tb_acesso ADD COLUMN tipo_evento INTEGER NOT NULL DEFAULT 1;",
                        connection);

                    await alterCommand.ExecuteNonQueryAsync(cancellationToken);
                }
            }

            BancosInicializados.TryAdd(key, true);
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_INICIALIZAR_BANCO",
                $"Erro ao inicializar banco de dados: {ex.Message}",
                ex);
        }
    }

    public static string ObterScript(DatabaseType databaseType)
    {
        var nomeScript = DbProvider.GetScriptName(databaseType);
        var assembly = Assembly.GetExecutingAssembly();

        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(r => r.EndsWith(
                nomeScript,
                StringComparison.OrdinalIgnoreCase))
            ?? throw new InfrastructureException(
                "SCRIPT_EMBARCADO_NAO_ENCONTRADO",
                $"Script SQL embarcado '{nomeScript}' nao encontrado.");

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InfrastructureException(
                "ERRO_LEITURA_SCRIPT",
                $"Erro ao carregar o fluxo do script embarcado '{nomeScript}'.");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
