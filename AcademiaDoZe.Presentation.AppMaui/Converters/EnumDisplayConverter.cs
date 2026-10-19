// Alifer Granemann

using AcademiaDoZe.Application.Enums;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

public sealed class EnumDisplayConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is Enum e
            ? e.GetDisplayName()
            : string.Empty;
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
