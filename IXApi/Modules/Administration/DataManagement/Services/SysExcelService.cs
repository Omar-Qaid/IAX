using ClosedXML.Excel;
using IAX.IXApi.Shared.Application.Attributes;

namespace IAX.IXApi.Modules.Administration.DataManagement.Services
{
    [ScopedService]
    public class SysExcelService : ISysExcelService
    {
        public IEnumerable<Dictionary<string, string>> ReadData(Stream stream, string sheetName = "")
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = string.IsNullOrEmpty(sheetName) ? workbook.Worksheet(1) : workbook.Worksheet(sheetName);

            var headerRow = worksheet.Row(1);
            var columnMap = new Dictionary<string, int>();

            int colCount = 1;
            while (!headerRow.Cell(colCount).IsEmpty())
            {
                string header = headerRow.Cell(colCount).GetValue<string>().Trim();
                if (!string.IsNullOrEmpty(header))
                {
                    columnMap[header] = colCount;
                }
                colCount++;
            }

            var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                var rowData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var kvp in columnMap)
                {
                    rowData[kvp.Key] = row.Cell(kvp.Value).GetValue<string>();
                }
                rowData["__RowNumber"] = row.RowNumber().ToString();

                yield return rowData;
            }
        }

        public Task<Stream> GenerateExcelAsync<T>(IEnumerable<T> data, List<string> properties, List<string>? headers = null, CancellationToken cancellationToken = default) where T : class
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(typeof(T).Name);

            var exportHeaders = headers ?? properties;

            for (int i = 0; i < exportHeaders.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = exportHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            int rowIdx = 2;
            foreach (var item in data)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for (int i = 0; i < properties.Count; i++)
                {
                    var prop = typeof(T).GetProperty(properties[i]);
                    if (prop == null) continue;
                    var val = prop.GetValue(item);
                    if (val != null) worksheet.Cell(rowIdx, i + 1).SetValue(XLCellValue.FromObject(val));
                }
                rowIdx++;
            }

            worksheet.Columns().AdjustToContents();

            var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            memoryStream.Position = 0;
            return Task.FromResult<Stream>(memoryStream);
        }

        public Task<Stream> GenerateTemplateAsync(
            List<string> columns,
            string sheetName = "Template",
            IReadOnlyCollection<string>? requiredColumns = null,
            CancellationToken cancellationToken = default)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            // Case-insensitive lookup so we match whether the caller passed property
            // names ("NameAR") or display names ("Name AR").
            var required = requiredColumns is null
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(requiredColumns, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < columns.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var raw = columns[i];
                var isRequired = required.Contains(raw);
                var cell = worksheet.Cell(1, i + 1);

                cell.Value = isRequired ? $"{raw} *" : raw;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;

                if (isRequired)
                {
                    // Red text signals required at a glance; a cell comment spells it out
                    // for users who don't know the convention.
                    cell.Style.Font.FontColor = XLColor.Red;
                    cell.GetComment().AddText("Required field — the importer will reject rows that leave this blank.");
                }
            }

            worksheet.Columns().AdjustToContents();

            var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            memoryStream.Position = 0;
            return Task.FromResult<Stream>(memoryStream);
        }

        /// <summary>
        /// Builds an XLSX workbook from an async row stream and writes it to <paramref name="output"/>.
        /// </summary>
        public async Task WriteAsync<T>(
            Stream output,
            IAsyncEnumerable<T> rows,
            IReadOnlyList<string> headers,
            Func<T, int, object?> getCellValue,
            string sheetName,
            CancellationToken cancellationToken = default)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            for (int i = 0; i < headers.Count; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            int rowIdx = 2;
            const int yieldEvery = 500;

            await foreach (var item in rows.WithCancellation(cancellationToken))
            {
                for (int c = 0; c < headers.Count; c++)
                {
                    var val = getCellValue(item, c);
                    if (val != null) worksheet.Cell(rowIdx, c + 1).SetValue(XLCellValue.FromObject(val));
                }

                rowIdx++;
                if (rowIdx % yieldEvery == 0) await Task.Yield();
            }

            worksheet.Columns().AdjustToContents();

            // ClosedXML SaveAs is synchronous → buffer into MemoryStream, then async-copy out.
            using var buffer = new MemoryStream();
            workbook.SaveAs(buffer);
            buffer.Position = 0;
            await buffer.CopyToAsync(output, cancellationToken);
        }
    }
}
