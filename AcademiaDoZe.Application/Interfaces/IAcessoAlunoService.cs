// Alifer Granemannn

using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoAlunoService
{
    Task<IEnumerable<AcessoAlunoDto>> ObterAcessosPorAlunoPeriodoAsync(
        int? alunoId = null,
        DateOnly? inicio = null,
        DateOnly? fim = null,
        CancellationToken cancellationToken = default);

    Task<AcessoAlunoDto?> ObterUltimoAcessoAsync(
        int alunoId,
        CancellationToken cancellationToken = default);

    Task<bool> EstaNaAcademiaAsync(
        int alunoId,
        CancellationToken cancellationToken = default);

    Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMesAsync(
        int mes,
        CancellationToken cancellationToken = default);

    Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMesAsync(
        int mes,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<AlunoDto>> ObterAlunosSemAcessoNosUltimosDiasAsync(
        int dias,
        CancellationToken cancellationToken = default);
}