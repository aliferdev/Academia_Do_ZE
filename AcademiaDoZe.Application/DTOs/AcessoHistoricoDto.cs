
namespace AcademiaDoZe.Application.DTOs;

public class AcessoHistoricoDto
{
    public required string TipoEvento { get; set; }

    public required DateTime DataHora { get; set; }

    public string Horario => DataHora.ToString("HH:mm");
}
