// Alifer Granemannn
namespace AcademiaDoZe.Application.DTOs;

public class AcessoAlunoDto
{
    public int Id { get; set; }
    public required int AlunoId { get; set; }
    public required DateTime DataHora { get; set; }
}