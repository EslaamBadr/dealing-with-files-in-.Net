using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Bussieness.DTOs.Files;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using Newtonsoft.Json;
using CsvHelper;
using System.IO;
using System.Text;
using System.Security.Policy;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;

namespace Receits.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        [HttpPost]
        [Route("fileUpload")]
        public ActionResult<FileUploadDto> Upload(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            var newFileName = $"data{extension}";
            var filePath = Path.Combine(Environment.CurrentDirectory, "Files");
            var fulFilePath = Path.Combine(filePath, newFileName);
            using var stream = new FileStream(fulFilePath, FileMode.Create);
            file.CopyTo(stream);
            var url = $"{Request.Scheme}://{Request.Host}/Files/{newFileName}";
            return new FileUploadDto(true, "Suceess", url);
        }

        [HttpGet("converToJson")]
        public IActionResult convertToJSon()
        {
            {
                var csvFilePath = Path.Combine(Environment.CurrentDirectory, "Files/data.csv");
                if (!System.IO.File.Exists(csvFilePath))
                    return NotFound("CSV file not found.");

                using (var reader = new StreamReader(csvFilePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<dynamic>().ToList();
                    return Ok(records);
                }
            }
        }
        [HttpGet("downloadAsXLSX")]
        public async Task<IActionResult> Download()
        {
            // Generate the XLSX file using EPPlus
            var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                var csvFilePath = Path.Combine(Environment.CurrentDirectory, "Files/data.csv");
                if (!System.IO.File.Exists(csvFilePath))
                    return NotFound("CSV file not found.");
                using (var reader = new StreamReader(csvFilePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<dynamic>().ToList();
                    int row = 1;
                    foreach (var csvRow in records)
                    {
                        var csvColumns = csvRow;
                        int column = 1;
                        foreach (var csvColumn in csvColumns)
                        {
                            worksheet.Cells[row, column].Value = csvColumn;
                            column++;
                        }
                        row++;
                    }
                }
                // Auto-fit the columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                await package.SaveAsync();
            }

            // Reset the stream position to the beginning
            stream.Position = 0;

            // Set the content type and headers for the response
            var contentDisposition = new System.Net.Mime.ContentDisposition
            {
                FileName = "data.xlsx",
                Inline = false
            };
            Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
            Response.Headers.Add("X-Content-Type-Options", "nosniff");
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

            // Return the file as a downloadable response
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
    }
}
