using Gemelo.Components.Common.Localization;
using System;
using System.Collections.Generic;
using System.IO;

namespace Gemelo.Components.Cts.Code.Media
{
    public static class MediaFileHelper
    {
        #region Konstanten

        public static readonly Dictionary<MediaType, List<string>> ConstExtensionsForTypes =
            new Dictionary<MediaType, List<string>>()
            {
                [MediaType.Image] = new List<string>() { ".png", ".jpg", ".jpeg" },
                [MediaType.Video] = new List<string>() { ".mp4", ".mov" },
                [MediaType.Audio] = new List<string>() { ".mp3", ".wav" }
            };

        public const string ConstMediaFileFilter = "Alle Mediendateien|*.mp4;*.mp3;*.jpg;*.png|" +
            "Videodatei|*.mp4|Audiodatei|*.mp3|Bilddateien|*.jpg;*.jpeg;*.png";

        private static readonly string[] ConstPreviewImageSuffixes = new string[]
        {
            ".preview.jpg",
            ".preview.png"
        };

        private static readonly string[] ConstPosterImageSuffixes = new string[]
        {
            ".jpg",
            ".png"
        };

        private const string ConstLocalizedMediaSuffixAndExtensionFormat = ".{0}{1}";
        private const string ConstLocalizedMediaFilenameFormat = "{0}.{1}{2}"; // <filename>.<language><extension>
        private const string ConstDefaultLanguageMediaFilenameSuffix = "." + Languages.German;

        private const string ConstSubtitleFilenameFormatSuffix = ".{0}.srt";

        private const string ConstPosterImageFilenameFormat = "{0}{1}";
        private const string ConstPreviewImageFilenameFormat = "{0}.preview{1}";
        private const string ConstLocalizedSubTitlesFilenameFormat = "{0}.{1}.srt";

        private const string ConstMediaFileVersionPrefix = "-v";

        #endregion Konstanten

        #region Öffentliche Methoden

        public static MediaType GetTypeFromFilename(string filename)
        {
            switch (filename?.ToLowerInvariant())
            {
                default:
                    string extension = Path.GetExtension(filename);
                    foreach (MediaType mediaType in MediaFileHelper.ConstExtensionsForTypes.Keys)
                    {
                        if (ConstExtensionsForTypes[mediaType].Contains(extension)) return mediaType;
                    }
                    return MediaType.None;
            }
        }

        public static string GetFilenameWithoutExtensionAndLocalization(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            if (filenameWithoutExtension.EndsWith(ConstDefaultLanguageMediaFilenameSuffix))
            {
                filenameWithoutExtension = filenameWithoutExtension.Substring(0,
                    filenameWithoutExtension.Length - ConstDefaultLanguageMediaFilenameSuffix.Length);
            }
            return filenameWithoutExtension;
        }

        public static string TryGetLocalizedMediaFilePathFor(string filePath, string language)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            string directory = Path.GetDirectoryName(filePath);
            string filenameWithoutExtension = GetFilenameWithoutExtensionAndLocalization(filePath);
            string extension = Path.GetExtension(filePath);
            string localizedFilePath = Path.Combine(directory, string.Format(ConstLocalizedMediaFilenameFormat,
                filenameWithoutExtension, language, extension));
            return File.Exists(localizedFilePath) ? localizedFilePath : null;
        }

