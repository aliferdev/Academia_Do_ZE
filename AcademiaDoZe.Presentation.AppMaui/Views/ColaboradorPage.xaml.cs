
// Alifer Granemann

using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ColaboradorPage : ContentPage
{
    public ColaboradorPage(ColaboradorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ColaboradorViewModel viewModel)
        {
            try
            {
                await viewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Erro",
                    $"Não foi possível carregar os dados do colaborador: {ex.Message}",
                    "OK");
            }
        }
    }
}
