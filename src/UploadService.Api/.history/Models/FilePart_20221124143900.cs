namespace UploadService.Api.Models;

public class FilePart
{
    public FilePart(Guid sessionCorrelationId, int partNumber, //byte[] content)
    {
        SessionCorrelationId = sessionCorrelationId;
        PartNumber = partNumber;
        Content = content;
    }
    public Guid SessionCorrelationId { get; private set; }
    public int PartNumber { get; private set; }
    public byte[] Content { get; private set; }
}
