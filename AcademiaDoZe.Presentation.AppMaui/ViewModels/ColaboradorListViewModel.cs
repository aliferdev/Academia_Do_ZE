// Alifer Granemann

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
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

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedFilterType = "Nome";

    public ColaboradorListViewModel(
        IColaboradorService colaboradorService)
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
                TimeSpan.FromSeconds(5));

            var resultado = await _colaboradorService
                .ObterTodosAsync(cts.Token);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Colaboradores.Clear();

                foreach (var colaborador in resultado)
                    Colaboradores.Add(colaborador);
            });
        }
        catch (OperationCanceledException)
        {
            await ExibirErroAsync(
                "A operação demorou demais. Tente novamente.");
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
                TimeSpan.FromSeconds(5));

            var colaboradores = await _colaboradorService
                .ObterTodosAsync(cts.Token);

            var termo = SearchText.Trim();

            var resultado = colaboradores.Where(colaborador =>
            {
                if (string.IsNullOrWhiteSpace(termo))
                    return true;

                return SelectedFilterType switch
                {
                    "Nome" => colaborador.Nome.Contains(
                        termo, StringComparison.OrdinalIgnoreCase),

                    "CPF" => colaborador.Cpf.Contains(
                        termo, StringComparison.OrdinalIgnoreCase),

                    "Email" => (colaborador.Email ?? string.Empty).Contains(
                        termo, StringComparison.OrdinalIgnoreCase),

                    "Tipo" => colaborador.Tipo.GetDisplayName().Contains(
                        termo, StringComparison.OrdinalIgnoreCase),

                    "Vínculo" => colaborador.Vinculo.GetDisplayName().Contains(
                        termo, StringComparison.OrdinalIgnoreCase),

                    _ => false
                };
            }).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Colaboradores.Clear();

                foreach (var colaborador in resultado)
                    Colaboradores.Add(colaborador);
            });
        }
        catch (OperationCanceledException)
        {
            await ExibirErroAsync(
                "A pesquisa demorou demais. Tente novamente.");
        }
        catch (Exception ex)
        {
            await ExibirErroAsync(
                $"Não foi possível pesquisar os colaboradores: {ex.Message}");
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
            await Shell.Current.DisplayAlert(
                "Atenção",
                mensagem,
                "OK");
        }
    }
}
