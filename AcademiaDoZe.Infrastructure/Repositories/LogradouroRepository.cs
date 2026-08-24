// Alifer Granemannn

using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class LogradouroRepository : BaseRepository, ILogradouroRepository
{
    private const string BaseSelectQuery = "SELECT id_logradouro, cep, nome, bairro, cidade, estado, pais FROM tb_logradouro";

    public LogradouroRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType)
    {
    }

    public async Task<Logradouro?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var command = await CreateCommandAsync($"{BaseSelectQuery} WHERE id_logradouro = @Id", cancellationToken);
            DbProvider.AddParameter(command, "@Id", id);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter logradouro por ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Logradouro>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            var logradouros = new List<Logradouro>();
            await using var command = await CreateCommandAsync($"{BaseSelectQuery} ORDER BY id_logradouro", cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                logradouros.Add(Map(reader));
            }

            return logradouros;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter todos os logradouros: {ex.Message}", ex);
        }
    }

    public async Task<Logradouro> Adicionar(Logradouro entity, CancellationToken cancellationToken = default)
    {
        try
        {
            const string insertSql = """
                INSERT INTO tb_logradouro (cep, nome, bairro, cidade, estado, pais)
                VALUES (@Cep, @Nome, @Bairro, @Cidade, @Estado, @Pais)
                """;

            await using var command = await CreateCommandAsync(FormatInsertQuery(insertSql), cancellationToken);
            AdicionarParametrosLogradouro(command, entity);

            var id = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            var result = Logradouro.Criar(id, entity.Cep.Valor, entity.Nome, entity.Bairro, entity.Cidade, entity.Estado, entity.Pais);

            if (result.IsFailure)
            {
                throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro de dominio ao mapear logradouro inserido: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
            }

            return result.Value!;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_LOGRADOURO", $"Erro ao adicionar logradouro: {ex.Message}", ex);
        }
    }

    public async Task<Logradouro> Atualizar(Logradouro entity, CancellationToken cancellationToken = default)
    {
        try
        {
            const string updateSql = """
                UPDATE tb_logradouro
                SET cep = @Cep,
                    nome = @Nome,
                    bairro = @Bairro,
                    cidade = @Cidade,
                    estado = @Estado,
                    pais = @Pais
                WHERE id_logradouro = @Id
                """;

            await using var command = await CreateCommandAsync(updateSql, cancellationToken);
            DbProvider.AddParameter(command, "@Id", entity.Id);
            AdicionarParametrosLogradouro(command, entity);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rowsAffected == 0)
            {
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum logradouro encontrado com ID {entity.Id} para atualizacao.");
            }

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_LOGRADOURO", $"Erro ao atualizar logradouro ID {entity.Id}: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var command = await CreateCommandAsync("DELETE FROM tb_logradouro WHERE id_logradouro = @Id", cancellationToken);
            DbProvider.AddParameter(command, "@Id", id);
            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_LOGRADOURO", $"Erro ao remover logradouro ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<Logradouro?> ObterPorCep(Cep cep, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var command = await CreateCommandAsync($"{BaseSelectQuery} WHERE cep = @Cep", cancellationToken);
            DbProvider.AddParameter(command, "@Cep", cep.Valor);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_CEP", $"Erro ao obter logradouro por CEP {cep.Valor}: {ex.Message}", ex);
        }
    }

    public async Task<bool> CepJaExiste(Cep cep, int? id = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var idFilter = id.HasValue ? " AND id_logradouro <> @Id" : string.Empty;
            await using var command = await CreateCommandAsync($"SELECT COUNT(1) FROM tb_logradouro WHERE cep = @Cep{idFilter}", cancellationToken);
            DbProvider.AddParameter(command, "@Cep", cep.Valor);
            if (id.HasValue)
            {
                DbProvider.AddParameter(command, "@Id", id.Value);
            }

            return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_VERIFICAR_CEP", $"Erro ao verificar existencia de CEP: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Logradouro>> ObterPorCidade(string cidade, CancellationToken cancellationToken = default)
    {
        try
        {
            var logradouros = new List<Logradouro>();
            var filtro = GetCaseInsensitiveEqualsExpression("cidade", "@Cidade");

            await using var command = await CreateCommandAsync($"{BaseSelectQuery} WHERE {filtro} ORDER BY id_logradouro", cancellationToken);
            DbProvider.AddParameter(command, "@Cidade", cidade);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                logradouros.Add(Map(reader));
            }

            return logradouros;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_CIDADE", $"Erro ao obter logradouros por cidade {cidade}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Logradouro>> ObterPorBairro(string cidade, string bairro, CancellationToken cancellationToken = default)
    {
        try
        {
            var logradouros = new List<Logradouro>();
            var filtroCidade = GetCaseInsensitiveEqualsExpression("cidade", "@Cidade");
            var filtroBairro = GetCaseInsensitiveEqualsExpression("bairro", "@Bairro");

            await using var command = await CreateCommandAsync($"{BaseSelectQuery} WHERE {filtroCidade} AND {filtroBairro} ORDER BY id_logradouro", cancellationToken);
            DbProvider.AddParameter(command, "@Cidade", cidade);
            DbProvider.AddParameter(command, "@Bairro", bairro);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                logradouros.Add(Map(reader));
            }

            return logradouros;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_BAIRRO", $"Erro ao obter logradouros por bairro {bairro}: {ex.Message}", ex);
        }
    }

    public static Logradouro Map(DbDataReader reader)
    {
        try
        {
            var id = Convert.ToInt32(reader["id_logradouro"]);
            var result = Logradouro.Criar(
                id,
                Convert.ToString(reader["cep"])!,
                Convert.ToString(reader["nome"])!,
                Convert.ToString(reader["bairro"])!,
                Convert.ToString(reader["cidade"])!,
                Convert.ToString(reader["estado"])!,
                Convert.ToString(reader["pais"])!);

            if (result.IsFailure)
            {
                throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro de dominio ao mapear logradouro ID {id}: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
            }

            return result.Value!;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("ERRO_MAPEAMENTO_LOGRADOURO", $"Erro ao mapear dados do logradouro: {ex.Message}", ex);
        }
    }

    private static void AdicionarParametrosLogradouro(DbCommand command, Logradouro entity)
    {
        DbProvider.AddParameter(command, "@Cep", entity.Cep.Valor);
        DbProvider.AddParameter(command, "@Nome", entity.Nome);
        DbProvider.AddParameter(command, "@Bairro", entity.Bairro);
        DbProvider.AddParameter(command, "@Cidade", entity.Cidade);
        DbProvider.AddParameter(command, "@Estado", entity.Estado);
        DbProvider.AddParameter(command, "@Pais", entity.Pais);
    }
}
