
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class MatriculaListViewModel : BaseViewModel
{
    public ObservableCollection<string> FilterTypes { get; } =
    [
        "Id",
        "Id Aluno",
        "CPF",
        "Ativas",
        "Vencendo em dias"
    ];

    private readonly IMatriculaService _matriculaService;

    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Ativas";

    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    private ObservableCollection<MatriculaDto> _matriculas = [];

    public ObservableCollection<MatriculaDto> Matriculas
    {
        get => _matriculas;
        set => SetProperty(ref _matriculas, value);
    }

    private MatriculaDto? _selectedMatricula;

    public MatriculaDto? SelectedMatricula
    {
        get => _selectedMatricula;
        set => SetProperty(ref _selectedMatricula, value);
    }

    public MatriculaListViewModel(IMatriculaService matriculaService)
    {
        _matriculaService = matriculaService;
        Title = "Matriculas";
    }

    [RelayCommand]
    private async Task AddMatriculaAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("matricula");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao navegar para tela de cadastro: {ex.Message}",
                "OK");
        }
    }

    [RelayCommand]
    private async Task EditMatriculaAsync(MatriculaDto matricula)
    {
        try
        {
            if (matricula == null)
                return;

            await Shell.Current.GoToAsync($"matricula?Id={matricula.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao navegar para tela de edição: {ex.Message}",
                "OK");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadMatriculasAsync();
    }

    [RelayCommand]
    private async Task SearchMatriculasAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

            IEnumerable<MatriculaDto> resultados = [];

            if (SelectedFilterType == "CPF")
            {
                var cpf = new string(
                    SearchText.Where(char.IsDigit).ToArray());

                if (cpf.Length != 11)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Aviso",
                        "Informe um CPF válido com 11 dígitos.",
                        "OK");

                    return;
                }

                var todasMatriculas = await _matriculaService
                    .ObterTodasAsync(cts.Token);

                resultados = todasMatriculas.Where(m =>
                    m.AlunoMatricula != null &&
                    new string(
                        (m.AlunoMatricula.Cpf ?? "")
                        .Where(char.IsDigit)
                        .ToArray()) == cpf);
            }
            else if (string.IsNullOrWhiteSpace(SearchText))
            {
                if (SelectedFilterType == "Ativas")
                {
                    resultados = await _matriculaService
                        .ObterAtivasAsync(0, cts.Token) ?? [];
                }
                else
                {
                    resultados = await _matriculaService
                        .ObterTodasAsync(cts.Token) ?? [];
                }
            }
            else if (SelectedFilterType == "Id")
            {
                if (int.TryParse(SearchText.Trim(), out int id))
                {
                    var matricula = await _matriculaService
                        .ObterPorIdAsync(id, cts.Token);

                    if (matricula != null)
                        resultados = [matricula];
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Aviso",
                        "Para busca por ID, informe um número válido.",
                        "OK");
                }
            }
            else if (SelectedFilterType == "Id Aluno")
            {
                if (int.TryParse(SearchText.Trim(), out int alunoId))
                {
                    resultados = await _matriculaService
                        .ObterPorAlunoIdAsync(alunoId, cts.Token) ?? [];
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Aviso",
                        "Para busca por ID do Aluno, informe um número válido.",
                        "OK");
                }
            }
            else if (SelectedFilterType == "Ativas")
            {
                if (!int.TryParse(SearchText.Trim(), out int alunoId))
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Aviso",
                        "Informe um ID de aluno válido.",
                        "OK");

                    return;
                }

                resultados = await _matriculaService
                    .ObterAtivasAsync(alunoId, cts.Token) ?? [];
            }
            else if (SelectedFilterType == "Vencendo em dias")
            {
                if (int.TryParse(SearchText.Trim(), out int dias))
                {
                    resultados = await _matriculaService
                        .ObterVencendoEmDiasAsync(dias, cts.Token) ?? [];
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Aviso",
                        "Para busca por vencimento, informe a quantidade de dias (número inteiro).",
                        "OK");
                }
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Matriculas.Clear();

                foreach (var item in resultados)
                    Matriculas.Add(item);

                OnPropertyChanged(nameof(Matriculas));
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao buscar matrículas: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadMatriculasAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

            var matriculasList = await _matriculaService
                .ObterTodasAsync(cts.Token);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Matriculas.Clear();

                foreach (var matricula in matriculasList)
                    Matriculas.Add(matricula);

                OnPropertyChanged(nameof(Matriculas));
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao carregar matrículas: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task DeleteMatriculaAsync(MatriculaDto matricula)
    {
        if (matricula == null)
            return;

        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Confirmar Exclusão",
            $"Deseja realmente excluir a matrícula {matricula.Id}?",
            "Sim",
            "Não");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(5));

            bool success = await _matriculaService
                .RemoverAsync(matricula.Id, cts.Token);

            if (success)
            {
                Matriculas.Remove(matricula);

                await Shell.Current.DisplayAlertAsync(
                    "Sucesso",
                    "Matrícula excluída com sucesso!",
                    "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(
                    "Erro",
                    "Não foi possível excluir a matrícula.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao excluir matrícula: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
