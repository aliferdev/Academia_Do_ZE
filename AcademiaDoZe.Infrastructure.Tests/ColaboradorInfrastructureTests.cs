// Alifer Granemann

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public sealed class ColaboradorInfrastructureTests : TestBase, IAsyncLifetime
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly ColaboradorRepository _colaboradorRepo;

    public ColaboradorInfrastructureTests()
    {
        _logradouroRepo =
            new LogradouroRepository(ConnectionString, DatabaseType);

        _colaboradorRepo =
            new ColaboradorRepository(ConnectionString, DatabaseType);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _colaboradorRepo.DisposeAsync();
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
                $"{string.Join(
                    ", ",
                    logradouroResult.Notifications
                        .Select(n => n.Mensagem))}");
        }

        return await _logradouroRepo.Adicionar(
            logradouroResult.Value!);
    }

    private async Task<Colaborador> CriarEInserirColaboradorAsync()
    {
        var logradouro =
            await CriarEInserirLogradouroAsync();

        var foto =
            Arquivo.Criar(
                new byte[] { 5, 6, 7, 8 }).Value!;

        var colaboradorResult =
            Colaborador.Criar(
                id: 0,
                nome: "Alifer",
                cpf: GerarCpf(),
                dataNascimento:
                    new DateOnly(1995, 5, 15),
                telefone: GerarTelefone(),
                email: GerarEmail(),
                endereco: logradouro,
                numero: "200",
                complemento: "Granemann",
                senha: $"Senha{SgbdSigla}123",
                foto: foto,
                dataAdmissao:
                    new DateOnly(2023, 1, 1),
                tipo: ColaboradorTipo.Instrutor,
                vinculo: ColaboradorVinculo.CLT
            );

        if (colaboradorResult.IsFailure)
        {
            throw new Exception(
                "Falha ao criar Colaborador: " +
                $"{string.Join(
                    ", ",
                    colaboradorResult.Notifications
                        .Select(n => n.Mensagem))}");
        }

        return await _colaboradorRepo.Adicionar(
            colaboradorResult.Value!);
    }

    [Fact]
    public async Task Colaborador_Adicionar_E_ObterPorId_Sucesso()
    {
        var colaborador =
            await CriarEInserirColaboradorAsync();

        Assert.NotNull(colaborador);
        Assert.True(colaborador.Id > 0);

        var obtido =
            await _colaboradorRepo.ObterPorId(
                colaborador.Id);

        Assert.NotNull(obtido);

        Assert.Equal(
            colaborador.Id,
            obtido.Id);

        Assert.Equal(
            colaborador.Nome,
            obtido.Nome);

        Assert.Equal(
            "Granemann",
            obtido.Endereco.Complemento);

        Assert.Equal(
            colaborador.Cpf.Valor,
            obtido.Cpf.Valor);

        Assert.Equal(
            colaborador.Email.Valor,
            obtido.Email.Valor);

        Assert.Equal(
            colaborador.Tipo,
            obtido.Tipo);

        Assert.Equal(
            colaborador.Vinculo,
            obtido.Vinculo);

        Assert.NotNull(obtido.Endereco);

        Assert.Equal(
            colaborador.Endereco.LogradouroId,
            obtido.Endereco.LogradouroId);
    }

    [Fact]
    public async Task Colaborador_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido =
            await _colaboradorRepo.ObterPorId(999999);

        Assert.Null(obtido);
    }

    [Fact]
    public async Task Colaborador_ObterTodos_Sucesso()
    {
        await CriarEInserirColaboradorAsync();

        var todos =
            await _colaboradorRepo.ObterTodos();

        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact]
    public async Task Colaborador_Atualizar_Sucesso()
    {
        var colaborador =
            await CriarEInserirColaboradorAsync();

        var novoNome =
            "Alifer";

        var atualizadoResult =
            Colaborador.Criar(
                id: colaborador.Id,
                nome: novoNome,
                cpf: colaborador.Cpf.Valor,
                dataNascimento:
                    colaborador.DataNascimento,
                telefone:
                    colaborador.Telefone.Valor,
                email:
                    colaborador.Email.Valor,
                endereco:
                    await ObterLogradouroAsync(
                        colaborador.Endereco.LogradouroId),
                numero:
                    colaborador.Endereco.Numero,
                complemento:
                    "Granemann",
                senha:
                    colaborador.Senha.Valor,
                foto:
                    colaborador.Foto,
                dataAdmissao:
                    colaborador.DataAdmissao,
                tipo:
                    ColaboradorTipo.Administrador,
                vinculo:
                    colaborador.Vinculo
            );

        if (atualizadoResult.IsFailure)
        {
            throw new Exception(
                "Falha ao criar colaborador atualizado: " +
                $"{string.Join(
                    ", ",
                    atualizadoResult.Notifications
                        .Select(n => n.Mensagem))}");
        }

        var resultado =
            await _colaboradorRepo.Atualizar(
                atualizadoResult.Value!);

        Assert.NotNull(resultado);

        Assert.Equal(
            colaborador.Id,
            resultado.Id);

        var noBanco =
            await _colaboradorRepo.ObterPorId(
                colaborador.Id);

        Assert.NotNull(noBanco);

        Assert.Equal(
            novoNome,
            noBanco.Nome);

        Assert.Equal(
            "Granemann",
            noBanco.Endereco.Complemento);

        Assert.Equal(
            ColaboradorTipo.Administrador,
            noBanco.Tipo);
    }

    [Fact]
    public async Task Colaborador_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var logradouro =
            await CriarEInserirLogradouroAsync();

        var foto =
            Arquivo.Criar(
                new byte[] { 1, 2 }).Value!;

        var colaboradorResult =
            Colaborador.Criar(
                id: 999999,
                nome: "Alifer",
                cpf: GerarCpf(),
                dataNascimento:
                    new DateOnly(1995, 5, 15),
                telefone: GerarTelefone(),
                email: GerarEmail(),
                endereco: logradouro,
                numero: "200",
                complemento: "Granemann",
                senha: $"Senha{SgbdSigla}123",
                foto: foto,
                dataAdmissao:
                    new DateOnly(2023, 1, 1),
                tipo: ColaboradorTipo.Instrutor,
                vinculo: ColaboradorVinculo.CLT
            );

        if (colaboradorResult.IsFailure)
        {
            throw new Exception(
                "Falha ao criar Colaborador: " +
                $"{string.Join(
                    ", ",
                    colaboradorResult.Notifications
                        .Select(n => n.Mensagem))}");
        }

        var ex =
            await Assert.ThrowsAsync<InfrastructureException>(
                () =>
                    _colaboradorRepo.Atualizar(
                        colaboradorResult.Value!));

        Assert.Equal(
            "COLABORADOR_NAO_ENCONTRADO",
            ex.ErrorCode);
    }

    [Fact]
    public async Task Colaborador_Remover_Sucesso()
    {
        var colaborador =
            await CriarEInserirColaboradorAsync();

        var removido =
            await _colaboradorRepo.Remover(
                colaborador.Id);

        Assert.True(removido);

        var noBanco =
            await _colaboradorRepo.ObterPorId(
                colaborador.Id);

        Assert.Null(noBanco);
    }

    [Fact]
    public async Task Colaborador_Remover_RetornaFalseQuandoInexistente()
    {
        var removido =
            await _colaboradorRepo.Remover(999999);

        Assert.False(removido);
    }

    [Fact]
    public async Task Colaborador_ObterPorCpf_SucessoENulo()
    {
        var colaborador =
            await CriarEInserirColaboradorAsync();

        var obtido =
            await _colaboradorRepo.ObterPorCpf(
                colaborador.Cpf);

        Assert.NotNull(obtido);

        Assert.Equal(
            colaborador.Id,
            obtido.Id);

        var cpfInexistente =
            Cpf.Criar(GerarCpf()).Value!;

        var naoObtido =
            await _colaboradorRepo.ObterPorCpf(
                cpfInexistente);

        Assert.Null(naoObtido);
    }

    [Fact]
    public async Task Colaborador_ObterPorEmail_SucessoENulo()
    {
        var colaborador =
            await CriarEInserirColaboradorAsync();

        var obtido =
            await _colaboradorRepo.ObterPorEmail(
                colaborador.Email);

        Assert.NotNull(obtido);

        Assert.Equal(
            colaborador.Id,
            obtido.Id);

        var emailInexistente =
            Email.Criar(GerarEmail()).Value!;

        var naoObtido =
            await _colaboradorRepo.ObterPorEmail(
                emailInexistente);

        Assert.Null(naoObtido);
    }

    [Fact]
    public async Task Colaborador_CpfJaExiste_ValidacaoCorreta()
    {
        var colaborador =
            await CriarEInserirColaboradorAsync();

        var existe =
            await _colaboradorRepo.CpfJaExiste(
                colaborador.Cpf);

        Assert.True(existe);

        var existeIgnorandoId =
            await _colaboradorRepo.CpfJaExiste(
                colaborador.Cpf,
                colaborador.Id);

        Assert.False(existeIgnorandoId);

        var cpfInedito =
            Cpf.Criar(GerarCpf()).Value!;

        var existeInedito =
            await _colaboradorRepo.CpfJaExiste(
                cpfInedito);

        Assert.False(existeInedito);
    }

    [Fact]
    public async Task Colaborador_EmailJaExiste_ValidacaoCorreta()
    {
        var colaborador =
            await CriarEInserirColaboradorAsync();

        var existe =
            await _colaboradorRepo.EmailJaExiste(
                colaborador.Email);

        Assert.True(existe);

        var existeIgnorandoId =
            await _colaboradorRepo.EmailJaExiste(
                colaborador.Email,
                colaborador.Id);

        Assert.False(existeIgnorandoId);

        var emailInedito =
            Email.Criar(GerarEmail()).Value!;

        var existeInedito =
            await _colaboradorRepo.EmailJaExiste(
                emailInedito);

        Assert.False(existeInedito);
    }

    [Fact]
    public async Task Colaborador_ObterPorTipo_FiltragemCorreta()
    {
        var colaborador =
            await CriarEInserirColaboradorAsync();

        var resultados =
            await _colaboradorRepo.ObterPorTipo(
                colaborador.Tipo);

        Assert.NotNull(resultados);

        Assert.Contains(
            resultados,
            c => c.Id == colaborador.Id);
    }

    [Fact]
    public async Task Colaborador_ObterPorVinculo_FiltragemCorreta()
    {
        var colaborador =
            await CriarEInserirColaboradorAsync();

        var resultados =
            await _colaboradorRepo.ObterPorVinculo(
                colaborador.Vinculo);

        Assert.NotNull(resultados);

        Assert.Contains(
            resultados,
            c => c.Id == colaborador.Id);
    }

    [Fact]
    public async Task Colaborador_TrocarSenha_SucessoEFalha()
    {
        var colaborador =
            await CriarEInserirColaboradorAsync();

        var novaSenha =
            Senha.Criar(
                $"NovaSenha{SgbdSigla}123").Value!;

        var alterou =
            await _colaboradorRepo.TrocarSenha(
                colaborador.Id,
                novaSenha);

        Assert.True(alterou);

        var atualizado =
            await _colaboradorRepo.ObterPorId(
                colaborador.Id);

        Assert.NotNull(atualizado);

        Assert.Equal(
            $"NovaSenha{SgbdSigla}123",
            atualizado.Senha.Valor);

        var alterouInexistente =
            await _colaboradorRepo.TrocarSenha(
                999999,
                novaSenha);

        Assert.False(alterouInexistente);
    }

    private async Task<Logradouro> ObterLogradouroAsync(
        int id)
    {
        var logradouro =
            await _logradouroRepo.ObterPorId(id);

        if (logradouro is null)
        {
            throw new Exception(
                $"Logradouro com ID {id} não encontrado.");
        }

        return logradouro;
    }
}