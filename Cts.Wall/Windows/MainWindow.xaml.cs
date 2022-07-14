using Gemelo.Applications.Cts.Wall.Controls;
using Gemelo.Components.Common.Exhibits.Settings;
using Gemelo.Components.Common.Tracing;
using Gemelo.Components.Common.Wpf.Threading;
using Gemelo.Components.Common.Wpf.UI;
using Gemelo.Components.Cts.Code.Data.Stations;
using Gemelo.Components.Cts.Windows;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Gemelo.Applications.Cts.Wall
{
    /// <summary>
    /// Hauptfenster CTS Wall
    /// </summary>
    public partial class MainWindow : CtsMainWindow
    {
        #region Konstanten

        private static readonly TimeSpan ConstStartUpdateAheadOfPageChangeInterval = TimeSpan.FromSeconds(10.0);

        #endregion Konstanten

        #region Felder und Eigenschaften

        private List<ChartsPage> m_PageControls = new List<ChartsPage>();

        private ChartsPage m_CurrentPageControl;

        private SurveyQuestionChart m_SettingsActivatedChart = null;

        #endregion Felder und Eigenschaften

        #region Konstruktor und Initialisierung

        public MainWindow()
        {
            InitializeComponent();
            CheckForRatio16to9();
            if (App.IsAvailable) BindApplicationEvents();
        }

        private void CheckForRatio16to9()
        {
            if (App.IsAvailable && App.Current.UseRatio16to9)
            {
                m_GridMain.Height = m_GridMain.Width / 16.0 * 9.0;
            }
        }

        private void BindApplicationEvents()
        {
            App.Current.StationDefinitionChanged += App_StationDefinitionChanged;
        }

        #endregion Konstruktor und Initialisierung

        #region Öffentliche Methoden

        public override void TryRestartRestartTimer()
        {
            RestartTimer.Stop();
        }

        #endregion Öffentliche Methoden

        #region Private Methoden

        public override void ShowStartup()
        {
            base.ShowStartup();
        }

        private void ShowPage(int pageIndex)
        {
            TraceX.WriteInformational($"ShowPage({pageIndex}) ...");
            m_CurrentPageControl = m_PageControls[pageIndex];
            m_CurrentPageControl.ShowUpQuestions(App.Current.StationDefinition.GetQuestionGroupsForPage(pageIndex),
                App.Current.GetCurrentSurveyResultSet());
            ResetActivatedChartForSettings();
            DelayedStartResultUpdate();
            ShowMainControl(m_CurrentPageControl);
        }

        private static void DelayedStartResultUpdate()
        {
            DelayedCall.Start(TimeSpan.FromSeconds(
                Math.Max(App.Current.StationDefinition.PageInterval.TotalSeconds -
                ConstStartUpdateAheadOfPageChangeInterval.TotalSeconds, 1.0)),
                App.Current.StartUpdateSurveyResultSet);
        }

        private void UpdatePageControls(int newPageCount)
        {
            m_CanvasMainControls.Children.Clear();
            m_PageControls.Clear();
            for (int pageIndex = 0; pageIndex < newPageCount; pageIndex++)
            {
                AddNewPageControl();
            }
            m_InformationDisplay.StartPage(0);
        }

        private void AddNewPageControl()
        {
            ChartsPage pageControl = new ChartsPage
            {
                Visibility = Visibility.Collapsed
            };
            m_PageControls.Add(pageControl);
            m_CanvasMainControls.Children.Add(pageControl);
        }

        private void ResetActivatedChartForSettings()
        {
            m_PageControls.ForEach(pageControl => pageControl.ResetAllActivatedForSettingsFlags());
            m_SettingsActivatedChart = null;
        }

        private void ActivateChartForSettings(int index)
        {
            m_PageControls.ForEach(pageControl => pageControl.ResetAllActivatedForSettingsFlags());
            m_SettingsActivatedChart = m_CurrentPageControl.GetChart(index);
            if (m_SettingsActivatedChart != null) m_SettingsActivatedChart.IsActiveForSettings = true;
        }

        private void DeltaTopMarginForActiveChart(double deltaMargin)
        {
            if (m_SettingsActivatedChart != null)
            {
                Thickness margin = m_SettingsActivatedChart.Margin;
                double topMargin = margin.Top + deltaMargin;
                m_SettingsActivatedChart.Margin =
                    new Thickness(margin.Left, topMargin, margin.Right, margin.Bottom);
                App.Current.GetChartSettings(m_SettingsActivatedChart.FirstQuestion.QuestionID).TopMartin =
                    topMargin;
                App.Current.SaveDisplaySettings();
            }
        }

        protected override async void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.F4) m_VideoWallSimulationGrid.ToggleVisibility();
            else if (e.Key == Key.F5)
            {
                await App.Current.UpdateSurveyResultSet();
                m_InformationDisplay.NextPage();
            }
            else if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                int index = e.Key == Key.D0 ? 9 : e.Key - Key.D1;
                ActivateChartForSettings(index);
            }
            else if (e.Key >= Key.A && e.Key <= Key.K)
            {
                int index = e.Key - Key.A + 10;
                ActivateChartForSettings(index);
            }
            else if (e.Key == Key.OemPlus)
            {
                DeltaTopMarginForActiveChart(+1.0);
            }
            else if (e.Key == Key.OemMinus)
            {
                DeltaTopMarginForActiveChart(-1.0);
            }
        }

        #endregion Private Methoden

        #region Ereignishandler

        private void App_StationDefinitionChanged(object sender, EventArgs e)
        {
            WallStationDefinition stationDefinition = App.Current.StationDefinition;
            int newPageCount = stationDefinition.GetPageCount();
            if (newPageCount != m_CanvasMainControls.Children.Count) UpdatePageControls(newPageCount);
        }

        private void InformationDisplay_PageChange(object sender, EventArgs e)
        {
            ShowPage(m_InformationDisplay.CurrentPageIndex);
        }

        #endregion Ereignishandler
    }
}
