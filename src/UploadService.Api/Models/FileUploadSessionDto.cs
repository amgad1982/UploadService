namespace UploadService.Api.Models;

public class FileUploadSessionDto
{
    public string FileName { get; set; }
    public int TotalParts { get; set; }
}
