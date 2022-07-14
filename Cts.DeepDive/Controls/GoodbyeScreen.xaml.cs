using Gemelo.Components.Common.Wpf.Localization;
using Gemelo.Components.Cts.Code.Data.Stations;
using System;
using System.Windows.Controls;

namespace Gemelo.Applications.Cts.DeepDive.Controls
{
    /// <summary>
    /// Interaktionslogik für GoodbyeScreen.xaml
    /// </summary>
    public partial class GoodbyeScreen : UserControl
    {
        public event EventHandler CompletedClick;

        protected void OnCompletedClick()
        {
            CompletedClick?.Invoke(this, null);
        }

        public GoodbyeScreen()
        {
            InitializeComponent();
            if (App.IsAvailable) App.Current.StationDefinitionChanged += App_StationDefinitionChanged;
        }

        private void App_StationDefinitionChanged(object sender, EventArgs e)
        {
            DeepDiveStationDefinition stationDefinition = App.Current.StationDefinition;
            m_TxtHeadline.SetLocalizedText(stationDefinition.GoodbyeHeadline);
            m_TxtText.SetLocalizedText(stationDefinition.GoodbyeText);
            m_TxtLink.SetLocalizedText(stationDefinition.GoodbyeLink);
            m_Footer.NextButtonText = stationDefinition.GoodbyeButtonText;
        }

        private void Footer_NextClick(object sender, EventArgs e)
        {
            OnCompletedClick();
        }
    }
}
