namespace IAX.IXApi.Modules.Administration.DataManagement.Services
{
    /// <summary>
    /// Infrastructure service for Excel operations with Sys prefix.
    /// Decoupled from business logic so the implementation (ClosedXML / EPPlus / OpenXML) can be swapped.
    /// </summary>
    public interface ISysExcelService
    {
        /// <summary>
        /// Reads an Excel file and returns each row as a dictionary (Key=ColumnName, Value=CellContent).
        /// </summary>
        IEnumerable<Dictionary<string, string>> ReadData(Stream stream, string sheetName = "");

        /// <summary>
        /// Generates an Excel file as a stream from an in-memory list. Buffers everything in RAM —
        /// only suitable for small / template-sized exports. For large datasets use <see cref="WriteAsync{T}"/>.
        /// </summary>
        Task<Stream> GenerateExcelAsync<T>(
            IEnumerable<T> data,
            List<string> properties,
            List<string>? headers = null,
            CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Generates an empty Excel template stream with the specified columns.
        /// Headers whose names appear in <paramref name="requiredColumns"/> (case-insensitive)
        /// are rendered with red text and a trailing asterisk so users can see at a glance
        /// which fields the importer will reject if left blank.
        /// </summary>
        Task<Stream> GenerateTemplateAsync(
            List<string> columns,
            string sheetName = "Template",
            IReadOnlyCollection<string>? requiredColumns = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Streams rows into an Excel workbook and writes the result directly to <paramref name="output"/>.
        /// The caller provides:
        /// <list type="bullet">
        ///   < <description><paramref name="rows"/> — a pull-based async stream from EF (e.g. AsAsyncEnumerable).</description></ 
        ///   < <description><paramref name="getCellValue"/> — extracts (row, columnIndex) values without per-cell reflection.</description></ 
        /// </list>
        /// Avoids materialising the result set as a List and avoids buffering the whole workbook as a MemoryStream copy.
        /// </summary>
        Task WriteAsync<T>(
            Stream output,
            IAsyncEnumerable<T> rows,
            IReadOnlyList<string> headers,
            Func<T, int, object?> getCellValue,
            string sheetName,
            CancellationToken cancellationToken = default);
    }
}
