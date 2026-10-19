
// Alifer Granemann

using System.Diagnostics;
using AcademiaDoZe.Presentation.AppMaui.Views;

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        RegisterRoutes();

        Navigating += OnShellNavigating;
        Navigated += OnShellNavigated;
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute("logradouro", typeof(LogradouroPage));
        Routing.RegisterRoute("colaborador", typeof(ColaboradorPage));
    }

    private void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        Debug.WriteLine($"[NAVEGACAO] Iniciando: {e.Target.Location}");
    }

    private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        Debug.WriteLine($"[NAVEGACAO] Concluída: {e.Current.Location}");
        Debug.WriteLine($"[NAVEGACAO] Página atual: {CurrentPage?.GetType().FullName}");
    }
}
