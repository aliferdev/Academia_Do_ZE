// Alifer Granemannn
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Exceptions;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;
using Xunit;

namespace AcademiaDoZe.Tests;

public sealed class NormalizacaoServiceTests
{
    [Theory]
    [InlineData(null, true)] [InlineData("", true)] [InlineData("   ", true)]
    [InlineData("a", false)] [InlineData("  nome", false)] [InlineData("0", false)]
    public void TextoVazioOuNulo_DeveIdentificarValoresVazios(string? texto, bool esperado) =>
        Assert.Equal(esperado, NormalizacaoService.TextoVazioOuNulo(texto));

    [Theory]
    [InlineData(null, "")] [InlineData("", "")] [InlineData("  Ana  ", "Ana")]
    [InlineData("Ana   Maria", "Ana Maria")] [InlineData("Ana\tMaria", "Ana Maria")]
    [InlineData(" Ana \n Maria ", "Ana Maria")]
    public void LimparEspacos_DeveNormalizarTexto(string? texto, string esperado) =>
        Assert.Equal(esperado, NormalizacaoService.LimparEspacos(texto));

    [Theory]
    [InlineData(null, "")] [InlineData("SP", "SP")] [InlineData(" S P ", "SP")]
    [InlineData("Rio de Janeiro", "RiodeJaneiro")]
    public void LimparTodosEspacos_DeveRemoverEspacosComuns(string? texto, string esperado) =>
        Assert.Equal(esperado, NormalizacaoService.LimparTodosEspacos(texto));

    [Theory]
    [InlineData(null, "")] [InlineData("", "")] [InlineData("sp", "SP")]
    [InlineData("São Paulo", "SÃO PAULO")]
    public void ParaMaiusculo_DeveUsarFormatoInvariante(string? texto, string esperado) =>
        Assert.Equal(esperado, NormalizacaoService.ParaMaiusculo(texto));

    [Theory]
    [InlineData(null, "")] [InlineData("abc", "")] [InlineData("(11) 99999-9999", "11999999999")]
    [InlineData("12a3", "123")]
    public void LimparEDigitos_DeveManterSomenteNumeros(string? texto, string esperado) =>
        Assert.Equal(esperado, NormalizacaoService.LimparEDigitos(texto));
}

public sealed class ValueObjectTests
{
    [Theory]
    [InlineData("13083-850", true, "13083850")] [InlineData("01001000", true, "01001000")]
    [InlineData(null, false, null)] [InlineData("", false, null)] [InlineData("123", false, null)]
    [InlineData("123456789", false, null)] [InlineData("abcdefgh", false, null)]
    [InlineData("12.345-678", true, "12345678")] [InlineData("0000000", false, null)]
    [InlineData("12 345 678", true, "12345678")]
    public void Cep_Criar_DeveValidarEPadronizar(string? valor, bool sucesso, string? esperado)
    {
        var result = Cep.Criar(valor!);
        Assert.Equal(sucesso, result.IsSuccess);
        if (sucesso) Assert.Equal(esperado, result.Value!.Valor); else Assert.NotEmpty(result.Notifications);
    }

    [Theory]
    [InlineData("123.456.789-01", true)] [InlineData("12345678901", true)]
    [InlineData(null, false)] [InlineData("", false)] [InlineData("123", false)]
    [InlineData("123456789012", false)] [InlineData("abcdefghijk", false)]
    [InlineData("123.456.789-0", false)] [InlineData("123.456.789-012", false)]
    [InlineData("123 456 789 01", true)]
    public void Cpf_Criar_DeveValidarQuantidadeDeDigitos(string? valor, bool sucesso) =>
        Assert.Equal(sucesso, Cpf.Criar(valor!).IsSuccess);

    [Theory]
    [InlineData("(11) 99999-9999", true)] [InlineData("11999999999", true)]
    [InlineData(null, false)] [InlineData("", false)] [InlineData("1199999999", false)]
    [InlineData("119999999999", false)] [InlineData("abcdefghijk", false)]
    [InlineData("(11)9999-9999", false)] [InlineData("11-99999-999", false)]
    public void Telefone_Criar_DeveValidarQuantidadeDeDigitos(string? valor, bool sucesso) =>
        Assert.Equal(sucesso, Telefone.Criar(valor!).IsSuccess);

