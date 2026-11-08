// Alifer Granemann

using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AlunoPage : ContentPage
{
    private readonly AlunoViewModel _viewModel;

    public AlunoPage(AlunoViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Erro",
                $"Não foi possível carregar os dados do aluno: {ex.Message}",
                "OK");
        }
    }
}