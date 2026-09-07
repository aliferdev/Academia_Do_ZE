//Alifer Granemann
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class ColaboradorRepository : BaseRepository, IColaboradorRepository
{
    private static string BaseSelectQuery => @"
        SELECT
            c.id_colaborador,
            c.cpf,
            c.nome,
            c.nascimento,
            c.telefone,
            c.email,
            c.logradouro_id,
            c.numero,
            c.complemento,
            c.senha,
            c.foto,
            c.data_admissao,
            c.tipo,
            c.vinculo,
            l.id_logradouro,
            l.cep,
            l.nome,
            l.bairro,
            l.cidade,
            l.estado,
            l.pais
        FROM tb_colaborador c
        INNER JOIN tb_logradouro l
            ON c.logradouro_id = l.id_logradouro";

    public ColaboradorRepository(
        string connectionString,
        DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    public async Task<Colaborador?> ObterPorId(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query =
            $"{BaseSelectQuery} WHERE c.id_colaborador = @Id";

        try
        {
            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            DbProvider.AddParameter(command, "@Id", id);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            return await reader.ReadAsync(cancellationToken)
                ? Map(reader)
                : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_POR_ID",
                $"Erro ao obter colaborador por ID {id}: {ex.Message}",
                ex);
        }
    }

    public async Task<IEnumerable<Colaborador>> ObterTodos(
        CancellationToken cancellationToken = default)
    {
        var query =
            $"{BaseSelectQuery} ORDER BY c.nome";

        try
        {
            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            var colaboradores = new List<Colaborador>();

            while (await reader.ReadAsync(cancellationToken))
            {
                colaboradores.Add(Map(reader));
            }

            return colaboradores;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_TODOS",
                $"Erro ao obter todos os colaboradores: {ex.Message}",
                ex);
        }
    }

    public async Task<Colaborador> Adicionar(
        Colaborador entity,
        CancellationToken cancellationToken = default)
    {
        var query = FormatInsertQuery(@"
            INSERT INTO tb_colaborador
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
                foto,
                data_admissao,
                tipo,
                vinculo
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
                @Foto,
                @DataAdmissao,
                @Tipo,
                @Vinculo
            )");

        try
        {
            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            AdicionarParametros(command, entity);

            var id = Convert.ToInt32(
                await command.ExecuteScalarAsync(cancellationToken));

            var colaborador =
                await ObterPorId(id, cancellationToken);

            if (colaborador is null)
            {
                throw new InfrastructureException(
                    "ERRO_OBTER_COLABORADOR_INSERIDO",
                    $"Não foi possível obter o colaborador inserido com ID {id}.");
            }

            return colaborador;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_ADICIONAR_COLABORADOR",
                $"Erro ao adicionar colaborador: {ex.Message}",
                ex);
        }
    }

    public async Task<Colaborador> Atualizar(
        Colaborador entity,
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            UPDATE tb_colaborador
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
                foto = @Foto,
                data_admissao = @DataAdmissao,
                tipo = @Tipo,
                vinculo = @Vinculo
            WHERE id_colaborador = @Id";

        try
        {
            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Id",
                entity.Id);

            AdicionarParametros(command, entity);

            var rowsAffected =
                await command.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                throw new InfrastructureException(
                    "COLABORADOR_NAO_ENCONTRADO",
                    $"Colaborador com ID {entity.Id} não encontrado.");
            }

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_ATUALIZAR_COLABORADOR",
                $"Erro ao atualizar colaborador ID {entity.Id}: {ex.Message}",
                ex);
        }
    }

    public async Task<bool> Remover(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            DELETE FROM tb_colaborador
            WHERE id_colaborador = @Id";

        try
        {
            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Id",
                id);

            var rowsAffected =
                await command.ExecuteNonQueryAsync(cancellationToken);

            return rowsAffected > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_REMOVER_COLABORADOR",
                $"Erro ao remover colaborador ID {id}: {ex.Message}",
                ex);
        }
    }

    public async Task<Colaborador?> ObterPorCpf(
        Cpf cpf,
        CancellationToken cancellationToken = default)
    {
        var query =
            $"{BaseSelectQuery} WHERE c.cpf = @Cpf";

        try
        {
            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Cpf",
                cpf.Valor);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            return await reader.ReadAsync(cancellationToken)
                ? Map(reader)
                : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_POR_CPF",
                $"Erro ao obter colaborador por CPF: {ex.Message}",
                ex);
        }
    }

    public async Task<Colaborador?> ObterPorEmail(
        Email email,
        CancellationToken cancellationToken = default)
    {
        var query =
            $"{BaseSelectQuery} WHERE c.email = @Email";

        try
        {
            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Email",
                email.Valor);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            return await reader.ReadAsync(cancellationToken)
                ? Map(reader)
                : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_POR_EMAIL",
                $"Erro ao obter colaborador por e-mail: {ex.Message}",
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
            FROM tb_colaborador
            WHERE cpf = @Cpf
              AND (@Id IS NULL OR id_colaborador <> @Id)";

        try
        {
            await using var command =
                await CreateCommandAsync(query, cancellationToken);

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

            var count = Convert.ToInt32(
                await command.ExecuteScalarAsync(cancellationToken));

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
            FROM tb_colaborador
            WHERE email = @Email
              AND (@Id IS NULL OR id_colaborador <> @Id)";

        try
        {
            await using var command =
                await CreateCommandAsync(query, cancellationToken);

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

            var count = Convert.ToInt32(
                await command.ExecuteScalarAsync(cancellationToken));

            return count > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_VERIFICAR_EMAIL",
                $"Erro ao verificar existência de Email: {ex.Message}",
                ex);
        }
    }

    public async Task<IEnumerable<Colaborador>> ObterPorTipo(
        ColaboradorTipo tipo,
        CancellationToken cancellationToken = default)
    {
        var query =
            $"{BaseSelectQuery} WHERE c.tipo = @Tipo ORDER BY c.nome";

        try
        {
            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Tipo",
                (int)tipo);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            var colaboradores = new List<Colaborador>();

            while (await reader.ReadAsync(cancellationToken))
            {
                colaboradores.Add(Map(reader));
            }

            return colaboradores;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_POR_TIPO",
                $"Erro ao obter colaboradores por tipo {tipo}: {ex.Message}",
                ex);
        }
    }

    public async Task<IEnumerable<Colaborador>> ObterPorVinculo(
        ColaboradorVinculo vinculo,
        CancellationToken cancellationToken = default)
    {
        var query =
            $"{BaseSelectQuery} WHERE c.vinculo = @Vinculo ORDER BY c.nome";

        try
        {
            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Vinculo",
                (int)vinculo);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            var colaboradores = new List<Colaborador>();

            while (await reader.ReadAsync(cancellationToken))
            {
                colaboradores.Add(Map(reader));
            }

            return colaboradores;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_OBTER_POR_VINCULO",
                $"Erro ao obter colaboradores por vínculo {vinculo}: {ex.Message}",
                ex);
        }
    }

    public async Task<bool> TrocarSenha(
        int id,
        Senha novaSenha,
        CancellationToken cancellationToken = default)
    {
        const string query = @"
            UPDATE tb_colaborador
            SET senha = @Senha
            WHERE id_colaborador = @Id";

        try
        {
            await using var command =
                await CreateCommandAsync(query, cancellationToken);

            DbProvider.AddParameter(
                command,
                "@Id",
                id);

            DbProvider.AddParameter(
                command,
                "@Senha",
                novaSenha.Valor);

            var rowsAffected =
                await command.ExecuteNonQueryAsync(cancellationToken);

            return rowsAffected > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_TROCAR_SENHA",
                $"Erro ao trocar senha do colaborador ID {id}: {ex.Message}",
                ex);
        }
    }

    public static Colaborador Map(
        DbDataReader reader,
        string nomeColumn = "nome")
    {
        try
        {
            var id =
                Convert.ToInt32(reader["id_colaborador"]);

            var cpf =
                Convert.ToString(reader["cpf"])!;

            var nome =
                Convert.ToString(reader[nomeColumn])!;

            var dataNascimento =
                DateOnly.FromDateTime(
                    Convert.ToDateTime(reader["nascimento"]));

            var telefone =
                Convert.ToString(reader["telefone"])!;

            var email =
                Convert.ToString(reader["email"])!;

            var numero =
                Convert.ToString(reader["numero"])!;

            var complemento =
                reader["complemento"] == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(reader["complemento"])!;

            var senha =
                Convert.ToString(reader["senha"])!;

            var dataAdmissao =
                DateOnly.FromDateTime(
                    Convert.ToDateTime(reader["data_admissao"]));

            var tipo =
                (ColaboradorTipo)
                Convert.ToInt32(reader["tipo"]);

            var vinculo =
                (ColaboradorVinculo)
                Convert.ToInt32(reader["vinculo"]);

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
                        $"Erro ao criar foto do colaborador ID {id}: " +
                        $"{string.Join(", ", fotoResult.Notifications.Select(n => n.Mensagem))}");
                }

                foto = fotoResult.Value;
            }

            var logradouro =
                LogradouroRepository.Map(reader);

            var result =
                Colaborador.Criar(
                    id: id,
                    nome: nome,
                    cpf: cpf,
                    dataNascimento: dataNascimento,
                    telefone: telefone,
                    email: email,
                    endereco: logradouro,
                    numero: numero,
                    complemento: complemento,
                    senha: senha,
                    foto: foto!,
                    dataAdmissao: dataAdmissao,
                    tipo: tipo,
                    vinculo: vinculo
                );

            if (result.IsFailure)
            {
                throw new InfrastructureException(
                    "ERRO_DOMINIO_MAPEAMENTO",
                    $"Erro de domínio ao mapear colaborador ID {id}: " +
                    $"{string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
            }

            return result.Value!;
        }
        catch (Exception ex)
            when (ex is not InfrastructureException)
        {
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_COLABORADOR",
                $"Erro ao mapear dados do colaborador: {ex.Message}",
                ex);
        }
    }

    private static void AdicionarParametros(
        DbCommand command,
        Colaborador entity)
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
            string.IsNullOrWhiteSpace(entity.Endereco.Complemento)
                ? DBNull.Value
                : entity.Endereco.Complemento);

        DbProvider.AddParameter(
            command,
            "@Senha",
            entity.Senha.Valor);

        DbProvider.AddParameter(
            command,
            "@Foto",
            entity.Foto.Conteudo);

        DbProvider.AddParameter(
            command,
            "@DataAdmissao",
            entity.DataAdmissao);

        DbProvider.AddParameter(
            command,
            "@Tipo",
            (int)entity.Tipo);

        DbProvider.AddParameter(
            command,
            "@Vinculo",
            (int)entity.Vinculo);
    }
}