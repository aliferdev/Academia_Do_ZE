// Alifer Granemann
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public sealed class MatriculaInfrastructureTests : TestBase, IAsyncLifetime
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly AlunoRepository _alunoRepo;
    private readonly MatriculaRepository _matriculaRepo;

    public MatriculaInfrastructureTests()
    {
        _logradouroRepo =
            new LogradouroRepository(
                ConnectionString,
                DatabaseType);

        _alunoRepo =
            new AlunoRepository(
                ConnectionString,
                DatabaseType);

        _matriculaRepo =
            new MatriculaRepository(
                ConnectionString,
                DatabaseType);
    }

    public Task InitializeAsync() =>
        Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _matriculaRepo.DisposeAsync();
        await _alunoRepo.DisposeAsync();
        await _logradouroRepo.DisposeAsync();
    }

    private async Task<Logradouro> CriarEInserirLogradouroAsync()
    {
        var logradouroResult =
            Logradouro.Criar(
                0,
                GerarCep(),
                NomeRua,
                NomeBairro,
                NomeCidade,
                "SC",
                "Brasil");

        if (logradouroResult.IsFailure)
        {
            throw new Exception(
                "Falha ao criar Logradouro: " +
                string.Join(
                    ", ",
                    logradouroResult.Notifications
                        .Select(n => n.Mensagem)));
        }

        return await _logradouroRepo.Adicionar(
            logradouroResult.Value!);
    }

    private async Task<Aluno> CriarEInserirAlunoAsync()
    {
        var logradouro =
            await CriarEInserirLogradouroAsync();

        var foto =
            Arquivo.Criar(
                new byte[] { 5, 6, 7, 8 }).Value!;

        var alunoResult =
            Aluno.Criar(
                id: 0,
                nome: "Alifer",
                cpf: GerarCpf(),
                dataNascimento:
                    new DateOnly(2000, 5, 15),
                telefone:
                    GerarTelefone(),
                email:
                    GerarEmail(),
                logradouro:
                    logradouro,
                numero:
                    "200",
                complemento:
                    "Granemann",
                senha:
                    $"Senha{SgbdSigla}123",
                foto:
                    foto);

        if (alunoResult.IsFailure)
        {
            throw new Exception(
                "Falha ao criar Aluno: " +
                string.Join(
                    ", ",
                    alunoResult.Notifications
                        .Select(n => n.Mensagem)));
        }

        return await _alunoRepo.Adicionar(
            alunoResult.Value!);
    }

    private async Task<Matricula> CriarEInserirMatriculaAsync(
        MatriculaPlano plano = MatriculaPlano.Mensal,
        DateOnly? dataInicio = null)
    {
        var aluno =
            await CriarEInserirAlunoAsync();

        var matriculaResult =
            Matricula.Criar(
                id: 0,
                aluno: aluno,
                plano: plano,
                dataInicio:
                    dataInicio ?? DateOnly.FromDateTime(DateTime.Today),
                objetivo:
                    aluno.Nome,
                restricoesMedicas:
                    MatriculaRestricoes.None,
                laudoMedico:
                    null,
                observacoesRestricoes:
                    $"Teste {SgbdSigla}");

        if (matriculaResult.IsFailure)
        {
            throw new Exception(
                "Falha ao criar Matrícula: " +
                string.Join(
                    ", ",
                    matriculaResult.Notifications
                        .Select(n => n.Mensagem)));
        }

        return await _matriculaRepo.Adicionar(
            matriculaResult.Value!);
    }

    [Fact]
    public async Task Matricula_Adicionar_E_ObterPorId_Sucesso()
    {
        var matricula =
            await CriarEInserirMatriculaAsync();

        Assert.NotNull(matricula);
        Assert.True(matricula.Id > 0);

        var obtida =
            await _matriculaRepo.ObterPorId(
                matricula.Id);

        Assert.NotNull(obtida);

        Assert.Equal(
            matricula.Id,
            obtida.Id);

        Assert.Equal(
            matricula.AlunoId,
            obtida.AlunoId);

        Assert.Equal(
            MatriculaPlano.Mensal,
            obtida.Plano);

        Assert.Equal(
            "Alifer",
            obtida.Objetivo);

        Assert.Contains(
            SgbdSigla,
            obtida.ObservacoesRestricoes);
    }

    [Fact]
    public async Task Matricula_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtida =
            await _matriculaRepo.ObterPorId(
                999999);

        Assert.Null(obtida);
    }

    [Fact]
    public async Task Matricula_ObterTodos_Sucesso()
    {
        await CriarEInserirMatriculaAsync();

        var todas =
            await _matriculaRepo.ObterTodos();

        Assert.NotNull(todas);
        Assert.NotEmpty(todas);
    }

    [Fact]
    public async Task Matricula_Atualizar_Sucesso()
    {
        var matricula =
            await CriarEInserirMatriculaAsync();

        var aluno =
            await _alunoRepo.ObterPorId(
                matricula.AlunoId);

        Assert.NotNull(aluno);

        var atualizadaResult =
            Matricula.Criar(
                id:
                    matricula.Id,
                aluno:
                    aluno,
                plano:
                    MatriculaPlano.Trimestral,
                dataInicio:
                    matricula.DataInicio,
                objetivo:
                    aluno.Nome,
                restricoesMedicas:
                    MatriculaRestricoes.None,
                laudoMedico:
                    null,
                observacoesRestricoes:
                    $"Atualizado {SgbdSigla}");

        if (atualizadaResult.IsFailure)
        {
            throw new Exception(
                "Falha ao criar Matrícula atualizada: " +
                string.Join(
                    ", ",
                    atualizadaResult.Notifications
                        .Select(n => n.Mensagem)));
        }

        var resultado =
            await _matriculaRepo.Atualizar(
                atualizadaResult.Value!);

        Assert.NotNull(resultado);

        Assert.Equal(
            matricula.Id,
            resultado.Id);

        Assert.Equal(
            MatriculaPlano.Trimestral,
            resultado.Plano);

        Assert.Equal(
            $"Atualizado {SgbdSigla}",
            resultado.ObservacoesRestricoes);

        var noBanco =
            await _matriculaRepo.ObterPorId(
                matricula.Id);

        Assert.NotNull(noBanco);

        Assert.Equal(
            MatriculaPlano.Trimestral,
            noBanco.Plano);
    }

    [Fact]
    public async Task Matricula_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var aluno =
            await CriarEInserirAlunoAsync();

        var matriculaResult =
            Matricula.Criar(
                id:
                    999999,
                aluno:
                    aluno,
                plano:
                    MatriculaPlano.Mensal,
                dataInicio:
                    DateOnly.FromDateTime(
                        DateTime.Today),
                objetivo:
                    aluno.Nome,
                restricoesMedicas:
                    MatriculaRestricoes.None,
                laudoMedico:
                    null,
                observacoesRestricoes:
                    $"Teste {SgbdSigla}");

        if (matriculaResult.IsFailure)
        {
            throw new Exception(
                "Falha ao criar Matrícula: " +
                string.Join(
                    ", ",
                    matriculaResult.Notifications
                        .Select(n => n.Mensagem)));
        }

        var ex =
            await Assert.ThrowsAsync<InfrastructureException>(
                () =>
                    _matriculaRepo.Atualizar(
                        matriculaResult.Value!));

        Assert.Equal(
            "REGISTRO_NAO_ENCONTRADO",
            ex.ErrorCode);
    }

    [Fact]
    public async Task Matricula_Remover_Sucesso()
    {
        var matricula =
            await CriarEInserirMatriculaAsync();

        var removida =
            await _matriculaRepo.Remover(
                matricula.Id);

        Assert.True(removida);

        var noBanco =
            await _matriculaRepo.ObterPorId(
                matricula.Id);

        Assert.Null(noBanco);
    }

    [Fact]
    public async Task Matricula_Remover_RetornaFalseQuandoInexistente()
    {
        var removida =
            await _matriculaRepo.Remover(
                999999);

        Assert.False(removida);
    }

    [Fact]
    public async Task Matricula_ObterPorAluno_Sucesso()
    {
        var matricula =
            await CriarEInserirMatriculaAsync();

        var resultados =
            await _matriculaRepo.ObterPorAluno(
                matricula.AlunoId);

        Assert.NotNull(resultados);

        Assert.Contains(
            resultados,
            m => m.Id == matricula.Id);
    }

    [Fact]
    public async Task Matricula_ObterMatriculaAtivaPorAluno_Sucesso()
    {
        var matricula =
            await CriarEInserirMatriculaAsync(
                plano:
                    MatriculaPlano.Mensal,
                dataInicio:
                    DateOnly.FromDateTime(
                        DateTime.Today));

        var resultado =
            await _matriculaRepo.ObterMatriculaAtivaPorAluno(
                matricula.AlunoId);

        Assert.NotNull(resultado);

        Assert.Equal(
            matricula.Id,
            resultado.Id);

        Assert.Equal(
            matricula.AlunoId,
            resultado.AlunoId);
    }

    [Fact]
    public async Task Matricula_PossuiMatriculaAtiva_Sucesso()
    {
        var matricula =
            await CriarEInserirMatriculaAsync(
                plano:
                    MatriculaPlano.Mensal,
                dataInicio:
                    DateOnly.FromDateTime(
                        DateTime.Today));

        var possui =
            await _matriculaRepo.PossuiMatriculaAtiva(
                matricula.AlunoId);

        Assert.True(possui);

        var alunoInexistente =
            await _alunoRepo.ObterPorId(
                999999);

        Assert.Null(alunoInexistente);

        var naoPossui =
            await _matriculaRepo.PossuiMatriculaAtiva(
                999999);

        Assert.False(naoPossui);
    }

    [Fact]
    public async Task Matricula_ObterAtivas_Sucesso()
    {
        var matricula =
            await CriarEInserirMatriculaAsync(
                plano:
                    MatriculaPlano.Mensal,
                dataInicio:
                    DateOnly.FromDateTime(
                        DateTime.Today));

        var todas =
            await _matriculaRepo.ObterAtivas();

        Assert.NotNull(todas);

        Assert.Contains(
            todas,
            m => m.Id == matricula.Id);

        var doAluno =
            await _matriculaRepo.ObterAtivas(
                matricula.AlunoId);

        Assert.Contains(
            doAluno,
            m => m.Id == matricula.Id);
    }

    [Fact]
    public async Task Matricula_ObterVencendoEmDias_Sucesso()
    {
        var matricula =
            await CriarEInserirMatriculaAsync(
                plano:
                    MatriculaPlano.Mensal,
                dataInicio:
                    DateOnly.FromDateTime(
                        DateTime.Today));

        var hoje =
            DateOnly.FromDateTime(
                DateTime.Today);

        var diasAteVencimento =
            matricula.DataFim.DayNumber -
            hoje.DayNumber;

        var resultados =
            await _matriculaRepo.ObterVencendoEmDias(
                diasAteVencimento);

        Assert.NotNull(resultados);

        Assert.Contains(
            resultados,
            m => m.Id == matricula.Id);
    }

    [Fact]
    public async Task Matricula_ObterPorPlano_Sucesso()
    {
        var matricula =
            await CriarEInserirMatriculaAsync(
                plano:
                    MatriculaPlano.Semestral);

        var resultados =
            await _matriculaRepo.ObterPorPlano(
                MatriculaPlano.Semestral);

        Assert.NotNull(resultados);

        Assert.Contains(
            resultados,
            m => m.Id == matricula.Id);
    }
}