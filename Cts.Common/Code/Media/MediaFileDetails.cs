using System;
using System.IO;

namespace Gemelo.Components.Cts.Code.Media
{
    public class MediaFileDetails
    {
        public string Filename { get; set; }

        public long Length { get; set; }

        public DateTime LastWriteTime { get; set; }

        public static MediaFileDetails From(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return new MediaFileDetails
            {
                Filename = fileInfo.Name,
                Length = fileInfo.Length,
                LastWriteTime = fileInfo.LastWriteTimeUtc
            };        
        }

        public bool IsUpToDate(string otherFilePath)
        {
            MediaFileDetails other = From(otherFilePath);
            return Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (obj is MediaFileDetails other) return Equals(other);
            return base.Equals(obj);
        }

        public bool Equals(MediaFileDetails other)
        {
            return Filename == other.Filename && Length == other.Length && LastWriteTime == other.LastWriteTime;
        }

        public override int GetHashCode()
        {
            return Filename.GetHashCode() ^ Length.GetHashCode() ^ LastWriteTime.GetHashCode();
        }
    }
}
