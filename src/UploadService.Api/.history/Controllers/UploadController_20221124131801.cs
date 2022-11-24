using Microsoft.AspNetCore.Mvc;
using UploadService.Api.Models;

namespace UploadService.Api.Controllers;

[Route("upload")]
[ApiController]
public class UploadController : ControllerBase
{
    [HttpPost("session")]
    public async Task<IActionResult> CreateUploadSession(FileUploadSessionDto session)
    {
        var sessionCorrelationId = (await FileUploadSession.Create(session.FileName, session.TotalParts)).SessionCorrelationId;

        return Ok(sessionCorrelationId);
    }
    [HttpPost("part/{sessionCorrelationId}/{partNumber}")]
    public async Task<IActionResult> UploadFilePart([FromBody] byte[] filePart, Guid sessionCorrelationId, int partNumber)
    {
        var session = await FileUploadSession.Get(sessionCorrelationId);
        session.AddPart(new FilePart(sessionCorrelationId, partNumber, filePart));
        return Ok();
    }

    [HttpPost("complete/{sessionCorrelationId}")]
    public async Task<IActionResult> CompleteUploadSession(Guid sessionCorrelationId)
    {
        var session = await FileUploadSession.Get(sessionCorrelationId);
        if (session.IsCompleted)
        {
            session.CompleteSession(sessionCorrelationId);
            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }
}
