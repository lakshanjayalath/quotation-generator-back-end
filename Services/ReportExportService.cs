using System.Data;
using System.Globalization;
using System.Text;
using CsvHelper;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;

namespace quotation_generator_back_end.Services
{
    public interface IReportExportService
    {
        byte[] ExportCsv(DataTable table);
        byte[] ExportExcel(DataTable table, string reportType = "Report");
        byte[] ExportPdf(DataTable table, string title);
    }

    public class ReportExportService : IReportExportService
    {
        public byte[] ExportCsv(DataTable table)
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            foreach (DataColumn col in table.Columns) csv.WriteField(col.ColumnName);
            csv.NextRecord();

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    csv.WriteField(row[col] ?? string.Empty);
                }
                csv.NextRecord();
            }
            writer.Flush();
            return ms.ToArray();
        }

        public byte[] ExportExcel(DataTable table, string reportType = "Report")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Report");

            var headerColorHex = "FFBC4749";
            var altRowColorHex = "FFF5F5F5";

            ws.Cells[1, 1].Value = $"{reportType} Report";
            ws.Cells[1, 1].Style.Font.Size = 14;
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(188, 71, 73));
            ws.Cells[1, 1, 1, table.Columns.Count].Merge = true;

            ws.Cells[2, 1].Value = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            ws.Cells[2, 1].Style.Font.Size = 10;
            ws.Cells[2, 1].Style.Font.Italic = true;
            ws.Cells[2, 1, 2, table.Columns.Count].Merge = true;

            int headerRow = 4;
            for (int c = 0; c < table.Columns.Count; c++)
            {
                var cell = ws.Cells[headerRow, c + 1];
                cell.Value = table.Columns[c].ColumnName;
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(188, 71, 73));
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            for (int r = 0; r < table.Rows.Count; r++)
            {
                int excelRow = headerRow + r + 1;
                bool isAltRow = r % 2 == 0;

                for (int c = 0; c < table.Columns.Count; c++)
                {
                    var cell = ws.Cells[excelRow, c + 1];
                    var value = table.Rows[r][c];

                    if (value is decimal decVal)
                    {
                        cell.Value = decVal;
                        cell.Style.Numberformat.Format = "$#,##0.00";
                    }
                    else if (value is int intVal)
                    {
                        cell.Value = intVal;
                    }
                    else if (value is bool boolVal)
                    {
                        cell.Value = boolVal ? "Yes" : "No";
                    }
                    else
                    {
                        cell.Value = value ?? string.Empty;
                    }

                    if (isAltRow)
                    {
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(245, 245, 245));
                    }

                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
            }

            for (int c = 1; c <= table.Columns.Count; c++)
            {
                ws.Column(c).Width = ws.Column(c).Width < 15 ? 15 : ws.Column(c).Width + 2;
            }

            int footerRow = headerRow + table.Rows.Count + 2;
            ws.Cells[footerRow, 1].Value = $"Total Records: {table.Rows.Count}";
            ws.Cells[footerRow, 1].Style.Font.Bold = true;
            ws.Cells[footerRow, 1].Style.Font.Size = 11;
            ws.Cells[footerRow, 1, footerRow, table.Columns.Count].Merge = true;

            return package.GetAsByteArray();
        }

        public byte[] ExportPdf(DataTable table, string title)
        {
            var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var titleFont = new XFont("Calibri", 16, XFontStyle.Bold);
            var headerFont = new XFont("Calibri", 10, XFontStyle.Bold);
            var dataFont = new XFont("Calibri", 9);
            var headerBrush = new XSolidBrush(XColor.FromArgb(188, 71, 73)); // #BC4749
            var alternateRowBrush = new XSolidBrush(XColor.FromArgb(245, 245, 245));

            double margin = 20;
            double y = margin;
            double pageWidth = page.Width;
            double pageHeight = page.Height;

            // Draw title
            gfx.DrawString(title, titleFont, XBrushes.Black, new XRect(margin, y, pageWidth - 2 * margin, 30), XStringFormats.TopLeft);
            y += 28;

            // Draw timestamp
            gfx.DrawString($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", dataFont, XBrushes.Gray, new XRect(margin, y, pageWidth - 2 * margin, 15), XStringFormats.TopLeft);
            y += 20;

            int colCount = table.Columns.Count;
            var availableWidth = pageWidth - 2 * margin;

            // Measure preferred widths for each column based on header and cell content
            double[] measuredWidths = new double[colCount];
            for (int c = 0; c < colCount; c++)
            {
                // Start with header width
                var hdr = table.Columns[c].ColumnName ?? string.Empty;
                var size = gfx.MeasureString(hdr, headerFont);
                double maxW = size.Width + 8; // padding

                // Measure each cell in column
                foreach (DataRow row in table.Rows)
                {
                    var txt = row[c]?.ToString() ?? string.Empty;
                    var ts = gfx.MeasureString(txt, dataFont);
                    if (ts.Width + 8 > maxW) maxW = ts.Width + 8;
                }

                // Enforce a reasonable min and max per-column
                const double minCol = 40;
                double maxCol = availableWidth * 0.6; // any single column shouldn't take more than 60% initially
                measuredWidths[c] = Math.Max(minCol, Math.Min(maxW, maxCol));
            }

            // If total measured width exceeds available width, scale down proportionally
            double totalMeasured = measuredWidths.Sum();
            double[] colWidths = new double[colCount];
            if (totalMeasured <= availableWidth)
            {
                // Use measured widths, but distribute remaining space evenly
                double remaining = availableWidth - totalMeasured;
                double extraPerCol = remaining / colCount;
                for (int c = 0; c < colCount; c++) colWidths[c] = measuredWidths[c] + extraPerCol;
            }
            else
            {
                double scale = availableWidth / totalMeasured;
                for (int c = 0; c < colCount; c++) colWidths[c] = Math.Max(40, measuredWidths[c] * scale);
                // If rounding error, adjust last column
                double sumCols = colWidths.Sum();
                if (sumCols < availableWidth) colWidths[colCount - 1] += (availableWidth - sumCols);
                else if (sumCols > availableWidth) colWidths[colCount - 1] -= (sumCols - availableWidth);
            }

            // Header row height
            double headerHeight = gfx.MeasureString("Mg", headerFont).Height + 6;

            // Function to wrap text into lines that fit width
            List<string> WrapText(string text, XFont font, double maxWidth)
            {
                var lines = new List<string>();
                if (string.IsNullOrEmpty(text)) { lines.Add(string.Empty); return lines; }
                var words = text.Split(' ');
                var cur = new StringBuilder();
                foreach (var w in words)
                {
                    var trial = cur.Length == 0 ? w : cur + " " + w;
                    var measure = gfx.MeasureString(trial, font).Width;
                    if (measure + 4 <= maxWidth)
                    {
                        if (cur.Length > 0) cur.Append(' ');
                        cur.Append(w);
                    }
                    else
                    {
                        if (cur.Length > 0) { lines.Add(cur.ToString()); cur.Clear(); }
                        // If single word is longer than width, break the word
                        if (gfx.MeasureString(w, font).Width + 4 > maxWidth)
                        {
                            var chunk = w;
                            while (gfx.MeasureString(chunk, font).Width + 4 > maxWidth && chunk.Length > 1)
                            {
                                int take = Math.Max(1, (int)(chunk.Length * (maxWidth / gfx.MeasureString(chunk, font).Width)) - 1);
                                if (take <= 0) take = 1;
                                lines.Add(chunk.Substring(0, take));
                                chunk = chunk.Substring(take);
                            }
                            if (!string.IsNullOrEmpty(chunk)) cur.Append(chunk);
                        }
                        else
                        {
                            cur.Append(w);
                        }
                    }
                }
                if (cur.Length > 0) lines.Add(cur.ToString());
                return lines;
            }

            // Draw header
            double x = margin;
            for (int c = 0; c < colCount; c++)
            {
                gfx.DrawRectangle(headerBrush, x, y, colWidths[c], headerHeight);
                gfx.DrawString(table.Columns[c].ColumnName, headerFont, XBrushes.White, new XRect(x + 4, y + 2, colWidths[c] - 8, headerHeight - 4), XStringFormats.TopLeft);
                x += colWidths[c];
            }
            y += headerHeight;

            // Draw rows with dynamic heights and wrapping
            foreach (DataRow row in table.Rows)
            {
                // Determine required row height based on wrapped cell lines
                double rowTop = y;
                double maxRowHeight = 0;
                var cellLinesPerCol = new List<List<string>>(colCount);
                for (int c = 0; c < colCount; c++)
                {
                    var txt = row[c]?.ToString() ?? string.Empty;
                    var lines = WrapText(txt, dataFont, colWidths[c] - 8);
                    cellLinesPerCol.Add(lines);
                    double lineHeight = gfx.MeasureString("Mg", dataFont).Height;
                    double h = lines.Count * lineHeight + 6;
                    if (h > maxRowHeight) maxRowHeight = h;
                }

                // Start a new page if not enough space
                if (rowTop + maxRowHeight > pageHeight - margin)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;

                    // redraw header on new page
                    x = margin;
                    for (int c = 0; c < colCount; c++)
                    {
                        gfx.DrawRectangle(headerBrush, x, y, colWidths[c], headerHeight);
                        gfx.DrawString(table.Columns[c].ColumnName, headerFont, XBrushes.White, new XRect(x + 4, y + 2, colWidths[c] - 8, headerHeight - 4), XStringFormats.TopLeft);
                        x += colWidths[c];
                    }
                    y += headerHeight;
                    rowTop = y;
                }

                // Draw alternating row background
                int rowIndex = table.Rows.IndexOf(row);
                if (rowIndex % 2 == 1)
                {
                    gfx.DrawRectangle(alternateRowBrush, margin, rowTop, availableWidth, maxRowHeight);
                }

                // Draw cell text lines
                x = margin;
                for (int c = 0; c < colCount; c++)
                {
                    var lines = cellLinesPerCol[c];
                    double lineHeight = gfx.MeasureString("Mg", dataFont).Height;
                    double textY = rowTop + 3;
                    foreach (var line in lines)
                    {
                        gfx.DrawString(line, dataFont, XBrushes.Black, new XRect(x + 4, textY, colWidths[c] - 8, lineHeight), XStringFormats.TopLeft);
                        textY += lineHeight;
                    }

                    // Draw vertical separators (optional)
                    gfx.DrawRectangle(XPens.LightGray, x, rowTop, colWidths[c], maxRowHeight);

                    x += colWidths[c];
                }

                y += maxRowHeight;
            }

            // Draw footer
            y += 8;
            if (y + 15 > pageHeight - margin)
            {
                page = doc.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                y = margin;
            }
            gfx.DrawString($"Total Records: {table.Rows.Count}", dataFont, XBrushes.Gray, new XRect(margin, y, pageWidth - 2 * margin, 15), XStringFormats.TopLeft);

            using var ms = new MemoryStream();
            doc.Save(ms);
            return ms.ToArray();
        }
    }
}
