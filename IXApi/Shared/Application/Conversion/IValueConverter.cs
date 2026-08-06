namespace IAX.IXApi.Shared.Application.Conversion
{
    /// <summary>
    /// Service for robust type conversion from strings (e.g. from Excel) to C# types.
    /// </summary>
    public interface IValueConverter
    {
        /// <summary>
        /// Converts a string value to the specified target type.
        /// </summary>
        /// <param name="stringValue">The string representation of the value.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <returns>The converted value as an object.</returns>
        object? ConvertValue(string? stringValue, Type targetType);
    }
}
