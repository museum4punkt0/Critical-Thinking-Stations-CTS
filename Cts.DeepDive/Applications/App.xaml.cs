using Gemelo.Applications.Cts.DeepDive.Controls.Common;
using Gemelo.Components.Cms.Data.Base;
using Gemelo.Components.Common.Settings;
using Gemelo.Components.Common.Tracing;
using Gemelo.Components.Common.Wpf.Text.Formatting;
using Gemelo.Components.Cts.Applications;
using Gemelo.Components.Cts.Code.Communication;
using Gemelo.Components.Cts.Code.Data.DeepDive;
using Gemelo.Components.Cts.Code.Data.Stations;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Code.Data.Users;
using Gemelo.Components.Cts.Code.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Gemelo.Applications.Cts.DeepDive
{
    /// <summary>
    /// Applikation CTS DeepDive
    /// </summary>
    public partial class App : CtsApplication
    {
        #region Konstanten

        private const string ConstStationName = "Cts.DeepDive";

        private const string ConstTextFormattingDeepDiveControlID = "deepdive";
        private const string ConstTextFormattingDeepDiveLargeControlID = "deepdive-large";

        #endregion Konstanten

        #region Felder und Eigenschaften

        public static new App Current => CtsApplication.Current as App;

        public new MainWindow MainWindow => base.MainWindow as MainWindow;

        public override string CmsStationName => ConstStationName;

        public bool UseRatio16to10 { get; private set; } = false;

        public bool IsRfidHandTopRightVisible { get; private set; } = false;

        public override bool UseRfid => true;

        public override string StationID { get; protected set; } = DeepDiveStationDefinition.StationID;

        protected override CmsStationDefinition CmsStationDefinition => null;

        public DeepDiveStationDefinition StationDefinition { get; set; }

        #endregion Felder und Eigenschaften

        #region Konstruktor und Initialisierung

        public App()
        {
            Settings.AdditionalStartArgumentHandlers.Add(new StartArgumentHandler
            {
                MatchingKey = "16to10",
                NeedsValue = false,
                Description = "Sets screen ratio to 16:10",
                ExistsAction = () => UseRatio16to10 = true
            });
            Settings.AdditionalStartArgumentHandlers.Add(new StartArgumentHandler
            {
                MatchingKey = "showrfid",
                NeedsValue = false,
                Description = "Shows RFID icon at the top right corner",
                ExistsAction = () => IsRfidHandTopRightVisible = true
            });
        }

        protected override void InitializeBeforeMainWindow()
        {
            base.InitializeBeforeMainWindow();
            InitializeTextFormatting();
        }

        private void InitializeTextFormatting()
        {
            TextFormattingEngine.DefaultCategories = FormattingCategories.Default | FormattingCategories.Control;
            ControlHandler.Default.ControlChecking += TextFormattingControlHandler_ControlChecking;
            ControlHandler.Default.ControlRequest += TextFormattingControlHandler_ControlRequest;
        }

        #region StationDefinition-Update

        protected override async Task UpdateStationDefinitionInternally()
        {
            StationDefinition = await CommunicationClient.GetDeepDiveStationDefinition();
            await UpdateMediaFiles();
        }

        private async Task UpdateMediaFiles()
        {
            foreach (var questionID in StationDefinition.DeepDiveContents.Keys)
            {
                await UpdateMediaFilesForDeepDiveContent(questionID);
            }
        }

        private async Task UpdateMediaFilesForDeepDiveContent(string questionID)
        {
            var deepDiveContent = StationDefinition.DeepDiveContents[questionID];
            foreach (var contentElement in deepDiveContent.ContentElements.Where(element => element.ElementType == DeepDiveContentElementType.Media))
            {
                await UpdateMediaFilesForContentElement(
                    questionID: questionID,
                    filename: contentElement.Filename);
            }
        }

        private async Task UpdateMediaFilesForContentElement(string questionID, string filename)
        {
            try
            {
                var result = await CommunicationClient.GetMediaFilesInformation(new GetMediaFilesInformationRequest
                {
                    QuestionID = questionID,
                    Filename = filename
                });
                if (result.IsSuccessful)
                {
                    await ProcessMediaFilesInformation(questionID, result.MediaFilesInformation);
                }
                else
                {
                    throw new Exception($"Error while trying to update media files for questionID: " +
                        $"{questionID} and filename: {filename}");
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Error while trying to update media files for questionID: " +
                    $"{questionID} and filename: {filename}", exception);
            }
        }

        private async Task ProcessMediaFilesInformation(string questionID, MediaFilesInformation mediaFilesInformation)
        {
            await EnsureMediaFile(questionID, mediaFilesInformation.MainFile);
            foreach (MediaSupportFileType supportFileType in Enum.GetValues<MediaSupportFileType>())
            {
                if (mediaFilesInformation.SupportFiles.ContainsKey(supportFileType))
                {
                    await EnsureMediaFile(questionID, mediaFilesInformation.SupportFiles[supportFileType]);
                }
                else TryToDeleteSupportFile(mediaFilesInformation.MainFile.Filename, supportFileType);
            }
        }

        private async Task EnsureMediaFile(string questionID, MediaFileDetails fileDetails)
        {
            string localPath = Path.Combine(MediaDirectoryPath, fileDetails.Filename);
            if (!File.Exists(localPath) || !fileDetails.IsUpToDate(localPath))
            {
                TraceX.WriteInformational($"Update media file: {localPath} ...");
                await CommunicationClient.DownloadMediaFile(
                    request: new DownloadMediaFileRequest
                    {
                        Filename = fileDetails.Filename,
                        QuestionID = questionID
                    },
                    destinationFilePath: localPath);
                File.SetLastWriteTimeUtc(localPath, fileDetails.LastWriteTime);
            }
        }

        private void TryToDeleteSupportFile(string filename, MediaSupportFileType supportFileType)
        {
            string filePath = Path.Combine(MediaDirectoryPath, filename);
            if (MediaFileHelper.ExistsMediaSupportFile(filePath, supportFileType, out string supportFilePath))
            {
                TraceX.WriteInformational($"Try to delete obsolete media file: {supportFilePath} ...");
                File.Delete(supportFilePath);
            }
        }

        #endregion StationDefinition-Update

        #endregion Konstruktor und Initialisierung

        #region Öffentliche Methoden

        public async Task<bool> SaveAnswers(string questionID, IEnumerable<SurveyAnswer> answers)
        {
            if (CurrentUser == null) return false;
            try
            {
                var result = await CommunicationClient.SaveAnswers(new SaveAnswersRequest
                {
                    CtsUserID = CurrentUser.CtsUserID,
                    StationID = App.Current.StationID,
                    QuestionID = questionID,
                    AnswerIDs = answers.Select(answer => answer.AnswerID).ToList()
                });
                if (!result.IsSuccessful)
                {
                    TraceX.WriteWarning(
                        message: $"Saving answers for question {questionID} and " +
                            $"user {CurrentUser?.CtsUserID} failed: {result.Message}");
                }
                else
                {
                    if (!CurrentUser.AnswersForQuestionIDs.ContainsKey(questionID))
                    {
                        CurrentUser.AnswersForQuestionIDs[questionID] = new List<CtsUserAnswerInformation>();
                    }
                    CurrentUser.AnswersForQuestionIDs[questionID].Add(new CtsUserAnswerInformation
                    {
                        QuestionID = questionID,
                        AnswerIDs = answers.Select(answer => answer.AnswerID).ToList(),
                        StationID = StationID,
                        Timestamp = DateTime.Now
                    });
                }
                return result.IsSuccessful;
            }
            catch (Exception exception)
            {
                TraceX.WriteHandledException(
                    message: $"Handled exception while saving answers for question {questionID} and " +
                        $"user {CurrentUser?.CtsUserID}: {exception.Message}",
                    exception: exception);
                return false;
            }
        }

        public async Task<bool> SaveDeepDiveVisit(string questionID)
        {
            if (CurrentUser == null) return false;
            try
            {
                var result = await CommunicationClient.SaveDeepDiveVisit(new SaveDeepDiveVisitRequest
                {
                    CtsUserID = CurrentUser.CtsUserID,
                    QuestionID = questionID
                });
                if (!result.IsSuccessful)
                {
                    TraceX.WriteWarning(
                        message: $"Saving DeepDive visit for question {questionID} and " +
                            $"user {CurrentUser?.CtsUserID} failed: {result.Message}");
                }
                return result.IsSuccessful;
            }
            catch (Exception exception)
            {
                TraceX.WriteHandledException(
                    message: $"Handled exception while saving DeepDive visit for question {questionID} and " +
                        $"user {CurrentUser?.CtsUserID}: {exception.Message}",
                    exception: exception);
                return false;
            }
        }

        public async Task<bool> AddUserState(CtsUserState state)
        {
            if (CurrentUser == null) return false;
            try
            {
                var result = await CommunicationClient.ChangeUserState(new ChangeUserStateRequest
                {
                    CtsUserID = CurrentUser.CtsUserID,
                    AddStates = state
                });
                if (result.IsSuccessful)
                {
                    CurrentUser.State |= state;
                }
                else
                {
                    TraceX.WriteWarning(
                        message: $"Add user state {state} to user {CurrentUser?.CtsUserID} failed: {result.Message}");
                }
                return result.IsSuccessful;
            }
            catch (Exception exception)
            {
                TraceX.WriteHandledException(
                    message: $"Handled exception while adding user state {state} to " +
                        $"user {CurrentUser?.CtsUserID}: {exception.Message}",
                    exception: exception);
                return false;
            }
        }

        #endregion Öffentliche Methoden

        #region Ereignishandler

        private void TextFormattingControlHandler_ControlChecking(object sender, ControlCheckingEventArgs e)
        {
            e.Cancel = e.ControlName != ConstTextFormattingDeepDiveControlID &&
                e.ControlName != ConstTextFormattingDeepDiveLargeControlID;
        }

        private void TextFormattingControlHandler_ControlRequest(object sender, ControlRequestEventArgs e)
        {
            e.Control = e.ControlName switch
            {
                ConstTextFormattingDeepDiveControlID => new DeepDiveInlineDisplay { Width = 69.0 },
                ConstTextFormattingDeepDiveLargeControlID => new DeepDiveInlineDisplay { Width = 120.0 },
                _ => null
            };
        }

        #endregion Ereignishandler
    }
}