        public static string TryGetSupportFilePathFor(string filePath, MediaSupportFileType supportFileType)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            ExistsMediaSupportFile(filePath, supportFileType, out string supportFilePath);
            return supportFilePath;
        }

        public static string GetSubTitleFilePathFormat(string filePath)
        {
            string filenameWithoutExtension = GetFilenameWithoutExtensionAndLocalization(filePath);
            string directoryPath = Path.GetDirectoryName(filePath);
            string filePathFormat =
                Path.Combine(directoryPath, filenameWithoutExtension + ConstSubtitleFilenameFormatSuffix);
            return filePathFormat;
        }

        public static bool ExistsMediaSupportFile(string filePath, MediaSupportFileType supportFileType)
        {
            return ExistsMediaSupportFile(filePath, supportFileType, out _);
        }

        public static bool ExistsMediaSupportFile(string filePath, MediaSupportFileType supportFileType,
            out string supportFilePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                supportFilePath = null;
                return false;
            }
            string directory = Path.GetDirectoryName(filePath);
            string filenameWithoutExtension = GetFilenameWithoutExtensionAndLocalization(filePath);
            string extension = Path.GetExtension(filePath);
            string[] suffixesAndExtensions;
            switch (supportFileType)
            {
                case MediaSupportFileType.VersionEN:
                    suffixesAndExtensions = new string[]
                    {
                        string.Format(ConstLocalizedMediaSuffixAndExtensionFormat, Languages.English, extension)
                    };
                    break;
                case MediaSupportFileType.PosterImage:
                    suffixesAndExtensions = ConstPosterImageSuffixes;
                    break;
                case MediaSupportFileType.PreviewImage:
                    suffixesAndExtensions = ConstPreviewImageSuffixes;
                    break;
                case MediaSupportFileType.SubTitlesDE:
                    suffixesAndExtensions = new string[]
                    {
                        string.Format(ConstSubtitleFilenameFormatSuffix, Languages.German)
                    };
                    break;
                case MediaSupportFileType.SubTitlesEN:
                    suffixesAndExtensions = new string[]
                    {
                        string.Format(ConstSubtitleFilenameFormatSuffix, Languages.English)
                    };
                    break;
                default:
                    supportFilePath = null;
                    return false;
            }
            foreach (string suffixAndExtension in suffixesAndExtensions)
            {
                supportFilePath = Path.Combine(directory, filenameWithoutExtension + suffixAndExtension);
                if (File.Exists(supportFilePath)) return true;
            }
            supportFilePath = null;
            return false;
        }

        public static string GetSupportFilePathFor(string mediaFilePath, MediaSupportFileType supportFileType, 
            string destinationExtension)
        {
            if (string.IsNullOrEmpty(mediaFilePath))
            {
                return null;
            }
            string directory = Path.GetDirectoryName(mediaFilePath);
            string filenameWithoutExtension = GetFilenameWithoutExtensionAndLocalization(mediaFilePath);
            switch (supportFileType)
            {
                case MediaSupportFileType.VersionEN:
                    string mediaExtension = Path.GetExtension(mediaFilePath);
                    return Path.Combine(directory, string.Format(ConstLocalizedMediaFilenameFormat, 
                        filenameWithoutExtension, Languages.English, mediaExtension));
                case MediaSupportFileType.PosterImage:
                    return Path.Combine(directory, string.Format(ConstPosterImageFilenameFormat,
                        filenameWithoutExtension, destinationExtension));
                case MediaSupportFileType.PreviewImage:
                    return Path.Combine(directory, string.Format(ConstPreviewImageFilenameFormat,
                        filenameWithoutExtension, destinationExtension));
                case MediaSupportFileType.SubTitlesDE:
                    return Path.Combine(directory, string.Format(ConstLocalizedSubTitlesFilenameFormat,
                        filenameWithoutExtension, Languages.German));
                case MediaSupportFileType.SubTitlesEN:
                    return Path.Combine(directory, string.Format(ConstLocalizedSubTitlesFilenameFormat,
                        filenameWithoutExtension, Languages.English));
                default:
                    throw new ArgumentOutOfRangeException(nameof(supportFileType));
            }
        }

        public static string GetFilenameWithoutExtensionLocalizationAndVersion(string filePath, out int currentVersion)
        {
            string result = MediaFileHelper.GetFilenameWithoutExtensionAndLocalization(filePath);
            int lastIndex = result.LastIndexOf(ConstMediaFileVersionPrefix);
            if (lastIndex >= 0)
            {
                string versionString = result.Substring(lastIndex + ConstMediaFileVersionPrefix.Length);
                if (int.TryParse(versionString, out currentVersion)) return result.Substring(0, lastIndex);
                else
                {
                    currentVersion = 1;
                    return result;
                }
            }
            else
            {
                currentVersion = 1;
                return result;
            }
        }

        public static string GetVersionedFilename(string fileNameWithoutAll, int currentVersion)
        {
            return $"{fileNameWithoutAll}{ConstMediaFileVersionPrefix}{currentVersion:#00}";
        }

        #endregion Öffentliche Methoden
    }
}
