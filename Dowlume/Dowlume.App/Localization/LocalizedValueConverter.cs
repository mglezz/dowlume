using System.Globalization;
using Avalonia.Data.Converters;
using Dowlume.Core;

namespace Dowlume.App.Localization;

public sealed class LocalizedValueConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2 || values[1] is not Localizer text)
        {
            return string.Empty;
        }
        return values[0] switch
        {
            OperationRecord operation => text.Describe(operation),
            DateTimeOffset date when parameter as string == "FirstTracked" => text.Format("First tracked: {0:d}", date.LocalDateTime),
            DateTimeOffset date => date.ToLocalTime().ToString("g", text.Culture),
            var value => text.Translate(value?.ToString())
        };
    }
}
