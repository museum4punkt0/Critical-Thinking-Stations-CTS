using Gemelo.Components.Common.Base;
using Gemelo.Components.Common.Exhibits.Applications;
using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Wpf.Localization;
using Gemelo.Components.Common.Wpf.Text;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Gemelo.Applications.Cts.Wall.Controls.Helper
{
    /// <summary>
    /// Zeigt den Fortschritt für einen Auswertungsseite an
    /// </summary>
    public partial class PageProgress : UserControl
    {
        #region Felder und Eigenschaften

        private int m_PageIndex = 0;

        public int PageIndex
        {
            get => m_PageIndex;
            set
            {
                m_PageIndex = value;
                Update();
            }
        }

        private int m_PageCount = 2;

        public int PageCount
        {
            get => m_PageCount;
            set
            {
                m_PageCount = value;
                Update();
            }
        }

        private double m_Progress = 0.0;

        public double Progress
        {
            get => m_Progress;
            set
            {
                m_Progress = value;
                Update();
            }
        }

        private TimeSpan m_Interval;

        public TimeSpan Interval
        {
            get => m_Interval;
            set
            {
                m_Interval = value;
                Update();
            }
        }

        private LocalizationString m_ProgressTextFormat;

        public LocalizationString ProgressTextFormat
        {
            get => m_ProgressTextFormat;
            set
            {
                m_ProgressTextFormat = value;
                Update();
            }
        }

        private double m_StartTime = double.MaxValue;

        #endregion Felder und Eigenschaften

        #region Ereignisse

        public event EventHandler Expired;

        protected void OnExpired()
        {
            Expired?.Invoke(this, null);
        }

        #endregion Ereignisse

        public PageProgress()
        {
            InitializeComponent();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void Update()
        {
            if (m_BorderProgressOuter.ActualWidth > 0.0)
            {
                double progressNormalized = MathEx.MinMax(m_Progress, 0.0, 1.0);
                m_BorderProgress.Width = m_BorderProgressOuter.ActualWidth * progressNormalized;
                TimeSpan remaining = Interval * (1.0 - progressNormalized);
                if (m_ProgressTextFormat != null)
                {
                    LocalizationString progressText =
                        LocalizationString.Format(m_ProgressTextFormat, m_PageIndex + 1, m_PageCount, remaining);
                    m_TxtProgress.SetLocalizedText(progressText);
                }
                else m_TxtProgress.Text = string.Empty;
            }
        }

        public void Start()
        {
            m_StartTime = ExhibitApplication.Current.TimeInMs;
        }

        public void Stop()
        {
            m_StartTime = double.MaxValue;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            Update();
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (IsVisible && m_StartTime != double.MaxValue && Interval > TimeSpan.Zero)
            {
                Progress = MathEx.MinMax(
                    (ExhibitApplication.Current.TimeInMs - m_StartTime) / Interval.TotalMilliseconds, 0.0, 1.0);
                if (Progress >= 1.0)
                {
                    Stop();
                    OnExpired();
                }
            }
        }
    }
}
