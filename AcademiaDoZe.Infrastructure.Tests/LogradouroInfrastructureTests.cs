// Alifer Granemannn

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public sealed class LogradouroInfrastructureTests : TestBase, IAsyncLifetime
{
    private readonly LogradouroRepository _repository;

    public LogradouroInfrastructureTests()
    {
        _repository = new LogradouroRepository(ConnectionString, DatabaseType);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _repository.DisposeAsync();

    private async Task<Logradouro> CriarEInserirLogradouroAsync()
    {
        var logradouroResult = Logradouro.Criar(0, GerarCep(), NomeRua, NomeBairro, NomeCidade, "SC", "Brasil");
        if (logradouroResult.IsFailure)
        {
            throw new Exception($"Falha ao criar Logradouro: {string.Join(", ", logradouroResult.Notifications.Select(n => n.Mensagem))}");
        }

        return await _repository.Adicionar(logradouroResult.Value!);
    }

    [Fact]
    public async Task Logradouro_Adicionar_E_ObterPorId_Sucesso()
    {
        var cep = GerarCep();
        var logradouro = Logradouro.Criar(0, cep, NomeRua, NomeBairro, NomeCidade, "SC", "Brasil").Value!;

        var inserido = await _repository.Adicionar(logradouro);

        Assert.NotNull(inserido);
        Assert.True(inserido.Id > 0);
        Assert.Equal(cep, inserido.Cep.Valor);
        Assert.Equal(NomeRua, inserido.Nome);
        Assert.Equal(NomeBairro, inserido.Bairro);
        Assert.Equal(NomeCidade, inserido.Cidade);

        var obtido = await _repository.ObterPorId(inserido.Id);

        Assert.NotNull(obtido);
        Assert.Equal(inserido.Id, obtido.Id);
        Assert.Equal(cep, obtido.Cep.Valor);
    }

    [Fact]
    public async Task Logradouro_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido = await _repository.ObterPorId(999999);

        Assert.Null(obtido);
    }

    [Fact]
    public async Task Logradouro_ObterTodos_Sucesso()
    {
        await CriarEInserirLogradouroAsync();

        var todos = await _repository.ObterTodos();

        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact]
    public async Task Logradouro_Atualizar_Sucesso()
    {
        var logradouro = await CriarEInserirLogradouroAsync();
        var novoCep = GerarCep();
        var logradouroAtualizado = Logradouro.Criar(logradouro.Id, novoCep, NomeRua, NomeBairro, NomeCidade, "SC", "Brasil").Value!;

        var resultado = await _repository.Atualizar(logradouroAtualizado);

        Assert.NotNull(resultado);
        Assert.Equal(novoCep, resultado.Cep.Valor);
        Assert.Equal(NomeRua, resultado.Nome);
        Assert.Equal(NomeBairro, resultado.Bairro);
        Assert.Equal(NomeCidade, resultado.Cidade);

        var noBanco = await _repository.ObterPorId(logradouro.Id);
        Assert.NotNull(noBanco);
        Assert.Equal(novoCep, noBanco.Cep.Valor);
    }

    [Fact]
    public async Task Logradouro_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var logradouroInexistente = Logradouro.Criar(999999, GerarCep(), NomeRua, NomeBairro, NomeCidade, "SC", "Brasil").Value!;

        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => _repository.Atualizar(logradouroInexistente));

        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact]
    public async Task Logradouro_Remover_Sucesso()
    {
        var logradouro = await CriarEInserirLogradouroAsync();

        var removido = await _repository.Remover(logradouro.Id);

        Assert.True(removido);
        Assert.Null(await _repository.ObterPorId(logradouro.Id));
    }

    [Fact]
    public async Task Logradouro_Remover_RetornaFalseQuandoInexistente()
    {
        var removido = await _repository.Remover(999999);

        Assert.False(removido);
    }

    [Fact]
    public async Task Logradouro_ObterPorCep_SucessoENulo()
    {
        var logradouro = await CriarEInserirLogradouroAsync();

        var obtido = await _repository.ObterPorCep(logradouro.Cep);
        Assert.NotNull(obtido);
        Assert.Equal(logradouro.Id, obtido.Id);

        var cepInexistente = Cep.Criar("99999999").Value!;
        Assert.Null(await _repository.ObterPorCep(cepInexistente));
    }

    [Fact]
    public async Task Logradouro_CepJaExiste_ValidacaoCorreta()
    {
        var logradouro = await CriarEInserirLogradouroAsync();

        Assert.True(await _repository.CepJaExiste(logradouro.Cep));
        Assert.False(await _repository.CepJaExiste(logradouro.Cep, logradouro.Id));

        var cepInedito = Cep.Criar(GerarCep()).Value!;
        Assert.False(await _repository.CepJaExiste(cepInedito));
    }

    [Fact]
    public async Task Logradouro_ObterPorCidade_FiltragemCorreta()
    {
        var logradouro = await CriarEInserirLogradouroAsync();

        var resultados = await _repository.ObterPorCidade(logradouro.Cidade.ToLowerInvariant());

        Assert.NotNull(resultados);
        Assert.Contains(resultados, item => item.Id == logradouro.Id);
    }

    [Fact]
    public async Task Logradouro_ObterPorBairro_FiltragemCorreta()
    {
        var logradouro = await CriarEInserirLogradouroAsync();

        var resultados = await _repository.ObterPorBairro(logradouro.Cidade, logradouro.Bairro);

        Assert.NotNull(resultados);
        Assert.Contains(resultados, item => item.Id == logradouro.Id);

        var resultadosVazio = await _repository.ObterPorBairro(logradouro.Cidade, "BairroInexistente");
        Assert.Empty(resultadosVazio);
    }
}
