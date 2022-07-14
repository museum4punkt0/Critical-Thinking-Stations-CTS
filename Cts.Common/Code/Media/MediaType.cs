using System.IO;

namespace Gemelo.Components.Cts.Code.Media
{
    public enum MediaType
    {
        None,
        Image,
        Video,
        Audio
    }

    public static class MediaTypeExtensions
    {
        public static string ToGermanString(this MediaType mediaType)
        {
            switch (mediaType)
            {
                default:
                case MediaType.None:
                    return "Kein Medium";
                case MediaType.Image:
                    return "Bild";
                case MediaType.Video:
                    return "Video";
                case MediaType.Audio:
                    return "Audio";
            }
        }
    }
}
