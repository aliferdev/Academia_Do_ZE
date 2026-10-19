// Alifer Granemann

using AcademiaDoZe.Application.DTOs;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

public sealed class FotoToImageSourceConverter : IValueConverter
{
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not ArquivoDto arquivo ||
            arquivo.Conteudo.Length == 0)
        {
            return null;
        }

        var conteudo = arquivo.Conteudo;

        return ImageSource.FromStream(
            () => new MemoryStream(conteudo));
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
