// Alifer Granemann

using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAlunoService
{
    Task<bool> CpfJaExisteAsync(string cpf, int? id = null);

    Task<bool> EmailJaExisteAsync(string email, int? id = null);

    Task<AlunoDto?> ObterPorIdAsync(int id);

    Task<IEnumerable<AlunoDto>> ObterTodosAsync();

    Task<AlunoDto?> ObterPorCpfAsync(string cpf);

    Task<AlunoDto?> ObterPorEmailAsync(string email);

    Task<IEnumerable<AlunoDto>> ObterPorNomeAsync(string nome);

    Task<AlunoDto> AdicionarAsync(AlunoDto alunoDto);

    Task<AlunoDto> AtualizarAsync(AlunoDto alunoDto);

    Task RemoverAsync(int id);

    Task TrocarSenhaAsync(int id, string novaSenha);
}