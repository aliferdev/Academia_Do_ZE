
// Alifer Granemann

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class ColaboradorListViewModel : BaseViewModel
{
    private readonly IColaboradorService _colaboradorService;

    public ObservableCollection<ColaboradorDto> Colaboradores { get; } = [];

    public ObservableCollection<string> FilterTypes { get; } =
    [
        "Nome",
        "CPF",
        "Email",
        "Tipo",
        "Vínculo"
    ];

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Nome";
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    public ColaboradorListViewModel(IColaboradorService colaboradorService)
    {
        _colaboradorService = colaboradorService;
        Title = "Colaboradores";
    }

    [RelayCommand]
    private async Task LoadColaboradoresAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

            var resultado = await _colaboradorService.ObterTodosAsync(cts.Token);

            Colaboradores.Clear();

            foreach (var colaborador in resultado)
                Colaboradores.Add(colaborador);
        }
        catch (OperationCanceledException)
        {
            await ExibirErroAsync(
                "O carregamento demorou demais. Tente novamente.");
        }
        catch (Exception ex)
        {
            await ExibirErroAsync(
                $"Não foi possível carregar os colaboradores: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchColaboradoresAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

            var todos = await _colaboradorService.ObterTodosAsync(cts.Token);
            var termo = SearchText.Trim();

            var resultado = todos.Where(colaborador =>
            {
                if (string.IsNullOrWhiteSpace(termo))
                    return true;

                return SelectedFilterType switch
                {
                    "Nome" => (colaborador.Nome ?? string.Empty)
                        .Contains(termo, StringComparison.OrdinalIgnoreCase),

                    "CPF" => (colaborador.Cpf ?? string.Empty)
                        .Contains(termo, StringComparison.OrdinalIgnoreCase),

                    "Email" => (colaborador.Email ?? string.Empty)
                        .Contains(termo, StringComparison.OrdinalIgnoreCase),

                    "Tipo" => colaborador.Tipo.GetDisplayName()
                        .Contains(termo, StringComparison.OrdinalIgnoreCase),

                    "Vínculo" => colaborador.Vinculo.GetDisplayName()
                        .Contains(termo, StringComparison.OrdinalIgnoreCase),

                    _ => false
                };
            }).ToList();

            Colaboradores.Clear();

            foreach (var colaborador in resultado)
                Colaboradores.Add(colaborador);
        }
        catch (OperationCanceledException)
        {
            await ExibirErroAsync(
                "A pesquisa demorou demais. Tente novamente.");
        }
        catch (Exception ex)
        {
            await ExibirErroAsync(
                $"Não foi possível pesquisar: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NovoColaboradorAsync()
    {
        await Shell.Current.GoToAsync("colaborador");
    }

    [RelayCommand]
    private async Task EditarColaboradorAsync(ColaboradorDto? colaborador)
    {
        if (colaborador is null)
            return;

        await Shell.Current.GoToAsync(
            "colaborador",
            new Dictionary<string, object>
            {
                ["Id"] = colaborador.Id
            });
    }

    [RelayCommand]
    private async Task ExcluirColaboradorAsync(ColaboradorDto? colaborador)
    {
        if (colaborador is null || IsBusy)
            return;

        bool confirmar = await Shell.Current.DisplayAlertAsync(
            "Excluir colaborador",
            $"Deseja realmente excluir o colaborador {colaborador.Nome}?",
            "Excluir",
            "Cancelar");

        if (!confirmar)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

            await _colaboradorService.RemoverAsync(
                colaborador.Id,
                cts.Token);

            Colaboradores.Remove(colaborador);

            await Shell.Current.DisplayAlertAsync(
                "Sucesso",
                "Colaborador excluído com sucesso!",
                "OK");
        }
        catch (OperationCanceledException)
        {
            await ExibirErroAsync(
                "A exclusão demorou demais. Tente novamente.");
        }
        catch (Exception ex)
        {
            await ExibirErroAsync(
                $"Não foi possível excluir o colaborador: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;

        try
        {
            await LoadColaboradoresAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private static async Task ExibirErroAsync(string mensagem)
    {
        if (Shell.Current is not null)
        {
            await Shell.Current.DisplayAlertAsync(
                "Atenção",
                mensagem,
                "OK");
        }
    }
}
