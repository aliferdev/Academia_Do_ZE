
// Alifer Granemannn

using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoAluno : Entity, IAggregateRoot
{
    public int AlunoId { get; private set; }
    public TipoEventoAcesso TipoEvento { get; private set; }
    public DateTime DataHora { get; private set; }

    private AcessoAluno(
        int id,
        int alunoId,
        TipoEventoAcesso tipoEvento,
        DateTime dataHora) : base(id)
    {
        AlunoId = alunoId;
        TipoEvento = tipoEvento;
        DataHora = dataHora;
    }

    public static Result<AcessoAluno> Criar(
        int id,
        Aluno? aluno,
        TipoEventoAcesso tipoEvento,
        DateTime dataHora)
    {
        if (aluno is null)
            return Result<AcessoAluno>.Failure("Aluno", "ALUNO_INVALIDO");

        if (!Enum.IsDefined(tipoEvento))
            return Result<AcessoAluno>.Failure("TipoEvento", "TIPO_EVENTO_INVALIDO");

        if (dataHora.TimeOfDay < new TimeSpan(6, 0, 0) ||
            dataHora.TimeOfDay >= new TimeSpan(24, 0, 0))
        {
            return Result<AcessoAluno>.Failure(
                "DataHora",
                "DATA_HORA_INTERVALO_INVALIDO");
        }

        return Result<AcessoAluno>.Success(
            new AcessoAluno(id, aluno.Id, tipoEvento, dataHora));
    }
}
