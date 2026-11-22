
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoColaboradorRepository : BaseRepository, IAcessoColaboradorRepository
{
    public AcessoColaboradorRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    public async Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT id_acesso, pessoa_id, tipo_evento, data_hora
            FROM tb_acesso
            WHERE id_acesso = @Id AND pessoa_tipo = 1";

        await using var command = await CreateCommandAsync(query, cancellationToken);
        DbProvider.AddParameter(command, "@Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        await using var repositorio = new ColaboradorRepository(_connectionString, _databaseType);
        return await MapAsync(reader, repositorio, cancellationToken);
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT id_acesso, pessoa_id, tipo_evento, data_hora
            FROM tb_acesso
            WHERE pessoa_tipo = 1
            ORDER BY data_hora DESC";

        var acessos = new List<AcessoColaborador>();

        await using var command = await CreateCommandAsync(query, cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await using var repositorio = new ColaboradorRepository(_connectionString, _databaseType);

        while (await reader.ReadAsync(cancellationToken))
            acessos.Add(await MapAsync(reader, repositorio, cancellationToken));

        return acessos;
    }

    public async Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        var query = FormatInsertQuery(@"
            INSERT INTO tb_acesso
                (pessoa_tipo, pessoa_id, tipo_evento, data_hora)
            VALUES
                (1, @PessoaId, @TipoEvento, @DataHora)");

        await using var command = await CreateCommandAsync(query, cancellationToken);

        DbProvider.AddParameter(command, "@PessoaId", entity.ColaboradorId);
        DbProvider.AddParameter(command, "@TipoEvento", (int)entity.TipoEvento);
        DbProvider.AddParameter(command, "@DataHora", entity.DataHora);

        var id = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));

        return await ObterPorId(id, cancellationToken)
            ?? throw new InfrastructureException(
                "ERRO_ACESSO_COLABORADOR",
                "Não foi possível recuperar o acesso registrado.");
    }

    public async Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        const string query = @"
            UPDATE tb_acesso
            SET pessoa_id = @PessoaId,
                tipo_evento = @TipoEvento,
                data_hora = @DataHora
            WHERE id_acesso = @Id AND pessoa_tipo = 1";

        await using var command = await CreateCommandAsync(query, cancellationToken);

        DbProvider.AddParameter(command, "@Id", entity.Id);
        DbProvider.AddParameter(command, "@PessoaId", entity.ColaboradorId);
        DbProvider.AddParameter(command, "@TipoEvento", (int)entity.TipoEvento);
        DbProvider.AddParameter(command, "@DataHora", entity.DataHora);

        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0)
            throw new InfrastructureException("ACESSO_NAO_ENCONTRADO", "Acesso não encontrado.");

        return entity;
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        const string query = "DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = 1";

        await using var command = await CreateCommandAsync(query, cancellationToken);
        DbProvider.AddParameter(command, "@Id", id);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterAcessosPorColaboradorPeriodo(
        int? colaboradorId = null,
        DateOnly? inicio = null,
        DateOnly? fim = null,
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT id_acesso, pessoa_id, tipo_evento, data_hora
            FROM tb_acesso
            WHERE pessoa_tipo = 1
              AND (@ColaboradorId IS NULL OR pessoa_id = @ColaboradorId)
              AND (@Inicio IS NULL OR data_hora >= @Inicio)
              AND (@Fim IS NULL OR data_hora < @Fim)
            ORDER BY data_hora, id_acesso";

        await using var command = await CreateCommandAsync(query, cancellationToken);

        DbProvider.AddParameter(command, "@ColaboradorId",
            colaboradorId.HasValue ? colaboradorId.Value : DBNull.Value);

        DbProvider.AddParameter(command, "@Inicio",
            inicio.HasValue ? inicio.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);

        DbProvider.AddParameter(command, "@Fim",
            fim.HasValue ? fim.Value.AddDays(1).ToDateTime(TimeOnly.MinValue) : DBNull.Value);

        var acessos = new List<AcessoColaborador>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await using var repositorio = new ColaboradorRepository(_connectionString, _databaseType);

        while (await reader.ReadAsync(cancellationToken))
            acessos.Add(await MapAsync(reader, repositorio, cancellationToken));

        return acessos;
    }

    public async Task<AcessoColaborador?> ObterUltimoAcesso(
        int colaboradorId,
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT id_acesso, pessoa_id, tipo_evento, data_hora
            FROM tb_acesso
            WHERE pessoa_tipo = 1 AND pessoa_id = @ColaboradorId
            ORDER BY data_hora DESC, id_acesso DESC";

        await using var command = await CreateCommandAsync(query, cancellationToken);
        DbProvider.AddParameter(command, "@ColaboradorId", colaboradorId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        await using var repositorio = new ColaboradorRepository(_connectionString, _databaseType);
        return await MapAsync(reader, repositorio, cancellationToken);
    }

    public async Task<TimeSpan> ObterHorasTrabalhadasNoDia(
        int colaboradorId,
        DateOnly data,
        CancellationToken cancellationToken = default)
    {
        var acessos = await ObterAcessosPorColaboradorPeriodo(
            colaboradorId, data, data, cancellationToken);

        DateTime? entrada = null;
        var total = TimeSpan.Zero;

        foreach (var acesso in acessos.OrderBy(a => a.DataHora))
        {
            if (acesso.TipoEvento == TipoEventoAcesso.Entrada)
            {
                if (entrada is null)
                    entrada = acesso.DataHora;
            }
            else if (acesso.TipoEvento == TipoEventoAcesso.Saida && entrada.HasValue)
            {
                if (acesso.DataHora > entrada.Value)
                    total += acesso.DataHora - entrada.Value;

                entrada = null;
            }
        }

        return total;
    }

    private static async Task<AcessoColaborador> MapAsync(
        System.Data.Common.DbDataReader reader,
        IColaboradorRepository repositorio,
        CancellationToken cancellationToken)
    {
        var id = Convert.ToInt32(reader["id_acesso"]);
        var colaboradorId = Convert.ToInt32(reader["pessoa_id"]);
        var tipoEvento = (TipoEventoAcesso)Convert.ToInt32(reader["tipo_evento"]);
        var dataHora = Convert.ToDateTime(reader["data_hora"]);

        var colaborador = await repositorio.ObterPorId(colaboradorId, cancellationToken);

        if (colaborador is null)
            throw new InfrastructureException(
                "COLABORADOR_NAO_ENCONTRADO",
                $"Colaborador {colaboradorId} não encontrado para o registro de acesso.");

        var resultado = AcessoColaborador.Criar(
            id, colaborador, tipoEvento, dataHora);

        if (resultado.IsFailure)
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_ACESSO",
                "Não foi possível reconstruir o registro de acesso.");

        return resultado.Value!;
    }
}
