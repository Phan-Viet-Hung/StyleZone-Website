using API.Domain.Extentions;
using API.Domain.Request.ImportRequest;
using API.Domain.Service;
using DAL_Empty.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly DbContextApp _dbContext;
        private readonly ExcelImporter _excelImporter;

        public ImportController(DbContextApp dbContext, IServiceProvider serviceProvider)
        {
            _dbContext = dbContext;
            _excelImporter = new ExcelImporter(serviceProvider);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcel([FromForm] ImportRequest request, [FromForm] string? ignoreFields = null)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("File trống.");

            // Parse ignoreFields JSON string thành List<string>
            List<string>? ignoreFieldsList = null;
            if (!string.IsNullOrEmpty(ignoreFields))
            {
                try
                {
                    ignoreFieldsList = JsonSerializer.Deserialize<List<string>>(ignoreFields);
                }
                catch
                {
                    return BadRequest("ignoreFields không đúng định dạng JSON array.");
                }
            }

            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xlsx");

            using (var stream = System.IO.File.Create(tempFile))
            {
                await request.File.CopyToAsync(stream);
            }

            try
            {
                await _excelImporter.ImportExcelFileAsync(request.EntityName, tempFile, _dbContext, ignoreFieldsList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            finally
            {
                if (System.IO.File.Exists(tempFile))
                    System.IO.File.Delete(tempFile);
            }

            return Ok("Import thành công.");
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportExcel(string entityName)
        {
            try
            {
                var fileContent = await _excelImporter.ExportExcelFileAsync(entityName, _dbContext);
                var fileName = $"{entityName}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
