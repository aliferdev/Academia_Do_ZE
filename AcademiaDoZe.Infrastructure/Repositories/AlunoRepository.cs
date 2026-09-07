// Alifer Granemann
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AlunoRepository : BaseRepository, IAlunoRepository
{
    private static string BaseSelectQuery => @"
        SELECT
            a.id_aluno,
            a.cpf,
            a.nome,
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
        FROM tb_aluno a
        INNER JOIN tb_logradouro l
            ON a.logradouro_id = l.id_logradouro";

    public AlunoRepository(
        string connectionString,
        DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    public async Task<Aluno?> ObterPorId(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query =
            $"{BaseSelectQuery} WHERE a.id_aluno = @Id";

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
                $"Erro ao obter aluno por ID {id}: {ex.Message}",
                ex);
        }
    }

    public async Task<IEnumerable<Aluno>> ObterTodos(
        CancellationToken cancellationToken = default)
    {
        var query =
            $"{BaseSelectQuery} ORDER BY a.nome";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            var alunos = new List<Aluno>();

            while (await reader.ReadAsync(
                cancellationToken))
            {
                alunos.Add(Map(reader));
            }

            return alunos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_TODOS",
                $"Erro ao obter todos os alunos: {ex.Message}",
                ex);
        }
    }

    public async Task<Aluno> Adicionar(
        Aluno entity,
        CancellationToken cancellationToken = default)
    {
        var query = FormatInsertQuery(@"
            INSERT INTO tb_aluno
            (
                cpf,
                nome,
                nascimento,
                telefone,
                email,
                logradouro_id,
                numero,
                complemento,
                senha,
                foto
            )
            VALUES
            (
                @Cpf,
                @Nome,
                @Nascimento,
                @Telefone,
                @Email,
                @LogradouroId,
                @Numero,
                @Complemento,
                @Senha,
                @Foto
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

            var aluno =
                await ObterPorId(
                    id,
                    cancellationToken);

            if (aluno is null)
            {
                throw new InfrastructureException(
                    "ERRO_OBTER_ALUNO_INSERIDO",
                    $"Não foi possível obter o aluno inserido com ID {id}.");
            }

            return aluno;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_ADICIONAR_ALUNO",
                $"Erro ao adicionar aluno: {ex.Message}",
                ex);
        }
    }

    public async Task<Aluno> Atualizar(
        Aluno entity,
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            UPDATE tb_aluno
            SET
                cpf = @Cpf,
                nome = @Nome,
                nascimento = @Nascimento,
                telefone = @Telefone,
                email = @Email,
                logradouro_id = @LogradouroId,
                numero = @Numero,
                complemento = @Complemento,
                senha = @Senha,
                foto = @Foto
            WHERE id_aluno = @Id";

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
                    $"Nenhum aluno encontrado com ID {entity.Id} para atualização.");
            }

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_ATUALIZAR_ALUNO",
                $"Erro ao atualizar aluno ID {entity.Id}: {ex.Message}",
                ex);
        }
    }

    public async Task<bool> Remover(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string query =
            "DELETE FROM tb_aluno WHERE id_aluno = @Id";

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
                "ERRO_REMOVER_ALUNO",
                $"Erro ao remover aluno ID {id}: {ex.Message}",
                ex);
        }
    }

    public async Task<Aluno?> ObterPorCpf(
        Cpf cpf,
        CancellationToken cancellationToken = default)
    {
        var query =
            $"{BaseSelectQuery} WHERE a.cpf = @Cpf";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Cpf",
                cpf.Valor);

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
                "ERRO_OBTER_POR_CPF",
                $"Erro ao obter aluno por CPF: {ex.Message}",
                ex);
        }
    }

    public async Task<Aluno?> ObterPorEmail(
        Email email,
        CancellationToken cancellationToken = default)
    {
        var query =
            $"{BaseSelectQuery} WHERE a.email = @Email";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Email",
                email.Valor);

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
                "ERRO_OBTER_POR_EMAIL",
                $"Erro ao obter aluno por e-mail: {ex.Message}",
                ex);
        }
    }

    public async Task<bool> CpfJaExiste(
        Cpf cpf,
        int? id = null,
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT COUNT(1)
            FROM tb_aluno
            WHERE cpf = @Cpf
              AND (@Id IS NULL OR id_aluno <> @Id)";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Cpf",
                cpf.Valor);

            DbProvider.AddParameter(
                command,
                "@Id",
                id.HasValue
                    ? id.Value
                    : DBNull.Value);

            var count =
                Convert.ToInt32(
                    await command.ExecuteScalarAsync(
                        cancellationToken));

            return count > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_VERIFICAR_CPF",
                $"Erro ao verificar existência de CPF: {ex.Message}",
                ex);
        }
    }

    public async Task<bool> EmailJaExiste(
        Email email,
        int? id = null,
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT COUNT(1)
            FROM tb_aluno
            WHERE email = @Email
              AND (@Id IS NULL OR id_aluno <> @Id)";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Email",
                email.Valor);

            DbProvider.AddParameter(
                command,
                "@Id",
                id.HasValue
                    ? id.Value
                    : DBNull.Value);

            var count =
                Convert.ToInt32(
                    await command.ExecuteScalarAsync(
                        cancellationToken));

            return count > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_VERIFICAR_EMAIL",
                $"Erro ao verificar existência de e-mail: {ex.Message}",
                ex);
        }
    }

    public async Task<IEnumerable<Aluno>> ObterPorNome(
        string nome,
        CancellationToken cancellationToken = default)
    {
        var filtro =
            GetCaseInsensitiveEqualsExpression(
                "a.nome",
                "@Nome");

        var query =
            $"{BaseSelectQuery} WHERE {filtro} ORDER BY a.nome";

        try
        {
            await using var command =
                await CreateCommandAsync(
                    query,
                    cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Nome",
                nome);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            var alunos = new List<Aluno>();

            while (await reader.ReadAsync(
                cancellationToken))
            {
                alunos.Add(Map(reader));
            }

            return alunos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_POR_NOME",
                $"Erro ao obter alunos por nome: {ex.Message}",
                ex);
        }
    }

    public async Task<bool> TrocarSenha(
        int id,
        Senha novaSenha,
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            UPDATE tb_aluno
            SET senha = @Senha
            WHERE id_aluno = @Id";

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

            DbProvider.AddParameter(
                command,
                "@Senha",
                novaSenha.Valor);

            var rowsAffected =
                await command.ExecuteNonQueryAsync(
                    cancellationToken);

            return rowsAffected > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_TROCAR_SENHA",
                $"Erro ao trocar senha do aluno ID {id}: {ex.Message}",
                ex);
        }
    }

    public static Aluno Map(
        DbDataReader reader,
        string nomeColumn = "nome")
    {
        try
        {
            var id =
                Convert.ToInt32(
                    reader["id_aluno"]);

            var cpf =
                Convert.ToString(
                    reader["cpf"])!;

            var nome =
                Convert.ToString(
                    reader[nomeColumn])!;

            var dataNascimento =
                DateOnly.FromDateTime(
                    Convert.ToDateTime(
                        reader["nascimento"]));

            var telefone =
                Convert.ToString(
                    reader["telefone"])!;

            var email =
                Convert.ToString(
                    reader["email"])!;

            var numero =
                Convert.ToString(
                    reader["numero"])!;

            var complemento =
                reader["complemento"] == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(
                        reader["complemento"])!;

            var senha =
                Convert.ToString(
                    reader["senha"])!;

            byte[]? fotoBytes =
                reader["foto"] == DBNull.Value
                    ? null
                    : (byte[])reader["foto"];

            Arquivo? foto = null;

            if (fotoBytes != null)
            {
                var fotoResult =
                    Arquivo.Criar(fotoBytes);

                if (fotoResult.IsFailure)
                {
                    throw new InfrastructureException(
                        "ERRO_MAPEAMENTO_FOTO",
                        $"Erro ao criar foto do aluno ID {id}: " +
                        $"{string.Join(
                            ", ",
                            fotoResult.Notifications
                                .Select(n => n.Mensagem))}");
                }

                foto = fotoResult.Value;
            }

            var logradouro =
                LogradouroRepository.Map(reader);

            var result =
                Aluno.Criar(
                    id: id,
                    nome: nome,
                    cpf: cpf,
                    dataNascimento: dataNascimento,
                    telefone: telefone,
                    email: email,
                    logradouro: logradouro,
                    numero: numero,
                    complemento: complemento,
                    senha: senha,
                    foto: foto!);

            if (result.IsFailure)
            {
                throw new InfrastructureException(
                    "ERRO_DOMINIO_MAPEAMENTO",
                    $"Erro de domínio ao mapear aluno ID {id}: " +
                    $"{string.Join(
                        ", ",
                        result.Notifications
                            .Select(n => n.Mensagem))}");
            }

            return result.Value!;
        }
        catch (Exception ex)
            when (ex is not InfrastructureException)
        {
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_ALUNO",
                $"Erro ao mapear dados do aluno: {ex.Message}",
                ex);
        }
    }

    private static void AdicionarParametros(
        DbCommand command,
        Aluno entity)
    {
        DbProvider.AddParameter(
            command,
            "@Cpf",
            entity.Cpf.Valor);

        DbProvider.AddParameter(
            command,
            "@Nome",
            entity.Nome);

        DbProvider.AddParameter(
            command,
            "@Nascimento",
            entity.DataNascimento);

        DbProvider.AddParameter(
            command,
            "@Telefone",
            entity.Telefone.Valor);

        DbProvider.AddParameter(
            command,
            "@Email",
            entity.Email.Valor);

        DbProvider.AddParameter(
            command,
            "@LogradouroId",
            entity.Endereco.LogradouroId);

        DbProvider.AddParameter(
            command,
            "@Numero",
            entity.Endereco.Numero);

        DbProvider.AddParameter(
            command,
            "@Complemento",
            string.IsNullOrWhiteSpace(
                entity.Endereco.Complemento)
                ? DBNull.Value
                : entity.Endereco.Complemento);

        DbProvider.AddParameter(
            command,
            "@Senha",
            entity.Senha.Valor);

        DbProvider.AddParameter(
    command,
    "@Foto",
    entity.Foto is null
        ? DBNull.Value
        : entity.Foto.Conteudo);
    }
}