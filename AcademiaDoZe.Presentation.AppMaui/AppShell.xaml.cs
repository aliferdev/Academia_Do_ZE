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

    // Registra as rotas de navegação das páginas.
    private static void RegisterRoutes()
    {
        Routing.RegisterRoute("logradouro", typeof(LogradouroPage));
        Routing.RegisterRoute("colaborador", typeof(ColaboradorPage));
        Routing.RegisterRoute("aluno", typeof(AlunoPage));
        Routing.RegisterRoute("matricula", typeof(MatriculaPage));
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