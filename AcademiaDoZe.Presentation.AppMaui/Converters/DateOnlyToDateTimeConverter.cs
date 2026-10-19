// Alifer Granemann

using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

public sealed class DateOnlyToDateTimeConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is DateOnly date
            ? date.ToDateTime(TimeOnly.MinValue)
            : DateTime.Today;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is DateTime dateTime
            ? DateOnly.FromDateTime(dateTime)
            : DateOnly.FromDateTime(DateTime.Today);
    }
}
