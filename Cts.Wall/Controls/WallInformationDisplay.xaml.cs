using Gemelo.Applications.Cts.Wall.Controls.Helper;
using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Wpf.Text;
using Gemelo.Components.Cts.Code.Data.Stations;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Gemelo.Applications.Cts.Wall.Controls
{
    /// <summary>
    /// Hauptinformation zu Wall
    /// </summary>
    public partial class WallInformationDisplay : UserControl
    {
        private readonly List<PageProgress> m_PageProgesses = new List<PageProgress>();

        public string SecondLanguage { get; set; } = Languages.English;

        public int CurrentPageIndex { get; private set; } = 0;

        public event EventHandler PageChange;

        protected void OnPageChange()
        {
            PageChange?.Invoke(this, null);
        }

        public WallInformationDisplay()
        {
            InitializeComponent();
            if (App.IsAvailable) BindApplicationEvents();
        }

        private void BindApplicationEvents()
        {
            App.Current.StationDefinitionChanged += App_StationDefinitionChanged;
        }

        public void StartPage(int pageIndex)
        {
            pageIndex = pageIndex % m_PageProgesses.Count;
            CurrentPageIndex = pageIndex;
            for (int i = 0; i < m_PageProgesses.Count; i++)
            {
                if (i != pageIndex)
                {
                    m_PageProgesses[i].Stop();
                    m_PageProgesses[i].Progress = i < pageIndex ? 1.0 : 0.0;
                }
                else m_PageProgesses[pageIndex].Start();
            }
            OnPageChange();
        }

        public void NextPage()
        {
            StartPage(CurrentPageIndex + 1);
        }

        private void UpdatePageCount(int newPageCount)
        {
            m_PageProgesses.ForEach(pageProgress =>
            {
                pageProgress.Stop();
                pageProgress.Expired -= PageProgress_Expired;
            });
            m_GridPageProgresses.Children.Clear();
            m_PageProgesses.Clear();
            m_GridPageProgresses.Columns = newPageCount;
            for (int pageIndex = 0; pageIndex < newPageCount; pageIndex++)
            {
                AddPageProgress(pageIndex, newPageCount);
            }
        }

        private void AddPageProgress(int pageIndex, int pageCount)
        {
            PageProgress pageProgress = new PageProgress
            {
                PageIndex = pageIndex,
                PageCount = pageCount
            };
            pageProgress.Expired += PageProgress_Expired;
            m_PageProgesses.Add(pageProgress);
            m_GridPageProgresses.Children.Add(pageProgress);
        }

        private void App_StationDefinitionChanged(object sender, EventArgs e)
        {
            WallStationDefinition stationDefinition = App.Current.StationDefinition;
            m_LocalizedTextTitle.Text = stationDefinition.Title.ToUpperInvariant();
            m_TxtDescriptionDefaultLanguage.SetFormattedText(stationDefinition.Description.GetFor(Languages.Default));
            m_TxtDescriptionSecondLanguage.SetFormattedText(stationDefinition.Description.GetFor(SecondLanguage));
            int newPageCount = stationDefinition.GetPageCount();
            if (m_PageProgesses.Count != newPageCount) UpdatePageCount(newPageCount);
            m_PageProgesses.ForEach(pageProgress =>
            {
                pageProgress.Interval = stationDefinition.PageInterval;
                pageProgress.ProgressTextFormat = stationDefinition.PageProgressTextFormat;
            });
        }

        private void PageProgress_Expired(object sender, EventArgs e)
        {
            StartPage(CurrentPageIndex + 1);
        }
    }
}
