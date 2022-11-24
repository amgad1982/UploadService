namespace UploadService.Api.Models;

public record SessionSettings
{
    public Guid SessionID { get; set; }
    public string SessionFileName { get; set; }
    public int SessionTotalParts { get; set; }
}
