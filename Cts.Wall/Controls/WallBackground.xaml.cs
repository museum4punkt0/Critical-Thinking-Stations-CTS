using Gemelo.Components.Common.Settings;
using Gemelo.Components.Common.Wpf.Text;
using Gemelo.Components.Cts.Code.Data.Stations;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Gemelo.Applications.Cts.Wall.Controls
{
    /// <summary>
    /// Zeigt den Hintergrund für die Wall an
    /// </summary>
    public partial class WallBackground : UserControl
    {
        #region Konstanten

        private const string ConstMediaFilename = @"Data\WallAnimation.mp4";

        private static readonly TimeSpan ConstLoopAheadOfTimeInterval = TimeSpan.FromMilliseconds(20.0);

        #endregion Konstanten

        public WallBackground()
        {
            InitializeComponent();
            InitializeMedia();
            if (App.IsAvailable) BindApplicationEvents();
            IsVisibleChanged += WallBackground_IsVisibleChanged;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void BindApplicationEvents()
        {
            App.Current.StationDefinitionChanged += App_StationDefinitionChanged;
        }

        private void InitializeMedia()
        {
            if (App.IsAvailable)
            {
                string filePath = Directories.GetPathInApplicationDirectory(ConstMediaFilename);
                m_MediaElement.Source = new Uri(filePath);
            }
        }

        private void Restart()
        {
            m_MediaElement.Stop();
            m_MediaElement.Position = TimeSpan.Zero;
            Dispatcher.BeginInvoke((Action)(() => m_MediaElement.Play()));
        }

        private void App_StationDefinitionChanged(object sender, EventArgs e)
        {
            WallStationDefinition stationDefinition = App.Current.StationDefinition;
            m_TxtLink.SetFormattedText(stationDefinition.BackgroundLink);
        }

        private void WallBackground_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) m_MediaElement.Play();
            else m_MediaElement.Stop();
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Restart();
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (IsVisible && m_MediaElement.NaturalDuration.HasTimeSpan && 
                m_MediaElement.Position + ConstLoopAheadOfTimeInterval > m_MediaElement.NaturalDuration.TimeSpan)
            {
                Restart();
            }
        }
    }
}
