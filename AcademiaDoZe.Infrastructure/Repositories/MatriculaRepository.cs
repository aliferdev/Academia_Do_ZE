// Alifer Granemannn

using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class MatriculaRepository : BaseRepository, IMatriculaRepository
{
    private static string BaseSelectQuery => @"
        SELECT
            m.id_matricula,
            m.aluno_id,
            m.plano,
            m.data_inicio,
            m.data_fim,
            m.objetivo,
            m.restricao_medica,
            m.obs_restricao,
            m.laudo_medico,

            a.id_aluno,
            a.cpf,
            a.nome AS aluno_nome,
            a.nascimento,
            a.telefone,
            a.email,
            a.logradouro_id,
            a.numero,
            a.complemento,
            a.senha,
            a.foto,

            l.id_logradouro,
            l.cep,
            l.nome,            
            l.bairro,
            l.cidade,
            l.estado,
            l.pais

        FROM tb_matricula m
        INNER JOIN tb_aluno a
            ON m.aluno_id = a.id_aluno
        INNER JOIN tb_logradouro l
            ON a.logradouro_id = l.id_logradouro";

    public MatriculaRepository(
        string connectionString,
        DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    public async Task<Matricula?> ObterPorId(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query =
            $"{BaseSelectQuery} WHERE m.id_matricula = @Id";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Id",
                id);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            return await reader.ReadAsync(
                cancellationToken)
                ? Map(reader)
                : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_POR_ID",
                $"Erro ao obter matrícula por ID {id}: {ex.Message}",
                ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterTodos(
        CancellationToken cancellationToken = default)
    {
        var query =
            $"{BaseSelectQuery} ORDER BY m.data_inicio DESC";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            var matriculas = new List<Matricula>();

            while (await reader.ReadAsync(
                cancellationToken))
            {
                matriculas.Add(Map(reader));
            }

            return matriculas;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_TODOS",
                $"Erro ao obter todas as matrículas: {ex.Message}",
                ex);
        }
    }

    public async Task<Matricula> Adicionar(
        Matricula entity,
        CancellationToken cancellationToken = default)
    {
        var query = FormatInsertQuery(@"
            INSERT INTO tb_matricula
            (
                aluno_id,
                plano,
                data_inicio,
                data_fim,
                objetivo,
                restricao_medica,
                obs_restricao,
                laudo_medico
            )
            VALUES
            (
                @AlunoId,
                @Plano,
                @DataInicio,
                @DataFim,
                @Objetivo,
                @RestricaoMedica,
                @ObsRestricao,
                @LaudoMedico
            )");

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            AdicionarParametros(
                command,
                entity);

            var id =
                Convert.ToInt32(
                    await command.ExecuteScalarAsync(
                        cancellationToken));

            var matricula =
                await ObterPorId(
                    id,
                    cancellationToken);

            if (matricula is null)
            {
                throw new InfrastructureException(
                    "ERRO_OBTER_MATRICULA_INSERIDA",
                    $"Não foi possível obter a matrícula inserida com ID {id}.");
            }

            return matricula;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_ADICIONAR_MATRICULA",
                $"Erro ao adicionar matrícula: {ex.Message}",
                ex);
        }
    }

    public async Task<Matricula> Atualizar(
        Matricula entity,
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            UPDATE tb_matricula
            SET
                aluno_id = @AlunoId,
                plano = @Plano,
                data_inicio = @DataInicio,
                data_fim = @DataFim,
                objetivo = @Objetivo,
                restricao_medica = @RestricaoMedica,
                obs_restricao = @ObsRestricao,
                laudo_medico = @LaudoMedico
            WHERE id_matricula = @Id";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Id",
                entity.Id);

            AdicionarParametros(
                command,
                entity);

            var rowsAffected =
                await command.ExecuteNonQueryAsync(
                    cancellationToken);

            if (rowsAffected == 0)
            {
                throw new InfrastructureException(
                    "REGISTRO_NAO_ENCONTRADO",
                    $"Nenhuma matrícula encontrada com ID {entity.Id} para atualização.");
            }

            var matricula =
                await ObterPorId(
                    entity.Id,
                    cancellationToken);

            if (matricula is null)
            {
                throw new InfrastructureException(
                    "ERRO_OBTER_MATRICULA_ATUALIZADA",
                    $"Não foi possível obter a matrícula atualizada com ID {entity.Id}.");
            }

            return matricula;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_ATUALIZAR_MATRICULA",
                $"Erro ao atualizar matrícula ID {entity.Id}: {ex.Message}",
                ex);
        }
    }

    public async Task<bool> Remover(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string query =
            "DELETE FROM tb_matricula WHERE id_matricula = @Id";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Id",
                id);

            var rowsAffected =
                await command.ExecuteNonQueryAsync(
                    cancellationToken);

            return rowsAffected > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_REMOVER_MATRICULA",
                $"Erro ao remover matrícula ID {id}: {ex.Message}",
                ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterPorAluno(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var query =
            $"{BaseSelectQuery} WHERE m.aluno_id = @AlunoId ORDER BY m.data_inicio DESC";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            DbProvider.AddParameter(
                command,
                "@AlunoId",
                alunoId);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            var matriculas = new List<Matricula>();

            while (await reader.ReadAsync(
                cancellationToken))
            {
                matriculas.Add(Map(reader));
            }

            return matriculas;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_POR_ALUNO",
                $"Erro ao obter matrículas do aluno ID {alunoId}: {ex.Message}",
                ex);
        }
    }

    public async Task<Matricula?> ObterMatriculaAtivaPorAluno(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var query = $@"
            {BaseSelectQuery}
            WHERE m.aluno_id = @AlunoId
              AND m.data_inicio <= {GetCurrentDateFunction()}
              AND m.data_fim >= {GetCurrentDateFunction()}
            ORDER BY m.data_inicio DESC";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            DbProvider.AddParameter(
                command,
                "@AlunoId",
                alunoId);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            return await reader.ReadAsync(
                cancellationToken)
                ? Map(reader)
                : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_MATRICULA_ATIVA",
                $"Erro ao obter matrícula ativa do aluno ID {alunoId}: {ex.Message}",
                ex);
        }
    }

    public async Task<bool> PossuiMatriculaAtiva(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var query = $@"
            SELECT COUNT(1)
            FROM tb_matricula
            WHERE aluno_id = @AlunoId
              AND data_inicio <= {GetCurrentDateFunction()}
              AND data_fim >= {GetCurrentDateFunction()}";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            DbProvider.AddParameter(
                command,
                "@AlunoId",
                alunoId);

            var count =
                Convert.ToInt32(
                    await command.ExecuteScalarAsync(
                        cancellationToken));

            return count > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_POSSUI_MATRICULA_ATIVA",
                $"Erro ao verificar matrícula ativa do aluno ID {alunoId}: {ex.Message}",
                ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterAtivas(
        int alunoId = 0,
        CancellationToken cancellationToken = default)
    {
        var query = $@"
            {BaseSelectQuery}
            WHERE m.data_inicio <= {GetCurrentDateFunction()}
              AND m.data_fim >= {GetCurrentDateFunction()}";

        if (alunoId > 0)
        {
            query += " AND m.aluno_id = @AlunoId";
        }

        query += " ORDER BY m.data_fim";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            if (alunoId > 0)
            {
                DbProvider.AddParameter(
                    command,
                    "@AlunoId",
                    alunoId);
            }

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            var matriculas = new List<Matricula>();

            while (await reader.ReadAsync(
                cancellationToken))
            {
                matriculas.Add(Map(reader));
            }

            return matriculas;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_ATIVAS",
                $"Erro ao obter matrículas ativas: {ex.Message}",
                ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterVencendoEmDias(
        int dias,
        CancellationToken cancellationToken = default)
    {
        var dataAtual =
            GetCurrentDateFunction();

        var dataLimite =
            GetDateAddDaysExpression(
                dataAtual,
                "@Dias");

        var query = $@"
            {BaseSelectQuery}
            WHERE m.data_fim >= {dataAtual}
              AND m.data_fim <= {dataLimite}
            ORDER BY m.data_fim";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Dias",
                dias);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            var matriculas = new List<Matricula>();

            while (await reader.ReadAsync(
                cancellationToken))
            {
                matriculas.Add(Map(reader));
            }

            return matriculas;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_VENCENDO_EM_DIAS",
                $"Erro ao obter matrículas vencendo em {dias} dias: {ex.Message}",
                ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterPorPlano(
        MatriculaPlano plano,
        CancellationToken cancellationToken = default)
    {
        var query = $@"
    {BaseSelectQuery}
    WHERE m.plano = @Plano
    ORDER BY m.data_inicio DESC";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Plano",
                plano);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            var matriculas = new List<Matricula>();

            while (await reader.ReadAsync(
                cancellationToken))
            {
                matriculas.Add(Map(reader));
            }

            return matriculas;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_POR_PLANO",
                $"Erro ao obter matrículas pelo plano {plano}: {ex.Message}",
                ex);
        }
    }

    public static Matricula Map(
        DbDataReader reader)
    {
        try
        {
            var id =
                Convert.ToInt32(
                    reader["id_matricula"]);

            var aluno =
                AlunoRepository.Map(
                    reader,
                    "aluno_nome");

            var plano =
                (MatriculaPlano)Convert.ToInt32(
                    reader["plano"]);

            var dataInicio =
                DateOnly.FromDateTime(
                    Convert.ToDateTime(
                        reader["data_inicio"]));

            var objetivo =
                Convert.ToString(
                    reader["objetivo"])!;

            var restricoesMedicas =
                (MatriculaRestricoes)Convert.ToInt32(
                    reader["restricao_medica"]);

            var observacoesRestricoes =
                reader["obs_restricao"] == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(
                        reader["obs_restricao"])!;

            Arquivo? laudoMedico = null;

            if (reader["laudo_medico"] != DBNull.Value)
            {
                var laudoBytes =
                    (byte[])reader["laudo_medico"];

                var laudoResult =
                    Arquivo.Criar(
                        laudoBytes);

                if (laudoResult.IsFailure)
                {
                    throw new InfrastructureException(
                        "ERRO_MAPEAMENTO_LAUDO_MEDICO",
                        $"Erro ao criar laudo médico da matrícula ID {id}: " +
                        $"{string.Join(
                            ", ",
                            laudoResult.Notifications
                                .Select(n => n.Mensagem))}");
                }

                laudoMedico =
                    laudoResult.Value;
            }

            var result =
                Matricula.Criar(
                    id: id,
                    aluno: aluno,
                    plano: plano,
                    dataInicio: dataInicio,
                    objetivo: objetivo,
                    restricoesMedicas: restricoesMedicas,
                    laudoMedico: laudoMedico,
                    observacoesRestricoes: observacoesRestricoes);

            if (result.IsFailure)
            {
                throw new InfrastructureException(
                    "ERRO_DOMINIO_MAPEAMENTO",
                    $"Erro de domínio ao mapear matrícula ID {id}: " +
                    $"{string.Join(
                        ", ",
                        result.Notifications
                            .Select(n => n.Mensagem))}");
            }

            return result.Value!;
        }
        catch (InfrastructureException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_MATRICULA",
                $"Erro ao mapear dados da matrícula: {ex.Message}",
                ex);
        }
    }

    private static void AdicionarParametros(
        DbCommand command,
        Matricula entity)
    {
        DbProvider.AddParameter(
            command,
            "@AlunoId",
            entity.AlunoId);

        DbProvider.AddParameter(
            command,
            "@Plano",
            entity.Plano);

        DbProvider.AddParameter(
            command,
            "@DataInicio",
            entity.DataInicio);

        DbProvider.AddParameter(
            command,
            "@DataFim",
            entity.DataFim);

        DbProvider.AddParameter(
            command,
            "@Objetivo",
            entity.Objetivo);

        DbProvider.AddParameter(
            command,
            "@RestricaoMedica",
            entity.RestricoesMedicas);

        DbProvider.AddParameter(
            command,
            "@ObsRestricao",
            string.IsNullOrWhiteSpace(
                entity.ObservacoesRestricoes)
                ? DBNull.Value
                : entity.ObservacoesRestricoes);

        var parametroLaudo =
    DbProvider.AddParameter(
        command,
        "@LaudoMedico",
        entity.LaudoMedico?.Conteudo ?? (object)DBNull.Value);

        parametroLaudo.DbType =
            System.Data.DbType.Binary;
    }
}