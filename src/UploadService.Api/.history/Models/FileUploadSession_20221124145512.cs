using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UploadService.Api.Models;

public class FileUploadSession
{
    private FileUploadSession(Guid sessionCorrelationId, string fileName, int totalParts)
    {
        SessionCorrelationId = sessionCorrelationId;
        FileName = fileName;
        TotalParts = totalParts;
    }
    public Guid SessionCorrelationId { get; private set; }
    public string FileName { get; private set; }
    public int TotalParts { get; private set; }
    public int UploadedParts { get; private set; }
    public bool IsCompleted
    {
        get
        {
            return _fileParts.Count == TotalParts;
        }
    }

    private readonly List<FilePart> _fileParts = new List<FilePart>();
    private async Task Load()
    {
            //check if folder with sessionCorrelationId exists
            if (Directory.Exists(Path.Combine("Uploads/Temp", SessionCorrelationId.ToString())))
            {
                //load all files from folder
                var files = Directory.GetFiles(Path.Combine("Uploads/Temp", SessionCorrelationId.ToString()), "*.part");
                foreach (var file in files)
                {
                    //load file content
                    //var content = await System.IO.File.ReadAllBytesAsync(file);
                    //create FilePart
                    var filePart = new FilePart(SessionCorrelationId, int.Parse(Path.GetFileNameWithoutExtension(file)));//, content);
                    //add to list
                    _fileParts.Add(filePart);
                }
            }
            else
            {
                //create folder
                Directory.CreateDirectory(Path.Combine("Uploads/Temp", SessionCorrelationId.ToString()));
                //save session data
                await File.WriteAllTextAsync(Path.Combine("Uploads/Temp", SessionCorrelationId.ToString(), "session.json"), JsonConvert.SerializeObject(new SessionSettings
                {
                    SessionID = SessionCorrelationId,
                    SessionFileName = FileName,
                    SessionTotalParts = TotalParts
                }));
            }
        }

    
    public async void AddPart(FilePart filePart,byte[] content)
    {
        try
        {
            if (filePart.SessionCorrelationId != SessionCorrelationId)
                throw new ArgumentException("SessionCorrelationId does not match");
            // if (filePart.PartNumber != UploadedParts+1)
            //     throw new ArgumentException("PartNumber does not match");
            _fileParts.Add(filePart);
            //save file to disk
            using(var fileStream = new FileStream(Path.Combine("Uploads/Temp", SessionCorrelationId.ToString(), $"{filePart.PartNumber}.part"), FileMode.Create))
            {
                await fileStream.WriteAsync(content);
            }
            
           
        }
        catch (Exception ex)
        {
            throw ex;
        }
        

    }
    public async void CompleteSession(Guid sessionCorrelationId)
    {
        
        try
        {
             //if all parts are uploaded,compine all parts in one file ,move file to final folder,remove session folder
            if (IsCompleted)
            {
                 var files = Directory.GetFiles(Path.Combine("Uploads/Temp", sessionCorrelationId.ToString()), "*.part");
                 foreach (var file in files)
                 {
                     //load file content
                     var content = await System.IO.File.ReadAllBytesAsync(file);
                     //append to final file
                     using(var fileStream = new FileStream(Path.Combine("Uploads", FileName), FileMode.Append))
                     {
                         await fileStream.WriteAsync(content);
                     }
                 }
              
                 //remove session folder
                Directory.Delete(Path.Combine("Uploads/Temp", sessionCorrelationId.ToString()), true);

            }
           
        }
        catch (Exception ex)
        {
            throw ex;
        }
       
    }
    public static async Task<FileUploadSession> Create(string fileName, int totalParts)
    {
        var sessionCorrelationId = Guid.NewGuid();
        var session = new FileUploadSession(sessionCorrelationId, fileName, totalParts);
        await session.Load();
        return session;
    }
    public static async Task<FileUploadSession> Get(Guid sessionCorrelationId)
    {
        //load session data from disk
        var sessionData = JsonConvert.DeserializeObject<SessionSettings>(await File.ReadAllTextAsync(Path.Combine("Uploads/Temp", sessionCorrelationId.ToString(), "session.json")));
        var session = new FileUploadSession(sessionCorrelationId, sessionData?.SessionFileName??"", sessionData?.SessionTotalParts??);
        await session.Load();
        return session;
    }

}