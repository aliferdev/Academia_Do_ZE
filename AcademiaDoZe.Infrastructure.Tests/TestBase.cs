// Alifer Granemannn

using AcademiaDoZe.Infrastructure.Data;
using Xunit;

[assembly: CollectionBehavior(
    CollectionBehavior.CollectionPerAssembly,
    DisableTestParallelization = true)]

namespace AcademiaDoZe.Infrastructure.Tests;

public abstract class TestBase
{
    // ALTERE SOMENTE ESTA LINHA PARA TROCAR O BANCO
    private const DatabaseType SelectedDatabaseType = DatabaseType.MySql;
    protected DatabaseType DatabaseType { get; }

    protected string ConnectionString { get; }

    protected string SgbdSigla => DatabaseType switch
    {
        DatabaseType.SqlServer => "SQLServer",
        DatabaseType.MySql => "MySQL",
        DatabaseType.Sqlite => "SQLite",
        _ => throw new ArgumentOutOfRangeException(
            nameof(DatabaseType),
            DatabaseType,
            "SGBD nao suportado para testes.")
    };

    protected string NomeRua => "Alifer";

    protected string NomeBairro => "Granemann";

    protected string NomeCidade => DatabaseType switch
    {
        DatabaseType.SqlServer => "SQLServer",
        DatabaseType.MySql => "MySQL",
        DatabaseType.Sqlite => "SQLite",
        _ => throw new ArgumentOutOfRangeException(
            nameof(DatabaseType),
            DatabaseType,
            "SGBD nao suportado para testes.")
    };

    protected TestBase()
    {
        DatabaseType = SelectedDatabaseType;

        var dbPath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "db_academia_do_ze.db"));

        Directory.CreateDirectory(
            Path.GetDirectoryName(dbPath)!);

        ConnectionString = DatabaseType switch
        {
            DatabaseType.SqlServer =>
                "Server=localhost,1433;" +
                "Database=db_academia_do_ze;" +
                "User Id=sa;" +
                "Password=abcBolinhas12345;" +
                "TrustServerCertificate=True;" +
                "Encrypt=False;",

            DatabaseType.MySql =>
                "Server=localhost;" +
                "Port=3307;" +
                "Database=db_academia_do_ze;" +
                "User Id=root;" +
                "Password=abcBolinhas12345;" +
                "SslMode=None;" +
                "AllowPublicKeyRetrieval=True;",

            DatabaseType.Sqlite =>
                $"Data Source={dbPath};Cache=Shared;",

            _ => throw new ArgumentOutOfRangeException(
                nameof(DatabaseType),
                DatabaseType,
                "SGBD nao suportado para testes.")
        };
    }

    private static int _counter = 10000;

    protected static string GerarCep() =>
        (
            80000000
            + ((int)(DateTime.UtcNow.Ticks % 8000000))
            + Interlocked.Increment(ref _counter)
        )
        .ToString("D8")[..8];

    protected static string GerarCpf() =>
        (
            10000000000L
            + (DateTime.UtcNow.Ticks % 8000000000L)
            + Interlocked.Increment(ref _counter)
        )
        .ToString("D11")[..11];

    protected static string GerarEmail() =>
        $"user_{Guid.NewGuid():N}@test.com";

    protected static string GerarTelefone() =>
        (
            49990000000L
            + (DateTime.UtcNow.Ticks % 8000000000L)
            + Interlocked.Increment(ref _counter)
        )
        .ToString("D11")[..11];
}