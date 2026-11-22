
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    private readonly Func<IAlunoRepository> _alunoRepoFactory;

    public AcessoAlunoRepository(
        string connectionString,
        DatabaseType databaseType,
        Func<IAlunoRepository> alunoRepoFactory)
        : base(connectionString, databaseType)
    {
        _alunoRepoFactory = alunoRepoFactory
            ?? throw new ArgumentNullException(nameof(alunoRepoFactory));
    }

    public async Task<AcessoAluno?> ObterPorId(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT id_acesso, pessoa_id, tipo_evento, data_hora
            FROM tb_acesso
            WHERE id_acesso = @Id AND pessoa_tipo = 0";

        await using var command = await CreateCommandAsync(query, cancellationToken);
        DbProvider.AddParameter(command, "@Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return await MapAsync(reader, cancellationToken);
    }

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT id_acesso, pessoa_id, tipo_evento, data_hora
            FROM tb_acesso
            WHERE pessoa_tipo = 0
            ORDER BY data_hora DESC";

        var acessos = new List<AcessoAluno>();

        await using var command = await CreateCommandAsync(query, cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
            acessos.Add(await MapAsync(reader, cancellationToken));

        return acessos;
    }

    public async Task<AcessoAluno> Adicionar(
        AcessoAluno entity,
        CancellationToken cancellationToken = default)
    {
        var query = FormatInsertQuery(@"
            INSERT INTO tb_acesso
                (pessoa_tipo, pessoa_id, tipo_evento, data_hora)
            VALUES
                (0, @PessoaId, @TipoEvento, @DataHora)");

        await using var command = await CreateCommandAsync(query, cancellationToken);

        DbProvider.AddParameter(command, "@PessoaId", entity.AlunoId);
        DbProvider.AddParameter(command, "@TipoEvento", (int)entity.TipoEvento);
        DbProvider.AddParameter(command, "@DataHora", entity.DataHora);

        var id = Convert.ToInt32(
            await command.ExecuteScalarAsync(cancellationToken));

        return await ObterPorId(id, cancellationToken)
            ?? throw new InfrastructureException(
                "ERRO_ACESSO_ALUNO",
                "Não foi possível recuperar o acesso registrado.");
    }

    public async Task<AcessoAluno> Atualizar(
        AcessoAluno entity,
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            UPDATE tb_acesso
            SET pessoa_id = @PessoaId,
                tipo_evento = @TipoEvento,
                data_hora = @DataHora
            WHERE id_acesso = @Id AND pessoa_tipo = 0";

        await using var command = await CreateCommandAsync(query, cancellationToken);

        DbProvider.AddParameter(command, "@Id", entity.Id);
        DbProvider.AddParameter(command, "@PessoaId", entity.AlunoId);
        DbProvider.AddParameter(command, "@TipoEvento", (int)entity.TipoEvento);
        DbProvider.AddParameter(command, "@DataHora", entity.DataHora);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);

        if (affected == 0)
        {
            throw new InfrastructureException(
                "ACESSO_NAO_ENCONTRADO",
                "Acesso não encontrado.");
        }

        return entity;
    }

    public async Task<bool> Remover(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            DELETE FROM tb_acesso
            WHERE id_acesso = @Id AND pessoa_tipo = 0";

        await using var command = await CreateCommandAsync(query, cancellationToken);
        DbProvider.AddParameter(command, "@Id", id);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(
        int? alunoId = null,
        DateOnly? inicio = null,
        DateOnly? fim = null,
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT id_acesso, pessoa_id, tipo_evento, data_hora
            FROM tb_acesso
            WHERE pessoa_tipo = 0
              AND (@AlunoId IS NULL OR pessoa_id = @AlunoId)
              AND (@Inicio IS NULL OR data_hora >= @Inicio)
              AND (@Fim IS NULL OR data_hora < @Fim)
            ORDER BY data_hora";

        await using var command = await CreateCommandAsync(query, cancellationToken);

        DbProvider.AddParameter(
            command,
            "@AlunoId",
            alunoId.HasValue ? alunoId.Value : DBNull.Value);

        DbProvider.AddParameter(
            command,
            "@Inicio",
            inicio.HasValue
                ? inicio.Value.ToDateTime(TimeOnly.MinValue)
                : DBNull.Value);

        DbProvider.AddParameter(
            command,
            "@Fim",
            fim.HasValue
                ? fim.Value.AddDays(1).ToDateTime(TimeOnly.MinValue)
                : DBNull.Value);

        var acessos = new List<AcessoAluno>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
            acessos.Add(await MapAsync(reader, cancellationToken));

        return acessos;
    }

    public async Task<AcessoAluno?> ObterUltimoAcesso(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT id_acesso, pessoa_id, tipo_evento, data_hora
            FROM tb_acesso
            WHERE pessoa_tipo = 0 AND pessoa_id = @AlunoId
            ORDER BY data_hora DESC, id_acesso DESC";

        await using var command = await CreateCommandAsync(query, cancellationToken);
        DbProvider.AddParameter(command, "@AlunoId", alunoId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken)
            ? await MapAsync(reader, cancellationToken)
            : null;
    }

    public async Task<bool> EstaNaAcademia(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var ultimo = await ObterUltimoAcesso(alunoId, cancellationToken);

        return ultimo?.TipoEvento == TipoEventoAcesso.Entrada;
    }

    public Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(
        int mes,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(
        int mes,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(
        int dias,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task<AcessoAluno> MapAsync(
        DbDataReader reader,
        CancellationToken cancellationToken)
    {
        // Lê os dados do acesso antes de consultar o repositório do aluno.
        var alunoId = Convert.ToInt32(reader["pessoa_id"]);
        var tipoEvento = (TipoEventoAcesso)Convert.ToInt32(reader["tipo_evento"]);
        var dataHora = Convert.ToDateTime(reader["data_hora"]);
        var id = Convert.ToInt32(reader["id_acesso"]);

        // Recupera o aluno completo.
        var aluno = await _alunoRepoFactory().ObterPorId(
            alunoId,
            cancellationToken);

        if (aluno == null)
        {
            throw new InfrastructureException(
                "ALUNO_NAO_ENCONTRADO",
                $"Não foi possível encontrar o aluno de ID {alunoId}.");
        }

        // Constrói a entidade de acesso.
        var resultado = AcessoAluno.Criar(
            id,
            aluno,
            tipoEvento,
            dataHora);

        if (resultado.IsFailure)
        {
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_ACESSO",
                string.Join(
                    ", ",
                    resultado.Notifications.Select(n => n.Mensagem)));
        }

        return resultado.Value!;
    }
}
