// Alifer Granemannn
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Email
{
    public string Valor { get; }
    private Email(string valor) => Valor = valor;

    public static Result<Email> Criar(string valor)
    {
        var textoLimpo = NormalizacaoService.LimparEspacos(valor);
        if (string.IsNullOrWhiteSpace(textoLimpo) || !ValidarFormato(textoLimpo))
            return Result<Email>.Failure("Email", "EMAIL_FORMATO");
        return Result<Email>.Success(new Email(textoLimpo));
    }

    private static bool ValidarFormato(string email)
    {
        var partes = email.Split('@');
        if (partes.Length != 2 || string.IsNullOrWhiteSpace(partes[0])) return false;
        var dominio = partes[1];
        if (string.IsNullOrWhiteSpace(dominio) || dominio.StartsWith('.') || dominio.EndsWith('.')) return false;
        var labels = dominio.Split('.');
        return labels.Length >= 2 && !labels.Any(string.IsNullOrWhiteSpace);
    }
    public override string ToString() => Valor;
}
