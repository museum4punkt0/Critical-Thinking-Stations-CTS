using Gemelo.Components.Cms.Data.Base;
using Gemelo.Components.Common.Settings;
using Gemelo.Components.Common.Text;
using Gemelo.Components.Common.Tracing;
using Gemelo.Components.Cts.Applications;
using Gemelo.Components.Cts.Code.Communication;
using Gemelo.Components.Cts.Code.Data.Stations;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.LidarTouch.Code;
using Gemelo.Components.LidarTouch.Code.Calibration;
using Gemelo.Components.LidarTouch.Code.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Gemelo.Applications.Cts.TouchBeamer
{
    /// <summary>
    /// Applikation CTS TouchBeamer
    /// </summary>
    public partial class App : CtsApplication
    {
        #region Konstanten

        private const string ConstStationName = "Cts.TouchBeamer";

        private static readonly string[] ConstDemoStationIDs =
            { "Galerie", "Ankunft", "GrandCentral", "Debattenraum", "Biosaal" };
        private const string ConstLidarCalibrationFilename = @"Settings\LidarCalibration.json";
        private const string ConstLidarSettingsFilename = @"Settings\LidarSettings.json";

        #endregion Konstanten

        #region Felder und Eigenschaften

        public static new App Current => CtsApplication.Current as App;

        public new MainWindow MainWindow => base.MainWindow as MainWindow;

        public override string CmsStationName => ConstStationName;

        public override bool UseRfid => true;

        public override string StationID { get; protected set; }

        protected override CmsStationDefinition CmsStationDefinition => null;

        public TouchBeamerStationDefinition StationDefinition { get; set; }

        private string m_LidarSerialPortName;
        public LidarSensor LidarSensor { get; private set; }

        #endregion Felder und Eigenschaften

        #region Konstruktor und Initialisierung

        public App()
        {
            Settings.AdditionalStartArgumentHandlers.Add(new StartArgumentHandler
            {
                Description = "Sets the stations ID",
                ValuePlaceholder = "station-id",
                NeedsValue = true,
                ValueAction = value => StationID = value
            });
            Settings.AdditionalStartArgumentHandlers.Add(new StartArgumentHandler
            {
                MatchingKey = "lidar",
                Description = "Sets the serial port for the Lidar sensor",
                ValuePlaceholder = "serialport",
                NeedsValue = true,
                ValueAction = value => m_LidarSerialPortName = value
            });
        }

        protected override async Task InitializeAfterMainWindowAsync()
        {
            if (string.IsNullOrEmpty(StationID))
            {
                throw new InvalidOperationException("Station ID must be specified via start argument!");
            }
            await base.InitializeAfterMainWindowAsync();
            MainWindow.PreviewKeyDown += (s, e) =>
            {
                if (MainWindow.m_LidarTouchCalibrationControl.Visibility != Visibility.Visible &&
                    e.Key >= Key.D1 && e.Key <= Key.D9 && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    DemoLoadStationDefinition(e.Key - Key.D1);
                }
            };
            await InitializeLidar();
        }

        protected override async Task UpdateStationDefinitionInternally()
        {
            StationDefinition = await CommunicationClient.GetTouchBeamerStationDefinition(StationID);
        }

        private async Task InitializeLidar()
        {
            if (m_LidarSerialPortName.IsNotNullOrEmpty())
            {
                LidarSensor = new LidarSensor
                {
                    SerialPortName = m_LidarSerialPortName
                };
                TryToLoadLidarSettingsAndCalibration();
                LidarTouchDevice.AddSensor(LidarSensor);
                if (await LidarSensor.Start())
                {
                    TraceX.WriteInformational($"{nameof(InitializeLidar)}() completed.");
                }
                else
                {
                    TraceX.WriteWarning($"Lidar initialization failed!");
                    MainWindow.ShowNoLidarMessage();
                }
            }
        }

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

        public void SaveLidarSettingsAndCalibration()
        {
            LidarSensor.Settings.Save(GetLidarSettingsFile().FullName);
            LidarTouchDevice.GetCalibrationFor(LidarSensor).Save(GetLidarCalibrationFile().FullName);
        }

        #endregion Öffentliche Methoden

        #region Private Methoden

        private void TryToLoadLidarSettingsAndCalibration()
        {
            try
            {
                FileInfo settingsFile = GetLidarSettingsFile();
                if (settingsFile.Exists)
                {
                    TraceX.WriteInformational(
                        $"{nameof(TryToLoadLidarSettingsAndCalibration)}, settingsFile: {settingsFile.FullName}");
                    LidarSensor.Settings = LidarSensorSettings.Load(settingsFile.FullName);
                }
                FileInfo calibrationFile = GetLidarCalibrationFile();
                if (calibrationFile.Exists)
                {
                    TraceX.WriteInformational(
                        $"{nameof(TryToLoadLidarSettingsAndCalibration)}, calibrationFile: {settingsFile.FullName}");
                    LidarTouchDevice.SetCalibrationFor(LidarSensor, 
                        LidarTouchCalibration.Load(calibrationFile.FullName));
                }
                TraceX.WriteInformational(
                    $"{nameof(TryToLoadLidarSettingsAndCalibration)} completed!");
            }
            catch { }
        }

        private FileInfo GetLidarSettingsFile()
        {
            FileInfo result = Directories.GetFileInDataDirectory(ConstLidarSettingsFilename);
            result.Directory.Create();
            return result;
        }

        private static FileInfo GetLidarCalibrationFile()
        {
            FileInfo result = Directories.GetFileInDataDirectory(ConstLidarCalibrationFilename);
            result.Directory.Create();
            return result;
        }

        private async void DemoLoadStationDefinition(int demoIndex)
        {
            if (demoIndex >= 0 && demoIndex < ConstDemoStationIDs.Length)
            {
                StationID = ConstDemoStationIDs[demoIndex];
                await UpdateStationDefinition();
                MainWindow.Restart();
            }
        }

        #endregion Private Methoden
    }
}
