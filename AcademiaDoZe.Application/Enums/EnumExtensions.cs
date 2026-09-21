// Alifer Granemann

using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AcademiaDoZe.Application.Enums;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var type = value.GetType();
        var field = type.GetField(value.ToString());

        var attribute = field?.GetCustomAttribute<DisplayAttribute>();

        if (attribute != null)
            return attribute.Name ?? value.ToString();

        return value.ToString();
    }
}