    [Theory]
    [InlineData("ana@academia.com", true)] [InlineData(" ana@academia.com ", true)]
    [InlineData(null, false)] [InlineData("", false)] [InlineData("ana", false)]
    [InlineData("@academia.com", false)] [InlineData("ana@academia", false)]
    [InlineData("ana@.com", false)] [InlineData("ana@academia.", false)]
    [InlineData("ana@@academia.com", false)] [InlineData("ana@academia..com", false)]
    public void Email_Criar_DeveValidarFormato(string? valor, bool sucesso) =>
        Assert.Equal(sucesso, Email.Criar(valor!).IsSuccess);

    [Theory]
    [InlineData("Senha1", true)] [InlineData("  Senha1  ", true)] [InlineData("", false)]
    [InlineData("   ", false)] [InlineData("\t", false)] [InlineData("senha1", false)]
    [InlineData("Senha", false)]
    public void Senha_Criar_DeveExigirConteudo(string valor, bool sucesso) =>
        Assert.Equal(sucesso, Senha.Criar(valor).IsSuccess);

    [Fact]
    public void Arquivo_Criar_DeveAceitarArquivoNoLimite() => Assert.True(Arquivo.Criar(new byte[15 * 1024 * 1024]).IsSuccess);

    [Fact]
    public void Arquivo_Criar_DeveRejeitarArquivoNulo() => Assert.True(Arquivo.Criar(null!).IsFailure);

    [Fact]
    public void Arquivo_Criar_DeveRejeitarArquivoAcimaDoLimite() => Assert.True(Arquivo.Criar(new byte[15 * 1024 * 1024 + 1]).IsFailure);

    [Fact]
    public void Endereco_Criar_DeveAceitarDadosValidos() => Assert.True(Endereco.Criar(TestData.Logradouro(), " 12 ", " Fundos ").IsSuccess);

    [Theory]
    [InlineData(null, "12")] [InlineData("logradouro", "")] [InlineData("logradouro", " ")]
    public void Endereco_Criar_DeveExigirLogradouroENumero(string? logradouro, string numero)
    {
        var result = Endereco.Criar(logradouro is null ? null! : TestData.Logradouro(), numero, "");
        Assert.True(result.IsFailure);
    }
}

public sealed class EntityTests
{
    [Fact]
    public void Logradouro_Criar_DeveNormalizarCampos()
    {
        var result = Logradouro.Criar(1, "13083-850", " Rua A ", " Centro ", " Campinas ", " sp ", " Brasil ");
        Assert.True(result.IsSuccess);
        Assert.Equal("SP", result.Value!.Estado);
        Assert.Equal("Rua A", result.Value.Nome);
    }

    [Theory]
    [InlineData(null, "bairro", "cidade", "SP", "Brasil")]
    [InlineData("rua", "", "cidade", "SP", "Brasil")]
    [InlineData("rua", "bairro", "", "SP", "Brasil")]
    [InlineData("rua", "bairro", "cidade", "", "Brasil")]
    [InlineData("rua", "bairro", "cidade", "SP", "")]
    public void Logradouro_Criar_DeveExigirCamposObrigatorios(string? nome, string bairro, string cidade, string estado, string pais) =>
        Assert.True(Logradouro.Criar(1, "13083850", nome!, bairro, cidade, estado, pais).IsFailure);

    [Fact]
    public void Aluno_Criar_DeveCriarAlunoValido() => Assert.True(TestData.Aluno().IsSuccess);

    [Theory]
    [InlineData("", "12345678901", "11999999999", "ana@academia.com", "Senha1")]
    [InlineData("Ana", "123", "11999999999", "ana@academia.com", "Senha1")]
    [InlineData("Ana", "12345678901", "1", "ana@academia.com", "Senha1")]
    [InlineData("Ana", "12345678901", "11999999999", "invalido", "Senha1")]
    [InlineData("Ana", "12345678901", "11999999999", "ana@academia.com", "")]
    public void Aluno_Criar_DeveAcumularValidacoes(string nome, string cpf, string telefone, string email, string senha)
    {
        var result = TestData.Aluno(nome, cpf, telefone, email, senha);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
    }

