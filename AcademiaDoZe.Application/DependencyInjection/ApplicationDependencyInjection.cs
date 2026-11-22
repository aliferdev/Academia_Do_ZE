
// Alifer Granemann

using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Serviços
        services.AddTransient<ILogradouroService, LogradouroService>();
        services.AddTransient<IColaboradorService, ColaboradorService>();
        services.AddTransient<IAlunoService, AlunoService>();
        services.AddTransient<IMatriculaService, MatriculaService>();

        // Fábrica de Logradouro
        services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();

            return (Func<ILogradouroRepository>)(() =>
                new LogradouroRepository(
                    config.ConnectionString,
                    config.DatabaseType));
        });

        // Fábrica de Colaborador
        services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();

            return (Func<IColaboradorRepository>)(() =>
                new ColaboradorRepository(
                    config.ConnectionString,
                    config.DatabaseType));
        });

        // Fábrica de Aluno
        services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();

            return (Func<IAlunoRepository>)(() =>
                new AlunoRepository(
                    config.ConnectionString,
                    config.DatabaseType));
        });

        // Fábrica de Matrícula
        services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();

            return (Func<IMatriculaRepository>)(() =>
                new MatriculaRepository(
                    config.ConnectionString,
                    config.DatabaseType));
        });

        // Fábrica de Acesso de Aluno
        services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();

            var alunoRepoFactory =
                provider.GetRequiredService<Func<IAlunoRepository>>();

            return (Func<IAcessoAlunoRepository>)(() =>
                new AcessoAlunoRepository(
                    config.ConnectionString,
                    config.DatabaseType,
                    alunoRepoFactory));
        });

        // Fábrica de Acesso de Colaborador
        services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();

            return (Func<IAcessoColaboradorRepository>)(() =>
                new AcessoColaboradorRepository(
                    config.ConnectionString,
                    config.DatabaseType));
        });

        return services;
    }
}
