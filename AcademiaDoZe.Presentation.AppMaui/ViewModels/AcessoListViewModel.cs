
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AcessoListViewModel : ObservableObject
{
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;

    private string _cpf = string.Empty;
    private string _tipoPessoa = "Aluno";
    private PessoaDto? _pessoaSelecionada;
    private string _statusAcesso = "Pesquise uma pessoa pelo CPF.";
    private string _mensagem = string.Empty;
    private string _horarioEntrada = "Não registrada";
    private string _horarioSaida = "Não registrada";

    public string Cpf
    {
        get => _cpf;
        set => SetProperty(ref _cpf, value);
    }

    public string TipoPessoa
    {
        get => _tipoPessoa;
        set => SetProperty(ref _tipoPessoa, value);
    }

    public PessoaDto? PessoaSelecionada
    {
        get => _pessoaSelecionada;
        set
        {
            if (SetProperty(ref _pessoaSelecionada, value))
                OnPropertyChanged(nameof(PessoaEncontrada));
        }
    }

    public string StatusAcesso
    {
        get => _statusAcesso;
        set => SetProperty(ref _statusAcesso, value);
    }

    public string Mensagem
    {
        get => _mensagem;
        set => SetProperty(ref _mensagem, value);
    }

    public string HorarioEntrada
    {
        get => _horarioEntrada;
        set => SetProperty(ref _horarioEntrada, value);
    }

    public string HorarioSaida
    {
        get => _horarioSaida;
        set => SetProperty(ref _horarioSaida, value);
    }

    public bool PessoaEncontrada => PessoaSelecionada != null;

    public List<string> TiposPessoa { get; } = ["Aluno", "Colaborador"];

    public AcessoListViewModel(
        IAlunoService alunoService,
        IColaboradorService colaboradorService)
    {
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
    }

    [RelayCommand]
    private async Task BuscarAsync()
    {
        try
        {
            Mensagem = string.Empty;
            PessoaSelecionada = null;
            HorarioEntrada = "Não registrada";
            HorarioSaida = "Não registrada";

            string cpfInformado = Cpf?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(cpfInformado))
            {
                Mensagem = "Informe o CPF para pesquisar.";
                return;
            }

            if (TipoPessoa == "Aluno")
            {
                var aluno = await _alunoService.ObterPorCpfAsync(cpfInformado);

                if (aluno == null)
                {
                    Mensagem = "Aluno não encontrado.";
                    StatusAcesso = "Pessoa não localizada.";
                    return;
                }

                PessoaSelecionada = aluno;

                bool dentro = await _alunoService.EstaNaAcademiaAsync(aluno.Id);

                StatusAcesso = dentro
                    ? "Dentro da academia"
                    : "Fora da academia";

                await CarregarHorariosAsync(aluno.Id);
            }
            else
            {
                var colaborador = await _colaboradorService.ObterPorCpfAsync(cpfInformado);

                if (colaborador == null)
                {
                    Mensagem = "Colaborador não encontrado.";
                    StatusAcesso = "Pessoa não localizada.";
                    return;
                }

                PessoaSelecionada = colaborador;

                bool dentro = await _colaboradorService.EstaNaAcademiaAsync(colaborador.Id);

                StatusAcesso = dentro
                    ? "Dentro da academia"
                    : "Fora da academia";
            }
        }
        catch (Exception ex)
        {
            Mensagem = $"Erro ao pesquisar pessoa: {ex.Message}";
        }
    }

    private async Task CarregarHorariosAsync(int alunoId)
    {
        var acessos = await _alunoService.ObterAcessosDeHojeAsync(alunoId);

        var ultimaEntrada = acessos
            .Where(a => string.Equals(
                a.TipoEvento, "Entrada",
                StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(a => a.DataHora)
            .FirstOrDefault();

        var ultimaSaida = acessos
            .Where(a => string.Equals(
                a.TipoEvento, "Saida",
                StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(a => a.DataHora)
            .FirstOrDefault();

        HorarioEntrada = ultimaEntrada?.Horario ?? "Não registrada";
        HorarioSaida = ultimaSaida?.Horario ?? "Não registrada";
    }

    [RelayCommand]
    private async Task RegistrarEntradaAsync()
    {
        if (PessoaSelecionada == null)
            return;

        try
        {
            if (PessoaSelecionada is AlunoDto aluno)
            {
                await _alunoService.RegistrarEntradaAsync(aluno.Id);
                await CarregarHorariosAsync(aluno.Id);
            }
            else if (PessoaSelecionada is ColaboradorDto colaborador)
            {
                await _colaboradorService.RegistrarEntradaAsync(colaborador.Id);
            }

            StatusAcesso = "Dentro da academia";
            Mensagem = "Entrada registrada com sucesso.";
        }
        catch (Exception ex)
        {
            Mensagem = $"Erro ao registrar entrada: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RegistrarSaidaAsync()
    {
        if (PessoaSelecionada == null)
            return;

        try
        {
            if (PessoaSelecionada is AlunoDto aluno)
            {
                await _alunoService.RegistrarSaidaAsync(aluno.Id);
                await CarregarHorariosAsync(aluno.Id);
            }
            else if (PessoaSelecionada is ColaboradorDto colaborador)
            {
                await _colaboradorService.RegistrarSaidaAsync(colaborador.Id);
            }

            StatusAcesso = "Fora da academia";
            Mensagem = "Saída registrada com sucesso.";
        }
        catch (Exception ex)
        {
            Mensagem = $"Erro ao registrar saída: {ex.Message}";
        }
    }
}
