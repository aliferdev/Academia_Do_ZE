// Alifer Granemann

using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ColaboradorListPage : ContentPage
{
    private readonly ColaboradorListViewModel _viewModel;

    public ColaboradorListPage(ColaboradorListViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadColaboradoresCommand.ExecuteAsync(null);
    }
}