    [Fact]
    public void Aluno_Criar_DeveRejeitarMenorDeDozeAnos() =>
        Assert.True(TestData.Aluno(dataNascimento: DateOnly.FromDateTime(DateTime.Today).AddYears(-11)).IsFailure);

    [Fact]
    public void Colaborador_Criar_DeveCriarColaboradorValido() => Assert.True(TestData.Colaborador().IsSuccess);

    [Theory]
    [InlineData(ColaboradorTipo.Administrador, ColaboradorVinculo.Estagio)]
    [InlineData((ColaboradorTipo)99, ColaboradorVinculo.CLT)]
    [InlineData(ColaboradorTipo.Atendente, (ColaboradorVinculo)99)]
    public void Colaborador_Criar_DeveRejeitarTipoOuVinculoInvalidos(ColaboradorTipo tipo, ColaboradorVinculo vinculo) =>
        Assert.True(TestData.Colaborador(tipo: tipo, vinculo: vinculo).IsFailure);

    [Fact]
    public void Colaborador_Criar_DeveRejeitarMenorDeDozeAnos() =>
        Assert.True(TestData.Colaborador(dataNascimento: DateOnly.FromDateTime(DateTime.Today).AddYears(-11)).IsFailure);

    [Fact]
    public void Colaborador_Criar_DeveRejeitarAdmissaoFutura() =>
        Assert.True(TestData.Colaborador(dataAdmissao: DateOnly.FromDateTime(DateTime.Today).AddDays(1)).IsFailure);

    [Theory]
    [InlineData(MatriculaPlano.Mensal, 1)] [InlineData(MatriculaPlano.Trimestral, 3)]
    [InlineData(MatriculaPlano.Semestral, 6)] [InlineData(MatriculaPlano.Anual, 12)]
    public void Matricula_Criar_DeveCalcularDataFinal(MatriculaPlano plano, int meses)
    {
        var inicio = new DateOnly(2026, 1, 15);
        var result = Matricula.Criar(1, TestData.Aluno().Value!, plano, inicio, "Saúde", MatriculaRestricoes.None, null);
        Assert.Equal(inicio.AddMonths(meses), result.Value!.DataFim);
    }

    [Theory]
    [InlineData((MatriculaPlano)99, MatriculaRestricoes.None, false)]
    [InlineData(MatriculaPlano.Mensal, MatriculaRestricoes.Diabetes, false)]
    [InlineData(MatriculaPlano.Mensal, MatriculaRestricoes.None, true)]
    public void Matricula_Criar_DeveValidarPlanoELaudo(MatriculaPlano plano, MatriculaRestricoes restricao, bool usarAlunoMenor)
    {
        var aluno = usarAlunoMenor
            ? TestData.Aluno(dataNascimento: DateOnly.FromDateTime(DateTime.Today).AddYears(-15)).Value!
            : TestData.Aluno().Value!;
        var result = Matricula.Criar(1, aluno, plano, new DateOnly(2026, 1, 1), "Saúde", restricao, null);
        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData(null, true)] [InlineData("", true)] [InlineData("  ganhar massa  ", false)]
    public void Matricula_Criar_DeveExigirObjetivo(string? objetivo, bool falha)
    {
        var result = Matricula.Criar(1, TestData.Aluno().Value!, MatriculaPlano.Mensal, new DateOnly(2026, 1, 1), objetivo!, MatriculaRestricoes.None, null);
        Assert.Equal(falha, result.IsFailure);
    }

    [Fact]
    public void AcessoAluno_Criar_DeveExigirAluno() => Assert.True(AcessoAluno.Criar(1, null, new DateTime(2026, 1, 1, 10, 0, 0)).IsFailure);

    [Fact]
    public void AcessoAluno_Criar_DeveRejeitarHorarioForaDoIntervalo() => Assert.True(AcessoAluno.Criar(1, TestData.Aluno().Value!, new DateTime(2026, 1, 1, 5, 59, 0)).IsFailure);

    [Fact]
    public void AcessoAluno_Criar_DeveCriarAcessoValido() => Assert.True(AcessoAluno.Criar(1, TestData.Aluno().Value!, new DateTime(2026, 1, 1, 10, 0, 0)).IsSuccess);

