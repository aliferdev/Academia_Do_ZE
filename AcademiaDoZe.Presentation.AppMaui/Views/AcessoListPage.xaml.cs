
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AcessoListPage : ContentPage
{
    public AcessoListPage(AcessoListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
