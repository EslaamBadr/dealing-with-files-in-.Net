using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bussieness.DTOs.Files;

public class FileUploadDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public FileUploadDto(bool isSuccess, string message, string url)
    {
        IsSuccess = isSuccess;
        Message = message;
        URL = url;

    }
}
