using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AlunoListPage : ContentPage
{
    private readonly AlunoListViewModel _viewModel;

    public AlunoListPage(AlunoListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAlunosCommand.ExecuteAsync(null);
    }

    private async void OnLimparPesquisaClicked(object sender, EventArgs e)
    {
        _viewModel.SearchText = string.Empty;
        _viewModel.SelectedFilterType = "Nome";
        await _viewModel.LoadAlunosCommand.ExecuteAsync(null);
    }
}