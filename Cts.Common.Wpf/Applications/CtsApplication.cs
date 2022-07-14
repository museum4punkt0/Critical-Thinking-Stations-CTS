using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Settings;
using Gemelo.Components.Common.Text;
using Gemelo.Components.Common.Tracing;
using Gemelo.Components.Common.Wpf.Media;
using Gemelo.Components.Cts.Code.Communication;
using Gemelo.Components.Cts.Code.Data.Layout;
using Gemelo.Components.Cts.Code.Data.Users;
using Gemelo.Components.Cts.Code.Media;
using Gemelo.Components.Cts.Windows;
using Gemelo.Components.Dah.Applications;
using Gemelo.Components.Dah.Code.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Gemelo.Components.Cts.Applications
{
    public abstract class CtsApplication : DahApplication
    {
        #region Konstanten

        private const int ConstUpdateStationDefinitionMaxRetries = 50;
        private static readonly TimeSpan ConstUpdateStationDefinitionRetryInterval = TimeSpan.FromSeconds(8.0);

        #endregion Konstanten

        #region Felder und Eigenschaften

        public static new CtsApplication Current => DahApplication.Current as CtsApplication;

        public abstract string StationID { get; protected set; }

        public new CtsMainWindow MainWindow => base.MainWindow as CtsMainWindow;

        public RfidHintPosition? OverrideRfidHintPosition { get; private set; } = null;

        private string m_ServerAddress;

        public virtual string MediaDirectoryPath => Path.Combine(DataDirectoryPath, ConstMediaSubdirectoryName);

        private readonly Dictionary<string, BitmapSource> m_CachedBitmapSources =
            new Dictionary<string, BitmapSource>();

        public CtsCommunicationClient CommunicationClient { get; protected set; }

        public CtsUserInformation CurrentUser { get; protected set; }

        #endregion Felder und Eigenschaften

        #region Konstruktor und Initialisierung

        public CtsApplication()
        {
            Settings.AdditionalStartArgumentHandlers.Add(new StartArgumentHandler
            {
                MatchingKey = "rfidpos",
                NeedsValue = true,
                Description = "Overrides the RFID reader position (values: Left, Right, BottomLeft, BottomRight)",
                ValuePlaceholder = "position",
                ValueAction = (value) => OverrideRfidHintPosition = Enum.Parse<RfidHintPosition>(value, ignoreCase: true)
            });
            Settings.AdditionalStartArgumentHandlers.Add(new StartArgumentHandler
            {
                MatchingKey = "server",
                NeedsValue = true,
                Description = "Sets the server address",
                ValuePlaceholder = "server",
                ValueAction = (value) => m_ServerAddress = value
            });
        }

        protected override void InitializeBeforeMainWindow()
        {
            base.InitializeBeforeMainWindow();
            InitializeCommunicationClient();
        }

        private void InitializeCommunicationClient()
        {
            CommunicationClient = new CtsCommunicationClient();
            if (m_ServerAddress.IsNotNullOrEmpty())
            {
                CommunicationClient.ServerBaseAddress = new Uri(m_ServerAddress.Contains(":") ?
                    m_ServerAddress :
                    string.Format(CtsCommunicationClient.DefaultServerAddressFormat, m_ServerAddress));
            }
        }

        public override async Task UpdateStationDefinition()
        {
            bool isUpdated = false;
            int failedUpdates = 0;
            do
            {
                try
                {
                    MainWindow.ShowInitializationStatus("Update configuration from server ...");
                    await UpdateStationDefinitionInternally();
                    MainWindow.ShowInitializationStatus("Configuration update completed!");
                    OnStationDefinitionChanged();
                    isUpdated = true;
                }
                catch (Exception exception)
                {
                    TraceX.WriteInformational(
                        message: $"StationDefinition update failed: {exception.Message}",
                        category: nameof(CtsApplication),
                        exception: exception);
                    MainWindow.ShowInitializationStatus($"Error while updating configuration: {exception.Message}\n" +
                        $"Retry in a few seconds ...");
                    failedUpdates++;
                    await Task.Delay(ConstUpdateStationDefinitionRetryInterval);
                }
            }
            while (!isUpdated && failedUpdates <= ConstUpdateStationDefinitionMaxRetries);
            if (!isUpdated) throw new Exception("Failed to update station configuration!");
        }

        #endregion Konstruktor und Initialisierung

        #region Öffentliche Methoden

        public void ResetUser()
        {
            CurrentUser = null;
        }

        public string GetMediaFilePathFor(string filename, string mediaDirectoryPath)
        {
            if (filename == null) return null;
            mediaDirectoryPath ??= MediaDirectoryPath;
            return Path.Combine(mediaDirectoryPath, filename);
        }

        public string TryGetLocalizedMediaFilePathFor(string filename, string language,
            string mediaDirectoryPath)
        {
            string filePath = GetMediaFilePathFor(filename, mediaDirectoryPath);
            return MediaFileHelper.TryGetLocalizedMediaFilePathFor(filePath, language);
        }

        public bool ExistsMediaSupportFile(string filename, MediaSupportFileType supportFileType,
            string mediaDirectoryPath)
        {
            string filePath = GetMediaFilePathFor(filename, mediaDirectoryPath: mediaDirectoryPath);
            return MediaFileHelper.ExistsMediaSupportFile(filePath, supportFileType);
        }

        public BitmapSource GetPreviewBitmapFor(string filename, bool preferPreview = true,
            string mediaDirectoryPath = null)
        {
            string filePath = GetMediaFilePathFor(filename, mediaDirectoryPath);
            MediaSupportFileType firstFileType =
                preferPreview ? MediaSupportFileType.PreviewImage : MediaSupportFileType.PosterImage;
            MediaSupportFileType secondFileType =
                preferPreview ? MediaSupportFileType.PosterImage : MediaSupportFileType.PreviewImage;
            string previewFilePath =
                MediaFileHelper.TryGetSupportFilePathFor(filePath, firstFileType) ??
                MediaFileHelper.TryGetSupportFilePathFor(filePath, secondFileType);
            if (previewFilePath != null)
            {
                return GetCachedImageSource(previewFilePath);
            }
            return null;
        }

        #endregion Öffentliche Methoden

        #region Private Methoden

        private BitmapSource GetCachedImageSource(string filePath)
        {
            if (!m_CachedBitmapSources.ContainsKey(filePath))
            {
                BitmapSource result = BitmapHelper.Load(filePath);
                m_CachedBitmapSources.Add(filePath, result);
                return result;
            }
            else return m_CachedBitmapSources[filePath];
        }

        #endregion Private Methoden

        #region Ereignishandler

        protected override async void ProcessRfidDetectedDispatched(RfidUserEventArgs e)
        {
            base.ProcessRfidDetectedDispatched(e);
            try
            {
                CtsUserInformation user = await CommunicationClient.GetUserForRfid(e.User);
                if (user != null)
                {
                    if (CurrentUser?.CtsUserID != user.CtsUserID)
                    {
                        CurrentUser = user;
                        if (e.User.Language.IsNullOrEmpty() && user.Language.IsNotNullOrEmpty())
                        {
                            string language = user.Language;
                            if (Languages.Available.Contains(language))
                            {
                                Languages.ChangeTo(language, source: LanguageChangeSource.Other);
                            }
                        }
                        MainWindow.ProcessNewUser();
                    }
                }
            }
            catch (Exception exception)
            {
                TraceX.WriteHandledException(
                    message: $"Handled exception while trying to get user for rfid '{e.User?.ID}: {exception.Message}",
                    category: nameof(CtsApplication),
                    exception: exception);
            }
        }

        #endregion Ereignishandler
    }
}
