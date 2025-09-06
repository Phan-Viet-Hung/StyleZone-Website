using ClosedXML.Excel;
using System.Globalization;
using System.Reflection;

namespace API.Domain.Extentions
{
    public static class ExcelHelper
    {
        // columnPropertyMap: optional map từ header Excel => tên property C# (nếu header khác tên property)
        public static List<T> ImportToList<T>(Stream stream, string sheetName = null, Dictionary<string, string>? columnPropertyMap = null) where T : new()
        {
            var list = new List<T>();
            using var workbook = new XLWorkbook(stream);
            IXLWorksheet worksheet = string.IsNullOrWhiteSpace(sheetName) ? workbook.Worksheets.First() : workbook.Worksheet(sheetName);
            if (worksheet == null) throw new Exception($"Sheet '{sheetName}' not found.");

            var firstRow = worksheet.FirstRowUsed();
            if (firstRow == null) return list;

            var headerCells = firstRow.CellsUsed().ToList();
            var headers = headerCells.Select(c => c.GetValue<string>().Trim()).ToList();

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // build mapping: column index -> PropertyInfo (or null nếu không map được)
            var colMap = headers.Select((h, idx) =>
            {
                string propName = h;
                if (columnPropertyMap != null && columnPropertyMap.TryGetValue(h, out var mapped)) propName = mapped;
                var prop = props.FirstOrDefault(p => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));
                return new { Index = idx, Header = h, Prop = prop };
            }).ToList();

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                if (row.CellsUsed().All(c => string.IsNullOrWhiteSpace(c.GetValue<string>()))) continue; // skip empty row

                var obj = new T();
                foreach (var m in colMap)
                {
                    if (m.Prop == null) continue;
                    var cell = row.Cell(m.Index + 1);
                    if (cell == null || cell.IsEmpty()) continue;

                    var cellVal = cell.Value;
                    var converted = ConvertCellToPropertyType(cellVal, m.Prop.PropertyType);
                    if (converted != null)
                    {
                        m.Prop.SetValue(obj, converted);
                    }
                }
                list.Add(obj);
            }

            return list;
        }

        static object? ConvertCellToPropertyType(object? cellValue, Type targetType)
        {
            if (cellValue == null) return null;
            var text = cellValue.ToString()?.Trim();
            if (string.IsNullOrEmpty(text)) return null;

            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try
            {
                if (underlying == typeof(string)) return text;
                if (underlying == typeof(Guid)) return Guid.Parse(text);
                if (underlying.IsEnum) return Enum.Parse(underlying, text, ignoreCase: true);
                if (underlying == typeof(bool))
                {
                    if (bool.TryParse(text, out var bv)) return bv;
                    if (int.TryParse(text, out var bi)) return bi != 0;
                    var low = text.ToLowerInvariant();
                    if (low is "x" or "yes" or "y") return true;
                    return false;
                }
                if (underlying == typeof(DateTime) && DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return dt;
                // numeric types
                if (underlying == typeof(int) && int.TryParse(text, out var i)) return i;
                if (underlying == typeof(long) && long.TryParse(text, out var l)) return l;
                if (underlying == typeof(decimal) && decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec)) return dec;
                if (underlying == typeof(double) && double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
                if (underlying == typeof(float) && float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)) return f;

                // fallback to Convert.ChangeType
                return Convert.ChangeType(text, underlying, CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }
    }

}
