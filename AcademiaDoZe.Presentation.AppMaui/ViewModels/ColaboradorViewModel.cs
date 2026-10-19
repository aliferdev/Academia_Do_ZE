// Alifer Granemann

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(ColaboradorId), "Id")]
public partial class ColaboradorViewModel : BaseViewModel
{
    private readonly IColaboradorService _colaboradorService;
    private readonly ILogradouroService _logradouroService;

    private int _colaboradorId;
    private bool _isEditMode;

    private ColaboradorDto _colaborador = CriarColaboradorVazio();

    public ColaboradorDto Colaborador
    {
        get => _colaborador;
        set => SetProperty(ref _colaborador, value);
    }

    public int ColaboradorId
    {
        get => _colaboradorId;
        set => SetProperty(ref _colaboradorId, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public ObservableCollection<LogradouroDto> Logradouros { get; } = [];

    public ObservableCollection<AppColaboradorTipo> Tipos { get; } =
        [.. Enum.GetValues<AppColaboradorTipo>()];

    public ObservableCollection<AppColaboradorVinculo> Vinculos { get; } =
        [.. Enum.GetValues<AppColaboradorVinculo>()];

    private LogradouroDto? _logradouroSelecionado;

    public LogradouroDto? LogradouroSelecionado
    {
        get => _logradouroSelecionado;
        set
        {
            if (SetProperty(ref _logradouroSelecionado, value))
            {
                Colaborador.Endereco = value;
            }
        }
    }

    public ColaboradorViewModel(
        IColaboradorService colaboradorService,
        ILogradouroService logradouroService)
    {
        _colaboradorService = colaboradorService;
        _logradouroService = logradouroService;

        Title = "Cadastro de Colaborador";
    }

    private static ColaboradorDto CriarColaboradorVazio()
    {
        return new ColaboradorDto
        {
            Nome = string.Empty,
            Cpf = string.Empty,
            DataNascimento = DateOnly.FromDateTime(DateTime.Today),
            Telefone = string.Empty,
            Email = string.Empty,
            Numero = string.Empty,
            Complemento = string.Empty,
            Senha = string.Empty,
            DataAdmissao = DateOnly.FromDateTime(DateTime.Today),
            Tipo = AppColaboradorTipo.Atendente,
            Vinculo = AppColaboradorVinculo.CLT
        };
    }

    public async Task InitializeAsync()
    {
        await CarregarLogradourosAsync();

        if (ColaboradorId > 0)
        {
            IsEditMode = true;
            Title = "Editar Colaborador";
            await LoadColaboradorAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo Colaborador";

            ColaboradorId = 0;
            Colaborador = CriarColaboradorVazio();
            LogradouroSelecionado = null;
        }
    }

    private async Task CarregarLogradourosAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(5));

            var resultado = await _logradouroService.ObterTodosAsync(
                cts.Token);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Logradouros.Clear();

                foreach (var logradouro in resultado)
                    Logradouros.Add(logradouro);
            });
        }
        catch (Exception ex)
        {
            await ExibirAlertaAsync(
                "Erro",
                $"Não foi possível carregar os logradouros: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task LoadColaboradorAsync()
    {
        if (ColaboradorId <= 0)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(5));

            var resultado = await _colaboradorService.ObterPorIdAsync(
                ColaboradorId,
                cts.Token);

            if (resultado is null)
            {
                await ExibirAlertaAsync(
                    "Aviso",
                    "Colaborador não encontrado.");

                return;
            }

            Colaborador = resultado;

            LogradouroSelecionado = resultado.Endereco is null
                ? null
                : Logradouros.FirstOrDefault(
                    l => l.Id == resultado.Endereco.Id);
        }
        catch (OperationCanceledException)
        {
            await ExibirAlertaAsync(
                "Tempo esgotado",
                "O carregamento do colaborador demorou demais.");
        }
        catch (Exception ex)
        {
            await ExibirAlertaAsync(
                "Erro",
                $"Não foi possível carregar o colaborador: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveColaboradorAsync()
    {
        if (IsBusy)
            return;

        if (!await ValidarColaboradorAsync())
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(5));

            Colaborador.Nome = Colaborador.Nome.Trim();
            Colaborador.Cpf = Colaborador.Cpf.Trim();
            Colaborador.Telefone = Colaborador.Telefone.Trim();
            Colaborador.Email = string.IsNullOrWhiteSpace(Colaborador.Email)
                ? null
                : Colaborador.Email.Trim();
            Colaborador.Numero = Colaborador.Numero.Trim();
            Colaborador.Complemento =
                string.IsNullOrWhiteSpace(Colaborador.Complemento)
                    ? null
                    : Colaborador.Complemento.Trim();

            Colaborador.Endereco = LogradouroSelecionado;

            if (IsEditMode)
            {
                await _colaboradorService.AtualizarAsync(
                    Colaborador,
                    cts.Token);

                await ExibirAlertaAsync(
                    "Sucesso",
                    "Colaborador atualizado com sucesso!");
            }
            else
            {
                await _colaboradorService.AdicionarAsync(
                    Colaborador,
                    cts.Token);

                await ExibirAlertaAsync(
                    "Sucesso",
                    "Colaborador cadastrado com sucesso!");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await ExibirAlertaAsync(
                "Tempo esgotado",
                "A gravação demorou demais. Tente novamente.");
        }
        catch (InvalidOperationException ex)
        {
            await ExibirAlertaAsync("Regra de negócio", ex.Message);
        }
        catch (ArgumentException ex)
        {
            await ExibirAlertaAsync("Validação", ex.Message);
        }
        catch (Exception ex)
        {
            await ExibirAlertaAsync(
                "Erro",
                $"Não foi possível salvar o colaborador: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    private async Task<bool> ValidarColaboradorAsync()
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(Colaborador.Nome))
            erros.Add("Nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(Colaborador.Cpf))
            erros.Add("CPF é obrigatório.");

        if (string.IsNullOrWhiteSpace(Colaborador.Telefone))
            erros.Add("Telefone é obrigatório.");

        if (string.IsNullOrWhiteSpace(Colaborador.Numero))
            erros.Add("Número do endereço é obrigatório.");

        if (LogradouroSelecionado is null)
            erros.Add("Selecione um logradouro.");

        if (Colaborador.DataNascimento > DateOnly.FromDateTime(DateTime.Today))
            erros.Add("A data de nascimento não pode estar no futuro.");

        if (Colaborador.DataAdmissao > DateOnly.FromDateTime(DateTime.Today))
            erros.Add("A data de admissão não pode estar no futuro.");

        if (!IsEditMode && string.IsNullOrWhiteSpace(Colaborador.Senha))
            erros.Add("Senha é obrigatória para um novo colaborador.");

        if (erros.Count > 0)
        {
            await ExibirAlertaAsync(
                "Verifique os campos",
                string.Join("\n", erros));

            return false;
        }

        return true;
    }

    private static async Task ExibirAlertaAsync(
        string titulo,
        string mensagem)
    {
        if (Shell.Current is not null)
        {
            await Shell.Current.DisplayAlertAsync(
                titulo,
                mensagem,
                "OK");
        }
    }
}