    [Fact]
    public void AcessoColaborador_Criar_DeveExigirColaborador() => Assert.True(AcessoColaborador.Criar(1, null, new DateTime(2026, 1, 1, 10, 0, 0)).IsFailure);

    [Fact]
    public void AcessoColaborador_Criar_DeveRejeitarHorarioForaDoIntervalo() => Assert.True(AcessoColaborador.Criar(1, TestData.Colaborador().Value!, new DateTime(2026, 1, 1, 22, 1, 0)).IsFailure);

    [Fact]
    public void AcessoColaborador_Criar_DevePermitirHorarioDeBorda()
    {
        Assert.True(AcessoColaborador.Criar(1, TestData.Colaborador().Value!, new DateTime(2026, 1, 1, 22, 0, 0)).IsSuccess);
    }

    [Fact]
    public void AcessoColaborador_Criar_DeveCriarAcessoValido() => Assert.True(AcessoColaborador.Criar(1, TestData.Colaborador().Value!, new DateTime(2026, 1, 1, 10, 0, 0)).IsSuccess);

    [Fact]
    public void Entity_DeveAceitarIdZero() => Assert.Equal(0, new TestEntity(0).Id);

    [Fact]
    public void Entity_DeveRejeitarIdNegativo() => Assert.Throws<DomainException>(() => new TestEntity(-1));
}

public sealed class CommonAndRepositoryTests
{
    [Fact]
    public void Result_Success_DeveConterValorESemNotificacoes()
    {
        var result = Result<int>.Success(10);
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
        Assert.Empty(result.Notifications);
    }

    [Fact]
    public void Result_Failure_DeveConterNotificacao()
    {
        var result = Result<int>.Failure("Campo", "ERRO");
        Assert.True(result.IsFailure);
        Assert.Equal(0, result.Value);
        Assert.Equal(new Notification("Campo", "ERRO"), Assert.Single(result.Notifications));
    }

    [Theory]
    [InlineData(typeof(IAlunoRepository), typeof(Aluno))]
    [InlineData(typeof(IColaboradorRepository), typeof(Colaborador))]
    [InlineData(typeof(ILogradouroRepository), typeof(Logradouro))]
    [InlineData(typeof(IMatriculaRepository), typeof(Matricula))]
    [InlineData(typeof(IAcessoAlunoRepository), typeof(AcessoAluno))]
    [InlineData(typeof(IAcessoColaboradorRepository), typeof(AcessoColaborador))]
    public void Repositorios_DeveDerivarDoContratoGenerico(Type repositorio, Type entidade)
    {
        var contrato = typeof(IRepository<>).MakeGenericType(entidade);
        Assert.True(contrato.IsAssignableFrom(repositorio));
    }
}

internal static class TestData
{
    public static AcademiaDoZe.Domain.Entities.Logradouro Logradouro() => AcademiaDoZe.Domain.Entities.Logradouro.Criar(1, "13083850", "Rua A", "Centro", "Campinas", "SP", "Brasil").Value!;
    public static Arquivo Foto() => Arquivo.Criar([1]).Value!;
    public static Result<Aluno> Aluno(string nome = "Ana Silva", string cpf = "12345678901", string telefone = "11999999999", string email = "ana@academia.com", string senha = "Senha1", DateOnly? dataNascimento = null) =>
        AcademiaDoZe.Domain.Entities.Aluno.Criar(1, nome, cpf, dataNascimento ?? new DateOnly(1990, 1, 1), telefone, email, Logradouro(), "10", "", senha, Foto());
    public static Result<Colaborador> Colaborador(ColaboradorTipo tipo = ColaboradorTipo.Atendente, ColaboradorVinculo vinculo = ColaboradorVinculo.CLT, DateOnly? dataNascimento = null, DateOnly? dataAdmissao = null) =>
        AcademiaDoZe.Domain.Entities.Colaborador.Criar(1, "Carlos", "12345678901", dataNascimento ?? new DateOnly(1990, 1, 1), "11999999999", "carlos@academia.com", Logradouro(), "10", "", "Senha1", Foto(), dataAdmissao ?? new DateOnly(2020, 1, 1), tipo, vinculo);
}

internal sealed class TestEntity(int id) : Entity(id);
