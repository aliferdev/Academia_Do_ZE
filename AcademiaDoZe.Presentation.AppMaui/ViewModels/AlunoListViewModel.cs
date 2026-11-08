// Alifer Granemann

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AlunoListViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;

    public ObservableCollection<AlunoDto> Alunos { get; } = [];

    public ObservableCollection<string> FilterTypes { get; } =
    [
        "Nome",
        "CPF",
        "Email"
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

    public AlunoListViewModel(IAlunoService alunoService)
    {
        _alunoService = alunoService;
        Title = "Alunos";
    }

    [RelayCommand]
    private async Task LoadAlunosAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var resultado = await _alunoService.ObterTodosAsync();

            Alunos.Clear();

            foreach (var aluno in resultado)
                Alunos.Add(aluno);
        }
        catch (Exception ex)
        {
            await ExibirErroAsync(
                $"Não foi possível carregar os alunos: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchAlunosAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var todos = await _alunoService.ObterTodosAsync();
            var termo = SearchText.Trim();

            var resultado = todos.Where(aluno =>
            {
                if (string.IsNullOrWhiteSpace(termo))
                    return true;

                return SelectedFilterType switch
                {
                    "Nome" => aluno.Nome.Contains(
                        termo,
                        StringComparison.OrdinalIgnoreCase),

                    "CPF" => aluno.Cpf.Contains(
                        termo,
                        StringComparison.OrdinalIgnoreCase),

                    "Email" => (aluno.Email ?? string.Empty).Contains(
                        termo,
                        StringComparison.OrdinalIgnoreCase),

                    _ => false
                };
            }).ToList();

            Alunos.Clear();

            foreach (var aluno in resultado)
                Alunos.Add(aluno);
        }
        catch (Exception ex)
        {
            await ExibirErroAsync(
                $"Não foi possível pesquisar os alunos: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NovoAlunoAsync()
    {
        await Shell.Current.GoToAsync("aluno");
    }

    [RelayCommand]
    private async Task EditarAlunoAsync(AlunoDto? aluno)
    {
        if (aluno is null)
            return;

        await Shell.Current.GoToAsync(
            "aluno",
            new Dictionary<string, object>
            {
                ["Id"] = aluno.Id
            });
    }

    [RelayCommand]
    private async Task ExcluirAlunoAsync(AlunoDto? aluno)
    {
        if (aluno is null || IsBusy)
            return;

        bool confirmar = await Shell.Current.DisplayAlertAsync(
            "Excluir aluno",
            $"Deseja realmente excluir o aluno {aluno.Nome}?",
            "Excluir",
            "Cancelar");

        if (!confirmar)
            return;

        try
        {
            IsBusy = true;

            await _alunoService.RemoverAsync(aluno.Id);

            Alunos.Remove(aluno);

            await Shell.Current.DisplayAlertAsync(
                "Sucesso",
                "Aluno excluído com sucesso!",
                "OK");
        }
        catch (Exception ex)
        {
            await ExibirErroAsync(
                $"Não foi possível excluir o aluno: {ex.Message}");
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
            await LoadAlunosAsync();
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