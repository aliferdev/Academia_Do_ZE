
// Alifer Granemannn

using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoColaborador : Entity, IAggregateRoot
{
    public int ColaboradorId { get; private set; }
    public TipoEventoAcesso TipoEvento { get; private set; }
    public DateTime DataHora { get; private set; }

    private AcessoColaborador(
        int id,
        int colaboradorId,
        TipoEventoAcesso tipoEvento,
        DateTime dataHora) : base(id)
    {
        ColaboradorId = colaboradorId;
        TipoEvento = tipoEvento;
        DataHora = dataHora;
    }

    public static Result<AcessoColaborador> Criar(
        int id,
        Colaborador? colaborador,
        TipoEventoAcesso tipoEvento,
        DateTime dataHora)
    {
        if (colaborador is null)
            return Result<AcessoColaborador>.Failure(
                "Colaborador",
                "COLABORADOR_INVALIDO");

        if (!Enum.IsDefined(tipoEvento))
            return Result<AcessoColaborador>.Failure(
                "TipoEvento",
                "TIPO_EVENTO_INVALIDO");

        if (dataHora.TimeOfDay < new TimeSpan(6, 0, 0) ||
            dataHora.TimeOfDay >= new TimeSpan(24, 0, 0))
        {
            return Result<AcessoColaborador>.Failure(
                "DataHora",
                "DATA_HORA_INTERVALO_INVALIDO");
        }

        return Result<AcessoColaborador>.Success(
            new AcessoColaborador(
                id,
                colaborador.Id,
                tipoEvento,
                dataHora));
    }
}
