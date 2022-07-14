using Gemelo.Components.Common.Wpf.Localization;
using Gemelo.Components.Cts.Code.Data.Stations;
using System;
using System.Windows.Controls;

namespace Gemelo.Applications.Cts.DeepDive.Controls
{
    /// <summary>
    /// Zeigt den Einleitungsbildschirm für die Demographie-Fragen an.
    /// </summary>
    public partial class DemographyIntroScreen : UserControl
    {
        public event EventHandler SkipClick;

        protected void OnSkipClick()
        {
            SkipClick?.Invoke(this, null);
        }

        public event EventHandler ConfirmClick;

        protected void OnConfirmClick()
        {
            ConfirmClick?.Invoke(this, null);
        }

        public DemographyIntroScreen()
        {
            InitializeComponent();
            if (App.IsAvailable) App.Current.StationDefinitionChanged += App_StationDefinitionChanged;
        }

        private void App_StationDefinitionChanged(object sender, EventArgs e)
        {
            DeepDiveStationDefinition stationDefinition = App.Current.StationDefinition;
            m_TxtHeadline.SetLocalizedText(stationDefinition.DemographyIntroHeadline);
            m_TxtText.SetLocalizedText(stationDefinition.DemographyIntroText);
            m_Footer.HintText = stationDefinition.DemographyIntroPrivacyHint;
            m_Footer.SkipButtonText = stationDefinition.DemographyIntroSkipButtonText;
            m_Footer.NextButtonText = stationDefinition.DemographyIntroConfirmButtonText;
        }

        private void Footer_SkipClick(object sender, EventArgs e)
        {
            OnSkipClick();
        }

        private void Footer_NextClick(object sender, EventArgs e)
        {
            OnConfirmClick();
        }
    }
}
