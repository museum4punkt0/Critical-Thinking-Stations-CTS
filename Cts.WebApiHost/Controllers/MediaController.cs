using Gemelo.Components.Common.Tracing;
using Gemelo.Components.Cts.Code.Communication;
using Gemelo.Components.Cts.Database.Databases;
using Gemelo.Components.Cts.Code.Media;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

namespace Gemelo.Components.Cts.WebApiHost.Controllers
{
    [ApiController]
    public class MediaController : ControllerBase
    {
        private const string ConstMimeJson = "application/json";

        private CtsDatabaseContext m_Context;

        public MediaController(CtsDatabaseContext context)
        {
            m_Context = context;
        }

        [HttpPost]
        [Route("media/GetMediaFilesInformation")]
        public IActionResult GetMediaFilesInformation(GetMediaFilesInformationRequest request)
        {
            try
            {
                string filePath = Path.Combine(CtsWebApiHost.MediaDirectoryPath, request.Filename);
                MediaFilesInformation mediaFilesInformation = MediaFilesInformation.From(filePath);
                return Content(new GetMediaFilesInformationResult
                {
                    IsSuccessful = true,
                    MediaFilesInformation = mediaFilesInformation
                }.ToJson(), ConstMimeJson);
            }
            catch (Exception exception)
            {
                string message = $"Exception while trying to get media files information for filename: " +
                    $"{request?.Filename}: {exception.Message}!";
                TraceX.WriteWarning(
                    message: message,
                    arguments: $"filename: {request?.Filename}, questionID: {request?.QuestionID}",
                    category: nameof(MediaController));
                return Content(new GetMediaFilesInformationResult
                {
                    IsSuccessful = false,
                    Message = message
                }.ToJson(), ConstMimeJson);
            }
        }

        [HttpPost]
        [Route("media/DownloadMediaFile")]
        public IActionResult DownloadMediaFile(DownloadMediaFileRequest request)
        {
            string filePath = Path.Combine(CtsWebApiHost.MediaDirectoryPath, request.Filename);
            if (System.IO.File.Exists(filePath))
            {
                Stream stream = System.IO.File.OpenRead(filePath);
                return File(stream, "application/octet-stream");
            }
            else return NotFound();
        }
    }
}
