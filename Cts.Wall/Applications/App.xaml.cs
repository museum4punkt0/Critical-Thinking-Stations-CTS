using Gemelo.Applications.Cts.Wall.Code.Settings;
using Gemelo.Components.Cms.Data.Base;
using Gemelo.Components.Common.Settings;
using Gemelo.Components.Common.Tracing;
using Gemelo.Components.Cts.Applications;
using Gemelo.Components.Cts.Code.Communication;
using Gemelo.Components.Cts.Code.Data.Stations;
using Gemelo.Components.Cts.Code.Data.Survey;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemelo.Applications.Cts.Wall
{
    /// <summary>
    /// Applikation CTS Wall
    /// </summary>
    public partial class App : CtsApplication
    {
        #region Konstanten

        private const string ConstStationName = "Cts.Wall";

        private const string ConstDisplaySettingsFilename = @"Settings\DisplaySettings.json";

        #endregion Konstanten

        #region Felder und Eigenschaften

        public static new App Current => CtsApplication.Current as App;

        public new MainWindow MainWindow => base.MainWindow as MainWindow;

        public override string CmsStationName => ConstStationName;

        public bool UseRatio16to9 { get; private set; } = false;

        public override bool UseRfid => false;

        public override string StationID { get; protected set; } = WallStationDefinition.StationID;

        protected override CmsStationDefinition CmsStationDefinition => null;

        public WallStationDefinition StationDefinition { get; set; }

        public WallDisplaySettings DisplaySettings { get; set; } = new();

        private bool m_UseDemoResults = false;
        private object m_LatestResultSetLock = new object();
        private SurveyResultSet m_LatestResultSet = new SurveyResultSet();

        #endregion Felder und Eigenschaften

        #region Konstruktor und Initialisierung

        public App()
        {
            Settings.AdditionalStartArgumentHandlers.Add(new StartArgumentHandler
            {
                MatchingKey = "16to9",
                NeedsValue = false,
                Description = "Sets screen ratio to 16:9",
                ExistsAction = () => UseRatio16to9 = true
            });
            Settings.AdditionalStartArgumentHandlers.Add(new StartArgumentHandler
            {
                MatchingKey = "demoresults",
                NeedsValue = false,
                Description = "Use random results for demo purposes",
                ExistsAction = () => m_UseDemoResults = true
            });
        }

        protected override async Task UpdateStationDefinitionInternally()
        {
            StationDefinition = await CommunicationClient.GetWallStationDefinition();
            await UpdateSurveyResultSet();
        }

        protected override void InitializeBeforeMainWindow()
        {
            base.InitializeBeforeMainWindow();
            LoadDisplaySettings();
        }

        private void LoadDisplaySettings()
        {
            try
            {
                string settingsFilePath = Directories.GetPathInDataDirectory(ConstDisplaySettingsFilename);
                if (File.Exists(settingsFilePath))
                {
                    string json = File.ReadAllText(settingsFilePath, Encoding.UTF8);
                    DisplaySettings = JsonConvert.DeserializeObject<WallDisplaySettings>(json);
                }
            }
            catch (Exception exception)
            {
                TraceX.WriteWarning(
                    message: $"Handled exception while loading display settings: {exception.Message}",
                    exception: exception);
            }
        }

        #endregion Konstruktor und Initialisierung

        #region Öffentliche Methoden

        public void StartUpdateSurveyResultSet()
        {
            _ = UpdateSurveyResultSet();
        }

        public async Task UpdateSurveyResultSet()
        {
            var result = await CommunicationClient.GetSurveyResultSet(new GetSurveyResultSetRequest
            {
                QuestionIDs = StationDefinition.SurveyQuestions.Select(question => question.QuestionID).ToList()
            });
            if (result?.IsSuccessful == true)
            {
                lock (m_LatestResultSetLock) m_LatestResultSet = result.ResultSet;
            }
        }

        public SurveyResultSet GetCurrentSurveyResultSet()
        {
            if (m_UseDemoResults) return SurveyResultSet.CreateDemoResultSetFor(StationDefinition.SurveyQuestions);
            else
            {
                lock (m_LatestResultSetLock) return m_LatestResultSet;
            }
        }

        public void SaveDisplaySettings()
        {
            try
            {
                string settingsFilePath = Directories.GetPathInDataDirectory(ConstDisplaySettingsFilename);
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));
                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(DisplaySettings), Encoding.UTF8);
            }
            catch (Exception exception)
            {
                TraceX.WriteWarning(
                    message: $"Handled exception while saving display settings: {exception.Message}",
                    exception: exception);
            }
        }

        public ChartSettings GetChartSettings(string questionID)
        {
            if (!DisplaySettings.ChartSettings.ContainsKey(questionID))
            {
                DisplaySettings.ChartSettings.Add(questionID, new ChartSettings());
            }
            return DisplaySettings.ChartSettings[questionID];
        }

        #endregion Öffentliche Methoden

        #region Private Methoden

        #endregion Private Methoden
    }
}
