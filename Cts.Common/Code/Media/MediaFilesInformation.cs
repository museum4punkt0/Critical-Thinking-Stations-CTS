using System;
using System.Collections.Generic;

namespace Gemelo.Components.Cts.Code.Media
{
    public class MediaFilesInformation
    {
        public MediaFileDetails MainFile { get; set; }

        public Dictionary<MediaSupportFileType, MediaFileDetails> SupportFiles { get; }
            = new Dictionary<MediaSupportFileType, MediaFileDetails>();

        public static MediaFilesInformation From(string filePath)
        {
            MediaFilesInformation result = new MediaFilesInformation
            {
                MainFile = MediaFileDetails.From(filePath)
            };
            foreach (MediaSupportFileType supportFileType in Enum.GetValues<MediaSupportFileType>())
            {
                if (MediaFileHelper.ExistsMediaSupportFile(filePath, supportFileType, out string supportFilePath))
                {
                    result.SupportFiles.Add(supportFileType, MediaFileDetails.From(supportFilePath));
                }
            }
            return result;
        }
    }
}
