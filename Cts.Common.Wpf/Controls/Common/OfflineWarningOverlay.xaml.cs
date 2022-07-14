using Gemelo.Components.Cts.Applications;
using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Gemelo.Components.Cts.Controls.Common
{
    /// <summary>
    /// Zeigt am oberen Rand eine Warnung an, wenn keine Verbinung zum Server besteht.
    /// </summary>
    public partial class OfflineWarningOverlay : UserControl
    {
        #region Konstanten

        private static readonly TimeSpan DefaultCheckInterval = TimeSpan.FromSeconds(5.0);

        private const int DefaultFailedThreshold = 2;

        #endregion Konstanten

        private Storyboard m_StorySlideIn;
        private Storyboard m_StorySlideOut;

        private DispatcherTimer m_CheckTimer;

        private int m_FailedCount = 0;

        public int FailedThreshold { get; set; } = DefaultFailedThreshold;

        public OfflineWarningOverlay()
        {
            InitializeComponent();
            InitializeStoryboards();
            if (CtsApplication.IsAvailable) InitializeCheckTimer();
        }

        private void InitializeStoryboards()
        {
            m_StorySlideIn = (Storyboard)FindResource("StorySlideIn");
            m_StorySlideOut = (Storyboard)FindResource("StorySlideOut");
        }

        private void InitializeCheckTimer()
        {
            m_CheckTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = DefaultCheckInterval
            };
            m_CheckTimer.Tick += CheckTimer_Tick;
        }

        public void Start()
        {
            if (m_CheckTimer?.IsEnabled == false) m_CheckTimer?.Start();
        }

        public void Stop()
        {
            m_CheckTimer?.Stop();
            m_StorySlideIn.Stop(this);
            m_StorySlideOut.Stop(this);
        }

        private async void CheckTimer_Tick(object sender, EventArgs e)
        {
            bool checkResult = await CtsApplication.Current.CommunicationClient.CheckConnection();
            if (checkResult)
            {
                m_FailedCount = 0;
                m_StorySlideOut.Begin(this, true);
            }
            else
            {
                m_FailedCount++;
                if (m_FailedCount == FailedThreshold) m_StorySlideIn.Begin(this, true);
            }
        }
    }
}
