using API.Domain.Mappers;
using API.Domain.Request.ProductRequest;
using API.Domain.Validate;
using API.Domain.Validate.IExcelValidator;
using ClosedXML.Excel;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace API.Domain.Service
{
    public class ExcelImporter
    {
        private readonly IServiceProvider _serviceProvider;

        public ExcelImporter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Thêm param ignoreFields vào đây
        public async Task ImportExcelFileAsync(string entityName, string filePath, DbContext dbContext, List<string>? ignoreFields = null)
        {
            var typeEntity = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));

            if (typeEntity == null)
                throw new Exception($"Không tìm thấy class '{entityName}' trong các Assembly.");

            var requestTypeName = "API.Domain.Request." + entityName + "Request.Create" + entityName + "Request";
            var requestType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName == requestTypeName);

            if (requestType == null)
                throw new Exception($"Không tìm thấy request class '{requestTypeName}'.");

            var method = typeof(ExcelImporter).GetMethod(nameof(ImportToDbSetAsync), BindingFlags.NonPublic | BindingFlags.Instance);
            var genericMethod = method.MakeGenericMethod(typeEntity, requestType);

            // Truyền ignoreFields vào generic method
            await (Task)genericMethod.Invoke(this, new object[] { filePath, dbContext, ignoreFields });
        }


        // Thêm param ignoreFields
        private async Task ImportToDbSetAsync<TEntity, TRequest>(string filePath, DbContext dbContext, List<string>? ignoreFields = null)
    where TEntity : class, new()
    where TRequest : class, new()
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheets.First();

            var entityProperties = typeof(TEntity).GetProperties().ToList();
            var requestProperties = typeof(TRequest).GetProperties().ToList();

            var headerRow = worksheet.Row(1);
            var headerMap = new Dictionary<int, PropertyInfo>();
            foreach (var cell in headerRow.CellsUsed())
            {
                var headerText = cell.GetString().Trim();
                var prop = requestProperties.FirstOrDefault(p => p.Name.Equals(headerText, StringComparison.OrdinalIgnoreCase));
                if (prop != null)
                {
                    headerMap[cell.Address.ColumnNumber] = prop;
                }
            }

            var existingData = await dbContext.Set<TEntity>().AsNoTracking().ToListAsync();

            PropertyInfo? keyProp = typeof(TEntity).GetProperty("Id");
            if (keyProp == null)
                throw new Exception($"Class {typeof(TEntity).Name} không có property 'Id' để check trùng.");

            // Hàm tạo key từ 3 property trong TRequest (ưu tiên lấy từ request vì dữ liệu nhập từ đó)
            string MakeKey(TRequest obj)
            {
                string getPropVal(string propName)
                {
                    var prop = requestProperties.FirstOrDefault(p => p.Name == propName);
                    if (prop == null) return "";
                    return prop.GetValue(obj)?.ToString()?.Trim() ?? "";
                }
                var name = getPropVal("Name");
                var code = getPropVal("Code");
                var desc = getPropVal("Description");
                return $"{name}|{code}|{desc}".ToLower();
            }

            // Tạo tập hợp các key đã tồn tại trong DB
            var existingKeys = new HashSet<string>(
                existingData.Select(entity =>
                {
                    // Lấy 3 prop từ entity để tạo key
                    string getPropVal(string propName)
                    {
                        var prop = entityProperties.FirstOrDefault(p => p.Name == propName);
                        if (prop == null) return "";
                        return prop.GetValue(entity)?.ToString()?.Trim() ?? "";
                    }
                    var name = getPropVal("Name");
                    var code = getPropVal("Code");
                    var desc = getPropVal("Description");
                    return $"{name}|{code}|{desc}".ToLower();
                }),
                StringComparer.OrdinalIgnoreCase
            );

            var rows = worksheet.RowsUsed().Skip(1);

            var newEntities = new List<TEntity>();
            var allErrors = new List<string>();
            var seenKeysInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int rowIndex = 2;
            foreach (var row in rows)
            {
                if (row.IsEmpty())
                {
                    rowIndex++;
                    continue;
                }

                var requestObj = new TRequest();
                foreach (var kv in headerMap)
                {
                    var colIdx = kv.Key;
                    var prop = kv.Value;
                    var cell = row.Cell(colIdx);
                    if (cell.IsEmpty())
                        continue;

                    object? val = ConvertCellValue(cell, prop.PropertyType);
                    if (val != null)
                        prop.SetValue(requestObj, val);
                }

                var validationResults = new List<ValidationResult>();
                var ctx = new ValidationContext(requestObj, null, null);
                bool isValid = Validator.TryValidateObject(requestObj, ctx, validationResults, true);

                if (!isValid)
                {
                    // Lọc bỏ lỗi những field nằm trong ignoreFields
                    if (ignoreFields != null && ignoreFields.Any())
                    {
                        validationResults = validationResults
                            .Where(vr => vr.MemberNames.All(mn => !ignoreFields.Contains(mn, StringComparer.OrdinalIgnoreCase)))
                            .ToList();

                        if (validationResults.Count == 0)
                        {
                            // Nếu tất cả lỗi bị bỏ qua => coi dòng hợp lệ
                            isValid = true;
                        }
                    }
                }

                if (!isValid)
                {
                    allErrors.Add($"Dòng {rowIndex}: {string.Join("; ", validationResults.Select(r => r.ErrorMessage))}");
                    rowIndex++;
                    continue; // Bỏ qua dòng lỗi
                }

                // Tạo key dòng hiện tại
                var currentKey = MakeKey(requestObj);

                // Nếu key rỗng hoặc đã tồn tại DB hoặc đã có trong file => bỏ qua
                if (string.IsNullOrWhiteSpace(currentKey.Replace("|", "")) || existingKeys.Contains(currentKey) || seenKeysInFile.Contains(currentKey))
                {
                    rowIndex++;
                    continue;
                }

                seenKeysInFile.Add(currentKey);

                var entity = new TEntity();
                foreach (var prop in entityProperties)
                {
                    var reqProp = requestProperties.FirstOrDefault(p => p.Name == prop.Name);
                    if (reqProp != null)
                    {
                        var value = reqProp.GetValue(requestObj);
                        prop.SetValue(entity, value);
                    }
                }

                var idValue = keyProp.GetValue(entity);
                if (idValue == null || idValue.Equals(Guid.Empty) || !existingKeys.Contains(idValue))
                {
                    newEntities.Add(entity);
                }
                rowIndex++;
            }

            if (allErrors.Any())
                throw new Exception("Lỗi dữ liệu Excel:\n" + string.Join("\n", allErrors));

            if (newEntities.Any())
            {
                await dbContext.Set<TEntity>().AddRangeAsync(newEntities);
                await dbContext.SaveChangesAsync();
            }
        }


        // Xuất Excel từ dữ liệu DB
        public async Task<byte[]> ExportExcelFileAsync(string entityName, DbContext dbContext)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));

            if (type == null)
                throw new Exception($"Không tìm thấy class '{entityName}' trong các Assembly đang load.");

            var method = typeof(ExcelImporter).GetMethod(nameof(ExportFromDbSetAsync), BindingFlags.NonPublic | BindingFlags.Instance);
            var genericMethod = method.MakeGenericMethod(type);
            var result = await (Task<byte[]>)genericMethod.Invoke(this, new object[] { dbContext });
            return result;
        }

        private async Task<byte[]> ExportFromDbSetAsync<T>(DbContext dbContext) where T : class, new()
        {
            var data = await dbContext.Set<T>().AsNoTracking().ToListAsync();
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(typeof(T).Name);

            var properties = typeof(T).GetProperties().ToList();

            // Ghi header
            for (int i = 0; i < properties.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = properties[i].Name;
            }

            // Ghi dữ liệu
            for (int rowIdx = 0; rowIdx < data.Count; rowIdx++)
            {
                var item = data[rowIdx];
                for (int colIdx = 0; colIdx < properties.Count; colIdx++)
                {
                    var propInfo = properties[colIdx];
                    var value = propInfo.GetValue(item);

                    if (value == null)
                    {
                        worksheet.Cell(rowIdx + 2, colIdx + 1).Value = "";
                    }
                    else
                    {
                        var type = value.GetType();

                        // Nếu là IEnumerable nhưng không phải string, convert từng phần tử thành string nối chuỗi
                        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
                        {
                            var enumerable = (System.Collections.IEnumerable)value;
                            var itemsStrings = new List<string>();
                            foreach (var e in enumerable)
                            {
                                if (e != null)
                                    itemsStrings.Add(e.ToString());
                            }
                            worksheet.Cell(rowIdx + 2, colIdx + 1).Value = string.Join(", ", itemsStrings);
                        }
                        else
                        {
                            worksheet.Cell(rowIdx + 2, colIdx + 1).Value = value.ToString();
                        }
                    }
                }
            }

            using var ms = new System.IO.MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        private object? ConvertCellValue(IXLCell cell, Type targetType)
        {
            if (cell.IsEmpty())
                return null;

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            var cellValue = cell.Value;

            try
            {
                if (underlyingType == typeof(string))
                    return cell.GetString();

                if (underlyingType == typeof(Guid))
                {
                    if (Guid.TryParse(cell.GetString(), out var g))
                        return g;
                    return null;
                }

                if (underlyingType.IsEnum)
                    return Enum.Parse(underlyingType, cell.GetString(), ignoreCase: true);

                if (underlyingType == typeof(bool))
                    return cell.GetBoolean();

                if (underlyingType == typeof(int))
                    return cell.GetValue<int>();

                if (underlyingType == typeof(long))
                    return cell.GetValue<long>();

                if (underlyingType == typeof(decimal))
                    return cell.GetValue<decimal>();

                if (underlyingType == typeof(double))
                    return cell.GetValue<double>();

                if (underlyingType == typeof(float))
                    return cell.GetValue<float>();

                if (underlyingType == typeof(DateTime))
                    return cell.GetDateTime();

                return Convert.ChangeType(cellValue, underlyingType);
            }
            catch
            {
                return null;
            }
        }
    }

}
