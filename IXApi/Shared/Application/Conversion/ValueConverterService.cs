using IAX.IXApi.Shared.Application.Attributes;
using System.Globalization;

namespace IAX.IXApi.Shared.Application.Conversion
{
    public class ValueConverterService : IValueConverter
    {
        public object? ConvertValue(string? stringValue, Type targetType)
        {
            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                {
                    throw new InvalidCastException($"Value cannot be null or empty for required type '{targetType.Name}'");
                }
                return null;
            }

            try
            {
                if (underlyingType == typeof(bool))
                {
                    if (bool.TryParse(stringValue, out bool boolVal)) return boolVal;
                    if (stringValue == "1" || stringValue.Equals("Yes", StringComparison.OrdinalIgnoreCase)) return true;
                    if (stringValue == "0" || stringValue.Equals("No", StringComparison.OrdinalIgnoreCase)) return false;
                    throw new FormatException($"Invalid boolean: {stringValue}");
                }

                if (underlyingType == typeof(DateTime))
                {
                    return DateTime.Parse(stringValue, CultureInfo.InvariantCulture);
                }

                if (underlyingType == typeof(int)) return int.Parse(stringValue);
                if (underlyingType == typeof(long)) return long.Parse(stringValue);
                if (underlyingType == typeof(short)) return short.Parse(stringValue);
                if (underlyingType == typeof(byte)) return byte.Parse(stringValue);
                if (underlyingType == typeof(double)) return double.Parse(stringValue);
                if (underlyingType == typeof(decimal)) return decimal.Parse(stringValue);
                if (underlyingType == typeof(Guid)) return Guid.Parse(stringValue);

                if (underlyingType.IsEnum)
                {
                    return Enum.Parse(underlyingType, stringValue, true);
                }

                return Convert.ChangeType(stringValue, underlyingType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Failed to convert '{stringValue}' to {underlyingType.Name}: {ex.Message}", ex);
            }
        }
    }
}

